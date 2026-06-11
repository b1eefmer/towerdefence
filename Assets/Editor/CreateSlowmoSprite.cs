using UnityEditor;
using UnityEngine;
using System.IO;

public static class CreateSlowmoSprite
{
    [MenuItem("Tools/Generate Slowmo Tower Sprite")]
    static void Generate()
    {
        const int Size = 64;
        var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false);

        var px = new Color[Size * Size];
        for (int i = 0; i < px.Length; i++) px[i] = Color.clear;
        tex.SetPixels(px);

        var center  = new Vector2(Size * 0.5f, Size * 0.5f);
        var gold    = new Color(0.831f, 0.659f, 0.263f, 1f);
        var darkBg  = new Color(0.082f, 0.075f, 0.165f, 1f);
        var deeper  = new Color(0.05f,  0.045f, 0.11f,  1f);
        var crystal = new Color(0.45f,  0.90f,  1.00f,  1f);
        var glow    = new Color(0.25f,  0.72f,  0.95f,  1f);
        var core    = new Color(0.85f,  0.97f,  1.00f,  1f);

        const float OuterR  = 30f;
        const float RingR   = 27f;
        const float InnerR  = 23f;
        const float CoreR   = 6f;
        const float ArmW    = 10f;  // arm half-angle degrees
        const float BarbW   = 5f;
        const float BarbMin = 12f;
        const float BarbMax = 18f;

        for (int y = 0; y < Size; y++)
        for (int x = 0; x < Size; x++)
        {
            float dx   = x - center.x + 0.5f;
            float dy   = y - center.y + 0.5f;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);

            if (dist > OuterR) continue;

            // Gold border ring
            if (dist > RingR) { tex.SetPixel(x, y, gold); continue; }

            // Dark base donut
            if (dist > InnerR) { tex.SetPixel(x, y, darkBg); continue; }

            // Inner area — snowflake crystal
            float angleDeg = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
            if (angleDeg < 0f) angleDeg += 360f;

            bool inArm  = false;
            bool inBarb = false;

            for (int arm = 0; arm < 6; arm++)
            {
                float armAngle = arm * 60f;
                float diff = Mathf.Abs(Mathf.DeltaAngle(angleDeg, armAngle));

                if (diff < ArmW) inArm = true;

                if (diff < BarbW && dist >= BarbMin && dist <= BarbMax)
                    inBarb = true;
            }

            if (dist <= CoreR)
                tex.SetPixel(x, y, core);
            else if (inBarb)
                tex.SetPixel(x, y, crystal);
            else if (inArm)
                tex.SetPixel(x, y, glow);
            else
                tex.SetPixel(x, y, deeper);
        }

        // Bright core centre pixel
        tex.SetPixel(Size / 2, Size / 2, core);
        tex.Apply();

        string assetPath = "Assets/art/Sprites/SlowmoTower.png";
        string fullPath  = Path.Combine(Application.dataPath, "../", assetPath);
        File.WriteAllBytes(fullPath, tex.EncodeToPNG());

        AssetDatabase.ImportAsset(assetPath);
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType        = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 32;
            importer.filterMode         = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
        }

        AssetDatabase.Refresh();

        // Assign to Slowmo Tower prefab
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (sprite == null) { Debug.LogError("Sprite not loaded — check import."); return; }

        string prefabPath = "Assets/art/Prefabs/Slowmo Tower.prefab";
        var prefabAsset   = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefabAsset == null) { Debug.LogError("Prefab not found."); return; }

        var renderer = prefabAsset.GetComponent<SpriteRenderer>();
        if (renderer == null) { Debug.LogError("No SpriteRenderer on prefab root."); return; }

        renderer.sprite = sprite;
        renderer.color  = new Color(0.45f, 0.90f, 1.00f, 1f);

        EditorUtility.SetDirty(prefabAsset);
        AssetDatabase.SaveAssets();

        Debug.Log("Slowmo Tower sprite created and assigned.");
    }
}
