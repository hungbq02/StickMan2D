using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// Đọc <id>_manifest.json do Tools/ArtPipeline/build.py sinh ra trong Art/Enemies/<Name>/:
// set import sprite (PPU, pivot đáy-giữa), tạo clip "{anim}_{Name}" + AnimatorController<Name>.
// Chạy lại an toàn: clip / controller / state đã có được cập nhật tại chỗ (giữ GUID).
public static class EnemyArtImporter
{
    private const string ArtRoot = "Assets/_GamePlay/Art/Enemies";
    private const string AnimRoot = "Assets/_GamePlay/Anim/Enemies";

    [System.Serializable]
    private class AnimEntry
    {
        public string name;
        public float fps = 10f;
        public bool loop;
        public string[] frames;
    }

    [System.Serializable]
    private class FxEntry
    {
        public string name;
        public string[] frames;
        public float pivotX = 0.5f;
        public float pivotY = 0.5f;
        public float pixelsPerUnit = 100f;
    }

    [System.Serializable]
    private class Manifest
    {
        public string id;
        public float pivotX = 0.5f;
        public float pivotY;
        public float pixelsPerUnit = 100f;
        public AnimEntry[] anims;
        public FxEntry[] fx;
    }

    [MenuItem("Tools/Enemies/Import Enemy Art")]
    public static void ImportAll()
    {
        foreach (string dir in Directory.GetDirectories(ArtRoot))
        {
            string name = Path.GetFileName(dir);
            string[] manifests = Directory.GetFiles(dir, "*_manifest.json");
            if (manifests.Length == 0) continue;
            Import(name, dir.Replace('\\', '/'), JsonUtility.FromJson<Manifest>(File.ReadAllText(manifests[0])));
        }
        AssetDatabase.SaveAssets();
    }

    private static void Import(string name, string dir, Manifest m)
    {
        // 1) import setting cho mọi frame (gom 1 lần reimport)
        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (AnimEntry a in m.anims)
                foreach (string f in a.frames)
                    SetupSprite($"{dir}/{f}", m.pixelsPerUnit, new Vector2(m.pivotX, m.pivotY));
            if (m.fx != null)
                foreach (FxEntry fx in m.fx)
                    foreach (string f in fx.frames)
                        SetupSprite($"{dir}/FX/{f}", fx.pixelsPerUnit, new Vector2(fx.pivotX, fx.pivotY));
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }

        // 2) clip + controller
        string animDir = $"{AnimRoot}/{name}";
        EnsureFolder(animDir);
        var clips = new List<AnimationClip>();
        foreach (AnimEntry a in m.anims)
            clips.Add(CreateClip($"{animDir}/{a.name}_{name}.anim", dir, a));

        string ctrlPath = $"{animDir}/Animator{name}.controller";
        var ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(ctrlPath) ?? AnimatorController.CreateAnimatorControllerAtPath(ctrlPath);
        AnimatorStateMachine sm = ctrl.layers[0].stateMachine;
        foreach (AnimationClip clip in clips)
        {
            AnimatorState state = FindState(sm, clip.name) ?? sm.AddState(clip.name);
            state.motion = clip;
            state.writeDefaultValues = false;
            if (clip.name.StartsWith("idle_"))
                sm.defaultState = state;
        }
        EditorUtility.SetDirty(ctrl);
        Debug.Log($"[EnemyArtImporter] {name}: {clips.Count} clip -> {ctrlPath}");
    }

    private static void SetupSprite(string path, float ppu, Vector2 pivot)
    {
        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null)
        {
            Debug.LogError($"[EnemyArtImporter] Không tìm thấy texture {path} (đã refresh Unity sau khi chạy build.py?)");
            return;
        }
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        var s = new TextureImporterSettings();
        ti.ReadTextureSettings(s);
        s.spriteAlignment = (int)SpriteAlignment.Custom;
        s.spritePivot = pivot;
        s.spritePixelsPerUnit = ppu;
        s.spriteMeshType = SpriteMeshType.Tight;
        s.alphaIsTransparency = true;
        s.mipmapEnabled = false;
        s.filterMode = FilterMode.Bilinear;
        s.wrapMode = TextureWrapMode.Clamp;
        ti.SetTextureSettings(s);
        ti.textureCompression = TextureImporterCompression.Compressed;
        ti.SaveAndReimport();
    }

    private static AnimationClip CreateClip(string path, string dir, AnimEntry a)
    {
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        bool isNew = clip == null;
        if (isNew) clip = new AnimationClip();
        clip.frameRate = a.fps;

        // thêm 1 key cuối giữ frame cuối đủ 1 nhịp => độ dài clip = số frame / fps
        var keys = new ObjectReferenceKeyframe[a.frames.Length + 1];
        for (int i = 0; i <= a.frames.Length; i++)
        {
            string f = a.frames[Mathf.Min(i, a.frames.Length - 1)];
            keys[i] = new ObjectReferenceKeyframe
            {
                time = i / a.fps,
                value = AssetDatabase.LoadAssetAtPath<Sprite>($"{dir}/{f}")
            };
        }
        var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = a.loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        if (isNew) AssetDatabase.CreateAsset(clip, path);
        else EditorUtility.SetDirty(clip);
        return clip;
    }

    private static AnimatorState FindState(AnimatorStateMachine sm, string stateName)
    {
        foreach (ChildAnimatorState s in sm.states)
            if (s.state.name == stateName) return s.state;
        return null;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }
}
