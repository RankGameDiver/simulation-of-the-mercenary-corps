using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

// TMP 폰트 애셋을 Static 아틀라스로 굽는다.
//
// Dynamic 모드에서는 런타임에 처음 보는 글자가 나올 때마다 TMP가 아틀라스에
// 글리프를 추가하고 애셋 파일을 다시 쓴다. 그래서 폰트를 건드리지 않아도
// 에디터를 실행할 때마다 .asset이 변경되어 git diff가 계속 생긴다.
// 필요한 글자를 미리 다 구워두고 Static으로 고정하면 파일이 더 이상 변하지 않는다.
public static class StaticFontAssetBaker
{
    // 대상 폰트 애셋 (씬/프리팹 참조를 지키기 위해 이 파일을 제자리에서 갱신한다)
    private const string TargetFontAssetPath = "Assets/Fonts/NotoSansKR SDF.asset";

    // 베이크 설정. 품질이나 용량을 조정하려면 여기만 바꾸고 다시 실행하면 된다.
    private const int SamplingPointSize = 48;
    private const int AtlasPadding = 5;
    private const int AtlasWidth = 4096;
    private const int AtlasHeight = 4096;
    private const GlyphRenderMode RenderMode = GlyphRenderMode.SDFAA;

    [MenuItem("Tools/Font/Static SDF 베이크 (NotoSansKR)")]
    public static void Bake() {
        var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TargetFontAssetPath);
        if(existing == null) {
            Debug.LogError($"[StaticFontAssetBaker] 폰트 애셋을 찾을 수 없습니다: {TargetFontAssetPath}");
            return;
        }

        var sourceFont = existing.sourceFontFile;
        if(sourceFont == null) {
            // Static으로 이미 전환된 애셋은 sourceFontFile이 비어 있을 수 있어 GUID로 되짚는다.
            var guid = new SerializedObject(existing).FindProperty("m_SourceFontFileGUID").stringValue;
            var path = AssetDatabase.GUIDToAssetPath(guid);
            sourceFont = AssetDatabase.LoadAssetAtPath<Font>(path);
        }
        if(sourceFont == null) {
            Debug.LogError("[StaticFontAssetBaker] 원본 TTF를 찾을 수 없습니다. 애셋의 Source Font File을 확인하세요.");
            return;
        }

        var unicodes = BuildCharacterSet();
        Debug.Log($"[StaticFontAssetBaker] {sourceFont.name} / {unicodes.Length}자 베이크 시작...");

        // 1. 새 애셋을 메모리에 만들어 글리프를 모두 굽는다.
        //    (Dynamic으로 만들어야 TryAddCharacters가 아틀라스를 채운다)
        var baked = TMP_FontAsset.CreateFontAsset(
            sourceFont, SamplingPointSize, AtlasPadding, RenderMode,
            AtlasWidth, AtlasHeight, AtlasPopulationMode.Dynamic, enableMultiAtlasSupport: true);

        if(baked == null) {
            Debug.LogError("[StaticFontAssetBaker] 폰트 애셋 생성에 실패했습니다.");
            return;
        }

        // 한글은 커닝이 거의 없고, 1만 자 분량의 폰트 피처는 파일만 크게 만든다.
        baked.TryAddCharacters(unicodes, out uint[] missing, includeFontFeatures: false);
        if(missing != null && missing.Length > 0) {
            Debug.LogWarning($"[StaticFontAssetBaker] 원본 폰트에 없어 건너뛴 글자 {missing.Length}자 " +
                             $"(예: {string.Join(", ", missing.Take(10).Select(u => $"U+{u:X4}"))})");
        }

        baked.atlasPopulationMode = AtlasPopulationMode.Static;
        baked.isMultiAtlasTexturesEnabled = false; // Static에서는 런타임 확장을 막는다
        baked.ReadFontAssetDefinition();

        // 2. 기존 파일의 텍스처 서브애셋을 걷어낸다.
        //    머티리얼은 씬이 fileID로 참조하므로 반드시 살려둔다.
        var survivingMaterial = existing.material;
        foreach(var sub in AssetDatabase.LoadAllAssetRepresentationsAtPath(TargetFontAssetPath)) {
            if(sub is Texture2D) {
                AssetDatabase.RemoveObjectFromAsset(sub);
                Object.DestroyImmediate(sub, true);
            }
        }

