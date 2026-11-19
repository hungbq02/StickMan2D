using UnityEditor;

// Android: mặc định Unity 2022.3 nén ETC2 => nét mảnh / viền trong suốt của sprite bị vỡ khối 4×4, lệch màu.
// Ép ASTC 4×4 (cùng 8 bit/pixel như ETC2 RGBA8, nét sạch hơn) cho mọi texture trong Art/ — kể cả art sinh mới sau này.
// Bỏ qua Art/EnemyConcepts (ảnh concept gốc, không vào game) và texture đã override Android bằng tay.
public class AndroidTextureCompression : AssetPostprocessor
{
    private const string ArtRoot = "Assets/_GamePlay/Art/";

    public override uint GetVersion() => 1;

    private void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith(ArtRoot) || assetPath.Contains("/EnemyConcepts/")) return;

        var importer = (TextureImporter)assetImporter;
        TextureImporterPlatformSettings android = importer.GetPlatformTextureSettings("Android");
        if (android.overridden) return;

        android.overridden = true;
        android.format = TextureImporterFormat.ASTC_4x4;
        android.maxTextureSize = importer.maxTextureSize;
        importer.SetPlatformTextureSettings(android);
    }
}
