using UnityEngine;

[CreateAssetMenu(menuName = "AI/Guard/States/Patrol")]
public class PatrolStateAsset : StateAsset
{
    public enum Mode { Loop, PingPong, Once }

    [Header("Transitions")]
    public StateAsset chaseState;

    [Header("Patrol")]
    public Mode mode = Mode.Loop;
    public float arriveTolerance = 0.08f;
    public float pauseAtPoint = 0.3f;
    public bool startFromClosest = true;

    public override IState CreateRuntime(GuardBrain brain) => new PatrolRuntime(brain, this);

    private class PatrolRuntime : IState
    {
        private readonly GuardBrain brain;
        private readonly PatrolStateAsset asset;
        private int index;
        private int dir = 1;
        private float waitTimer;

        public PatrolRuntime(GuardBrain brain, PatrolStateAsset asset)
        {
            this.brain = brain;
            this.asset = asset;
        }

        public void Enter()
        {
            if (brain.Ctx.Path == null || brain.Ctx.Path.Count == 0) return;
            index = asset.startFromClosest ? FindClosestIndex() : 0;
            waitTimer = 0f;
        }

        public void Tick()
        {
            var ctx = brain.Ctx;

            if (ctx.Detector != null && ctx.Detector.IsSpotted)
            {
                brain.Switch(asset.chaseState);
                return;
            }

            if (ctx.Path == null || ctx.Path.Count == 0) { ctx.Mover.Stop(); return; }

            if (waitTimer > 0f) { waitTimer -= Time.deltaTime; return; }

            var target = (Vector2)ctx.Path.GetPoint(index).position;
            bool arrived = ctx.Mover.MoveTowards(target, ctx.Config.patrolSpeed, asset.arriveTolerance);
            FaceTowards(target, ctx.Self);

            if (arrived)
            {
                waitTimer = asset.pauseAtPoint;
                AdvanceIndex();
            }
        }

        public void Exit() { }

        private int FindClosestIndex()
        {
            int best = 0; float bestD = float.MaxValue;
            var pos = brain.Ctx.Self.position;
            for (int i = 0; i < brain.Ctx.Path.Count; i++)
            {
                var p = brain.Ctx.Path.GetPoint(i);
                if (!p) continue;
                float d = (p.position - pos).sqrMagnitude;
                if (d < bestD) { bestD = d; best = i; }
            }
            return best;
        }

        private void AdvanceIndex()
        {
            var count = brain.Ctx.Path.Count;
            switch (asset.mode)
            {
                case Mode.Loop:
                    index = (index + 1) % count;
                    break;
                case Mode.Once:
                    if (index < count - 1) index++;
                    break;
                case Mode.PingPong:
                    int next = index + dir;
                    if (next >= count || next < 0) { dir *= -1; next = index + dir; }
                    index = next;
                    break;
            }
        }

        private static void FaceTowards(Vector2 target, Transform self)
        {
            var dir = (target - (Vector2)self.position).normalized;
            if (dir.sqrMagnitude > 1e-6f) self.right = new Vector3(dir.x, dir.y, 0f);
        }
    }
}
