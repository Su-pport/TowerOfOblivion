using UnityEngine;

public class TowerFloor : MonoBehaviour
{
    [Header("Floor")]
    [SerializeField] private int floorNumber = 1;

    [Header("Floor Components")]
    [SerializeField] private SpawnPoint spawnPoint;
    [SerializeField] private Map map;

    public int FloorNumber => floorNumber;
    public SpawnPoint SpawnPoint => spawnPoint;
    public Map Map => map;

    // Ensure the serialized `map` reference is valid at runtime.
    // Attempts to find a `Map` in children; if none exists, add one so
    // runtime generation can proceed even when the author removed the
    // child Map/Tilemap objects in the Hierarchy.
    public void EnsureMapReference()
    {
        if (map != null)
            return;

        map = GetComponentInChildren<Map>(true);
        if (map == null)
        {
            // Only add a Map component when this object is a scene instance.
            if (gameObject.scene.IsValid())
            {
                map = gameObject.AddComponent<Map>();
                Debug.Log("TowerFloor: Map component was missing; added at runtime on " + gameObject.name, this);
            }
            else
            {
                Debug.LogWarning("TowerFloor: Map component missing but object is a Prefab asset; skipping add on " + gameObject.name, this);
            }
        }
    }

    // Assign a Map instance to this TowerFloor (used when creating a
    // runtime Map instance to replace a prefab asset reference).
    public void AssignMap(Map m)
    {
        map = m;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (spawnPoint == null)
            spawnPoint = GetComponentInChildren<SpawnPoint>(true);

        if (map == null)
            map = GetComponentInChildren<Map>(true);
    }
#endif
}