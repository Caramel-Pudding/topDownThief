using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class NoisePulse : MonoBehaviour
{
    public class Config
    {
        public float MaxRadius;
        public float ExpandSpeed;
        public float LifeAfterReach;
        public LayerMask ObstacleMask;
        public float NotifyInterval;
        public int RaySamples;
        public float RingWidth;
        public Material RingMaterial;
    }

    private Config cfg;
    private LineRenderer line;
    private float radius;
    private float lifeTimer;
    private float notifyTimer;
    private Vector2 origin;
    private readonly Collider2D[] buf = new Collider2D[64];
    private readonly HashSet<Component> notified = new HashSet<Component>();

    public void Init(Config c)
    {
        cfg = c;
        origin = transform.position;

        line = GetComponent<LineRenderer>();
        line.loop = true;
        line.useWorldSpace = true;
        line.widthMultiplier = cfg.RingWidth;
        line.positionCount = Mathf.Max(8, cfg.RaySamples);
        if (cfg.RingMaterial) line.material = cfg.RingMaterial;

        // If no material color property, fall back to start/end colors.
        if (!cfg.RingMaterial || !cfg.RingMaterial.HasProperty("_BaseColor"))
        {
            line.startColor = new Color(1,1,1,1);
            line.endColor   = new Color(1,1,1,1);
        }

        lifeTimer = cfg.LifeAfterReach;
    }

    void Update()
    {
        // Expand
        if (radius < cfg.MaxRadius)
        {
            radius = Mathf.Min(cfg.MaxRadius, radius + cfg.ExpandSpeed * Time.deltaTime);
            DrawClippedRing();
        }
        else
        {
            lifeTimer -= Time.deltaTime;
            FadeOutByLifeTimer();
            if (lifeTimer <= 0f) Destroy(gameObject);
        }

        // Notify periodically
        notifyTimer -= Time.deltaTime;
        if (notifyTimer <= 0f)
        {
            notifyTimer = cfg.NotifyInterval;
            NotifyListeners();
        }
    }

    private void DrawClippedRing()
    {
        int n = line.positionCount;
        float step = 360f / n;
        for (int i = 0; i < n; i++)
        {
            float ang = step * i * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
            float dist = radius;

            var hit = Physics2D.Raycast(origin, dir, radius, cfg.ObstacleMask);
            if (hit.collider) dist = hit.distance;

            line.SetPosition(i, origin + dir * dist);
        }
    }

    private void FadeOutByLifeTimer()
    {
        if (cfg.LifeAfterReach <= 0f) return;
        float t = Mathf.Clamp01(lifeTimer / cfg.LifeAfterReach);

        if (line.material && line.material.HasProperty("_BaseColor"))
        {
            var col = line.material.GetColor("_BaseColor");
            col.a = t;
            line.material.SetColor("_BaseColor", col);
        }
        else
        {
            var sc = line.startColor; sc.a = t;
            var ec = line.endColor;   ec.a = t;
            line.startColor = sc; line.endColor = ec;
        }
    }

    private void NotifyListeners()
    {
        int count = Physics2D.OverlapCircleNonAlloc(origin, radius, buf);
        var ev = new NoiseEvent
        {
            Origin = origin,
            CurrentRadius = radius,
            MaxRadius = cfg.MaxRadius,
            Timestamp = Time.time
        };

        for (int i = 0; i < count; i++)
        {
            var col = buf[i];
            if (!col) continue;

            // Get listeners
            var listeners = col.GetComponents<INoiseListener>();
            if (listeners == null || listeners.Length == 0) continue;

            // LOS check: if blocked by obstacles, skip
            Vector2 to = (Vector2)col.bounds.center - origin;
            float dist = Mathf.Max(0.01f, to.magnitude);
            var blocked = Physics2D.Raycast(origin, to.normalized, dist, cfg.ObstacleMask);
            if (blocked.collider) continue;

            // Notify once per component
            foreach (var l in listeners)
            {
                var comp = l as Component;
                if (comp && notified.Add(comp))
                {
                    // Optional strength based on distance
                    ev.Strength = 1f - Mathf.Clamp01(dist / Mathf.Max(ev.MaxRadius, 0.001f));
                    l.OnNoiseHeard(ev);
                }
            }
        }
    }
}
