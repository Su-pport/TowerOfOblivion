using UnityEngine;
using UnityEditor;
using TMPro;

public class FontReplacer : EditorWindow
{
    public TMP_FontAsset newFont;

    [MenuItem("Tools/Replace TMP Fonts")]
    public static void ShowWindow()
    {
        GetWindow<FontReplacer>("Font Replacer");
    }

    private void OnGUI()
    {
        GUILayout.Label("프로젝트 전체 TMP 폰트 교체", EditorStyles.boldLabel);
        newFont = (TMP_FontAsset)EditorGUILayout.ObjectField("새 폰트", newFont, typeof(TMP_FontAsset), false);

        if (GUILayout.Button("모든 TMP 텍스트에 적용"))
        {
            ReplaceFonts();
        }
    }

    private void ReplaceFonts()
    {
        if (newFont == null)
        {
            Debug.LogWarning("새 폰트를 지정하세요!");
            return;
        }

        // 씬에 있는 모든 TMP 텍스트 교체
        foreach (var tmp in Resources.FindObjectsOfTypeAll<TextMeshProUGUI>())
        {
            tmp.font = newFont;
            EditorUtility.SetDirty(tmp);
        }

        // 프리팹에 있는 TMP 텍스트 교체
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            foreach (var tmp in prefab.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                tmp.font = newFont;
                EditorUtility.SetDirty(prefab);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("모든 TMP 텍스트에 새 폰트 적용 완료!");
    }
}
