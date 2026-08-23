using UnityEditor;
using UnityEngine;

// Editor utilities to create Tilemap structure and link TowerManager
public static class MapEditorUtilities
{
    [MenuItem("Tools/Map/Create Tilemaps For Selected Floor")]
    private static void CreateTilemapsForSelectedFloor()
    {
        if (Selection.activeGameObject == null)
        {
            Debug.LogWarning("선택된 GameObject가 없습니다.");
            return;
        }

        // Prevent running on a prefab asset selected in Project view.
        if (!Selection.activeGameObject.scene.IsValid())
        {
            Debug.LogWarning("씬의 Hierarchy에서 TowerFloor GameObject를 선택한 상태에서 실행하세요. (프리팹 에셋에는 적용할 수 없습니다.)");
            return;
        }

        var floorGO = Selection.activeGameObject;
        Undo.RegisterFullObjectHierarchyUndo(floorGO, "Create Tilemaps");

        Transform gridT = floorGO.transform.Find("Grid");
        GameObject gridGO;
        if (gridT == null)
        {
            gridGO = new GameObject("Grid");
            gridGO.transform.SetParent(floorGO.transform, false);
            gridGO.AddComponent<Grid>();
        }
        else
        {
            gridGO = gridT.gameObject;
        }

        void CreateChild(string name)
        {
            if (gridGO.transform.Find(name) != null)
                return;
            var go = new GameObject(name);
            go.transform.SetParent(gridGO.transform, false);
            go.AddComponent<UnityEngine.Tilemaps.Tilemap>();
            go.AddComponent<UnityEngine.Tilemaps.TilemapRenderer>();
            Undo.RegisterCreatedObjectUndo(go, "Create Tilemap Child");
        }

        CreateChild("BackgroundTilemap");
        CreateChild("FloorTilemap");
        CreateChild("WallRenderTilemap");

        // Ensure Map component exists
        var map = floorGO.GetComponentInChildren<Map>(true);
        if (map == null)
        {
            var mapComp = floorGO.AddComponent<Map>();
            Undo.RegisterCreatedObjectUndo(mapComp, "Add Map Component");
            Debug.Log("Map component added to " + floorGO.name);
        }
        else
        {
            Debug.Log("Map component already exists under " + floorGO.name);
        }
    }

    [MenuItem("Tools/Map/Auto Link TowerManager")]
    private static void AutoLinkTowerManager()
    {
        var manager = Object.FindObjectOfType<TowerManager>();
        if (manager == null)
        {
            Debug.LogWarning("씬에 TowerManager가 없습니다.");
            return;
        }

        var all = Resources.FindObjectsOfTypeAll<TowerFloor>();
        var floors = System.Array.FindAll(all, f => f != null && f.gameObject != null && f.gameObject.scene.IsValid());
        if (floors == null || floors.Length == 0)
        {
            Debug.LogWarning("씬에 TowerFloor가 없습니다.");
            return;
        }

        SerializedObject so = new SerializedObject(manager);
        var floorsProp = so.FindProperty("floors");
        floorsProp.arraySize = floors.Length;
        for (int i = 0; i < floors.Length; i++)
            floorsProp.GetArrayElementAtIndex(i).objectReferenceValue = floors[i];

        var startProp = so.FindProperty("startingFloor");
        if (startProp != null && startProp.objectReferenceValue == null)
            startProp.objectReferenceValue = floors[0];

        so.ApplyModifiedProperties();
        Debug.Log($"TowerManager linked: {floors.Length} floors assigned.");
        EditorUtility.SetDirty(manager);
    }
}
