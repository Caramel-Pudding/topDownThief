#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public static class LockpickingSpriteGenerator
{
    [MenuItem("Tools/Lockpicking/Generate Sprites")]
    public static void Generate()
    {
        // ---- Master parameters ----
        const int size = 256;         // texture size in pixels (square)
        const int ringThickness = 36; // ring thickness in pixels
        const int ppu = 100;          // pixels per unit for import

        // Tick layout
        const int majorCount = 12;        // number of major ticks around the circle
        const int minorPerMajor = 5;      // minor ticks per major sector (e.g., 5 -> 60 total minor, majors at each 5th)
        const int majorWidthPx = 4;       // visual width of a major tick (orthogonal to radius), in pixels
        const int minorWidthPx = 2;       // visual width of a minor tick, in pixels
        const int majorLenPx = -1;        // radial length of major tick in pixels; -1 -> 80% of ring thickness
        const int minorLenPx = -1;        // radial length of minor tick; -1 -> 50% of ring thickness
        const float startAngleDeg = 0f;   // rotation offset for tick zero (0° = up)

        string dir = "Assets/Lockpicking/Sprites";
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        // Base sprites
        var disk = MakeDisk(size);
        var ring = MakeRing(size, ringThickness);

        // Ticks
        var ticks = MakeTicks(
            size, ringThickness,
            majorCount, minorPerMajor,
            majorWidthPx, minorWidthPx,
            majorLenPx, minorLenPx,
            startAngleDeg
        );

        string diskPath  = Path.Combine(dir, "disk_full.png").Replace("\\", "/");
        string ringPath  = Path.Combine(dir, "wheel_ring.png").Replace("\\", "/");
        string ticksPath = Path.Combine(dir, "ticks_ring.png").Replace("\\", "/");

        File.WriteAllBytes(diskPath,  disk.EncodeToPNG());
        File.WriteAllBytes(ringPath,  ring.EncodeToPNG());
        File.WriteAllBytes(ticksPath, ticks.EncodeToPNG());

        AssetDatabase.ImportAsset(diskPath);
        AssetDatabase.ImportAsset(ringPath);
        AssetDatabase.ImportAsset(ticksPath);

        SetupSpriteImporter(diskPath,  ppu);
        SetupSpriteImporter(ringPath,  ppu);
        SetupSpriteImporter(ticksPath, ppu);

        Debug.Log($"Generated:\n  {diskPath}\n  {ringPath}\n  {ticksPath}");
    }

    private static Texture2D MakeDisk(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        float cx = (size - 1) * 0.5f;
        float cy = (size - 1) * 0.5f;
        float r  = size * 0.5f;
        float r2 = r * r;

        var pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dx = x - cx;
            float dy = y - cy;
            float d2 = dx * dx + dy * dy;
            pixels[y * size + x] = d2 <= r2 ? Color.white : new Color(0,0,0,0);
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    private static Texture2D MakeRing(int size, int thickness)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        float cx = (size - 1) * 0.5f;
        float cy = (size - 1) * 0.5f;
        float rOuter  = size * 0.5f;
        float rInner  = Mathf.Max(1, rOuter - thickness);
        float rOuter2 = rOuter * rOuter;
        float rInner2 = rInner * rInner;

        var pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dx = x - cx;
            float dy = y - cy;
            float d2 = dx * dx + dy * dy;
            bool inside = d2 <= rOuter2 && d2 >= rInner2;
            pixels[y * size + x] = inside ? Color.white : new Color(0,0,0,0);
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    private struct Tick
    {
        public float angleDeg; // tick direction angle (0° = up)
        public float halfDeg;  // half of angular width in degrees
        public int lenPx;      // radial length in pixels
    }

    private static Texture2D MakeTicks(
        int size, int ringThickness,
        int majorCount, int minorPerMajor,
        int majorWidthPx, int minorWidthPx,
        int majorLenPx, int minorLenPx,
        float startAngleDeg
    )
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        float cx = (size - 1) * 0.5f;
        float cy = (size - 1) * 0.5f;
        float rOuter = size * 0.5f;

        if (majorLenPx < 0) majorLenPx = Mathf.Max(1, Mathf.RoundToInt(ringThickness * 0.8f));
        if (minorLenPx < 0) minorLenPx = Mathf.Max(1, Mathf.RoundToInt(ringThickness * 0.5f));

        float rMidMajor = rOuter - majorLenPx * 0.5f;
        float rMidMinor = rOuter - minorLenPx * 0.5f;

        float majorHalfDeg = Mathf.Rad2Deg * Mathf.Atan2(majorWidthPx * 0.5f, Mathf.Max(1f, rMidMajor));
        float minorHalfDeg = Mathf.Rad2Deg * Mathf.Atan2(minorWidthPx * 0.5f, Mathf.Max(1f, rMidMinor));

        var ticks = new List<Tick>();

        // Major ticks
        for (int i = 0; i < Mathf.Max(1, majorCount); i++)
        {
            float ang = startAngleDeg + i * (360f / majorCount);
            ticks.Add(new Tick { angleDeg = Normalize360(ang), halfDeg = majorHalfDeg, lenPx = majorLenPx });
        }

        // Minor ticks (skip angles that coincide with major)
        int totalMinor = Mathf.Max(0, majorCount * Mathf.Max(1, minorPerMajor));
        float minorStep = 360f / Mathf.Max(1, totalMinor);
        for (int i = 0; i < totalMinor; i++)
        {
            // If this index hits a major tick position, skip
            if (minorPerMajor > 0 && (i % minorPerMajor) == 0) continue;

            float ang = startAngleDeg + i * minorStep;
            ticks.Add(new Tick { angleDeg = Normalize360(ang), halfDeg = minorHalfDeg, lenPx = minorLenPx });
        }

        var pixels = new Color[size * size];
        Color transparent = new Color(0, 0, 0, 0);

        // Precompute outer bounds for radial check to prune work
        int maxLen = Mathf.Max(majorLenPx, minorLenPx);
        float rMin = rOuter - maxLen - 1;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - cx;
                float dy = y - cy;
                float r = Mathf.Sqrt(dx * dx + dy * dy);

                // Only draw near outer ring
                if (r < rMin || r > rOuter + 0.5f)
                {
                    pixels[y * size + x] = transparent;
                    continue;
                }

                float angleDeg = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
                angleDeg = Normalize360(angleDeg + 90f); // make 0° point upwards

                bool hit = false;
                for (int t = 0; t < ticks.Count; t++)
                {
                    float dAng = Mathf.Abs(Mathf.DeltaAngle(angleDeg, ticks[t].angleDeg));
                    if (dAng <= ticks[t].halfDeg)
                    {
                        float rStart = rOuter - ticks[t].lenPx;
                        if (r >= rStart && r <= rOuter) { hit = true; break; }
                    }
                }

                pixels[y * size + x] = hit ? Color.white : transparent;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    private static float Normalize360(float deg)
    {
        deg %= 360f;
        if (deg < 0f) deg += 360f;
        return deg;
    }

    private static void SetupSpriteImporter(string path, int ppu)
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = ppu;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.SaveAndReimport();
    }
}
#endif
