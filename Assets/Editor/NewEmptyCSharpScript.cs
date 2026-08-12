#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class ItemDatabaseWindow : EditorWindow
{
    [MenuItem("Window/Item Database")]
    public static void ShowWindow()
    {
        GetWindow<ItemDatabaseWindow>("Item Database");
    }

    void OnGUI()
    {
        // 모든 Item ScriptableObject 찾기
        var items = Resources.FindObjectsOfTypeAll<Item>();

        foreach (var item in items)
        {
            GUILayout.BeginHorizontal();

            // 아이콘 표시
            if (item.icon != null)
                GUILayout.Label(item.icon.texture, GUILayout.Width(32), GUILayout.Height(32));

            // 버튼으로 아이템 선택
            if (GUILayout.Button(item.itemName))
            {
                // 프로젝트 뷰에서 해당 아이템 선택
                Selection.activeObject = item;
                EditorGUIUtility.PingObject(item); // 프로젝트 뷰에서 반짝 표시
            }

            GUILayout.EndHorizontal();
        }
    }
}
#endif