        // 3. 구운 결과를 기존 오브젝트에 덮어써서 메인 애셋의 fileID/GUID를 유지한다.
        //    CopySerialized는 m_Name까지 덮어쓰는데 임시 객체에는 이름이 없다.
        //    기존 객체의 이름을 재사용하면 한 번 비워졌을 때 복구가 안 되므로 파일 경로에서 가져온다.
        var assetName = Path.GetFileNameWithoutExtension(TargetFontAssetPath);
        var bakedMaterial = baked.material;
        EditorUtility.CopySerialized(baked, existing);
        existing.name = assetName;

        // 4. 새 아틀라스 텍스처를 서브애셋으로 붙인다.
        //    (CopySerialized로 existing.atlasTextures가 baked와 같은 객체를 가리키게 된 상태)
        //    TMP는 아틀라스 배열을 여유 있게 잡아 두므로 뒤쪽 빈 칸을 잘라낸다.
        var textures = existing.atlasTextures.Where(t => t != null).ToArray();
        for(int i = 0; i < textures.Length; i++) {
            textures[i].name = $"{existing.name} Atlas {i}";
            AssetDatabase.AddObjectToAsset(textures[i], existing);
        }
        existing.atlasTextures = textures;

        // 5. 머티리얼은 새로 만들지 않고 기존 객체에 내용만 복사해 참조를 보존한다.
        if(survivingMaterial != null) {
            // TMP 버전에 따라 CreateFontAsset이 머티리얼을 만들지 않을 수도 있다.
            if(bakedMaterial != null) EditorUtility.CopySerialized(bakedMaterial, survivingMaterial);
            survivingMaterial.name = $"{existing.name} Material";
            survivingMaterial.SetTexture(ShaderUtilities.ID_MainTex, existing.atlasTexture);
            survivingMaterial.SetFloat(ShaderUtilities.ID_TextureWidth, existing.atlasWidth);
            survivingMaterial.SetFloat(ShaderUtilities.ID_TextureHeight, existing.atlasHeight);
            survivingMaterial.SetFloat(ShaderUtilities.ID_GradientScale, AtlasPadding + 1);

            var so = new SerializedObject(existing);
            so.FindProperty("m_Material").objectReferenceValue = survivingMaterial;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(survivingMaterial);
        }

        // 6. 임시 객체 정리. 텍스처는 이미 애셋이 소유했으므로 건드리지 않는다.
        if(bakedMaterial != null) Object.DestroyImmediate(bakedMaterial);
        Object.DestroyImmediate(baked);

        EditorUtility.SetDirty(existing);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[StaticFontAssetBaker] 완료 — 글리프 {existing.glyphTable.Count}자, " +
                  $"문자 {existing.characterTable.Count}자, " +
                  $"아틀라스 {existing.atlasTextures.Length}장 ({AtlasWidth}x{AtlasHeight}), " +
                  $"모드 {existing.atlasPopulationMode}");
    }

    // 구울 문자 집합을 만든다.
    private static uint[] BuildCharacterSet() {
        var set = new HashSet<uint>();

        void AddRange(uint from, uint to) {
            for(uint u = from; u <= to; u++) set.Add(u);
        }

        AddRange(0x0020, 0x007E); // ASCII 인쇄 가능 문자
        AddRange(0x00A1, 0x00FF); // 라틴-1 보충 (·, ×, ° 등)
        AddRange(0x2010, 0x2027); // 대시, 따옴표, 말줄임표
        AddRange(0x2030, 0x205E); // ‰, †, 각종 문장부호
        AddRange(0x2190, 0x21BB); // 화살표
        AddRange(0x2200, 0x22FF); // 수학 기호
        AddRange(0x2460, 0x24FF); // 원문자 ①②③
        AddRange(0x2500, 0x257F); // 괘선 문자
        AddRange(0x25A0, 0x25FF); // 도형 ■●▲▼
        AddRange(0x2600, 0x26FF); // 기타 기호 ★☆♠♥
        AddRange(0x3000, 0x303F); // CJK 문장부호 「」『』〈〉
        AddRange(0x3131, 0x318E); // 한글 호환 자모 (ㄱㄴㄷ, ㅏㅑㅓ)
        AddRange(0xAC00, 0xD7A3); // 한글 완성형 전체 11,172자
        AddRange(0xFF01, 0xFF5E); // 전각 영숫자/기호

        var result = set.ToArray();
        System.Array.Sort(result);
        return result;
    }
}
