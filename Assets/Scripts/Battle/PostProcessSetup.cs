using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class PostProcessSetup
{
    [MenuItem("DarkFant/Setup Battle Post-Processing")]
    public static void Setup()
    {
        // ── 1. Volume Profile asset ───────────────────────────────
        string profilePath = "Assets/Settings/BattleVolumeProfile.asset";

        VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, profilePath);
        }

        // Bloom
        if (!profile.TryGet<Bloom>(out var bloom))
            bloom = profile.Add<Bloom>(false);
        bloom.active      = true;
        bloom.intensity.Override(1.4f);
        bloom.threshold.Override(0.85f);
        bloom.scatter.Override(0.65f);

        // Vignette
        if (!profile.TryGet<Vignette>(out var vignette))
            vignette = profile.Add<Vignette>(false);
        vignette.active      = true;
        vignette.intensity.Override(0.38f);
        vignette.smoothness.Override(0.5f);
        vignette.color.Override(new Color(0.05f, 0f, 0.08f)); // violet très sombre

        // Color Adjustments (légère teinte rouge-sang)
        if (!profile.TryGet<ColorAdjustments>(out var ca))
            ca = profile.Add<ColorAdjustments>(false);
        ca.active = true;
        ca.postExposure.Override(-0.25f);
        ca.colorFilter.Override(new Color(1f, 0.82f, 0.72f)); // chaud/rouge doux

        EditorUtility.SetDirty(profile);
        AssetDatabase.SaveAssets();

        // ── 2. GlobalVolume GameObject ────────────────────────────
        GameObject go = GameObject.Find("GlobalVolume");
        if (go == null)
            go = new GameObject("GlobalVolume");

        var vol = go.GetComponent<Volume>();
        if (vol == null) vol = go.AddComponent<Volume>();
        vol.isGlobal = true;
        vol.priority = 1f;
        vol.profile  = profile;

        // ── 3. Post Processing sur la Main Camera ─────────────────
        var cam = Camera.main;
        if (cam != null)
        {
            var data = cam.GetComponent<UniversalAdditionalCameraData>();
            if (data == null) data = cam.gameObject.AddComponent<UniversalAdditionalCameraData>();
            data.renderPostProcessing = true;
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        Debug.Log("[DarkFant] Post-Processing setup: Bloom + Vignette + ColorAdjustments ✓");
    }
}
