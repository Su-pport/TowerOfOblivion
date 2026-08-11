#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Item), true)] // true → 상속받은 클래스에도 적용
public class ItemEditor : Editor
{
    void OnEnable()
    {
        Item item = (Item)target;
        if (item.icon != null)
        {
            Texture2D tex = item.icon.texture;
            if (tex != null)
                EditorGUIUtility.SetIconForObject(item, tex);
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Item item = (Item)target;
        if (item.icon != null)
        {
            GUILayout.Label("아이콘 미리보기:");
            GUILayout.Label(item.icon.texture, GUILayout.Width(64), GUILayout.Height(64));
        }
    }
}
#endif
