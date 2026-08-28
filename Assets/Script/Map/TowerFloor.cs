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


    // ============================================================
    // Unity Lifecycle
    // ============================================================

    private void Awake()
    {
        // Debug.Log(
        //     "TowerFloor Awake: " +
        //     "name=" + gameObject.name +
        //     ", floorNumber=" + floorNumber +
        //     ", activeSelf=" + gameObject.activeSelf +
        //     ", activeInHierarchy=" + gameObject.activeInHierarchy +
        //     ", sceneValid=" + gameObject.scene.IsValid() +
        //     ", spawnPoint=" + (spawnPoint != null ? spawnPoint.name : "NULL") +
        //     ", map=" + (map != null ? map.name : "NULL"),
        //     this
        // );
    }

    private void OnEnable()
    {
        // Debug.Log(
        //     "TowerFloor OnEnable: " +
        //     "name=" + gameObject.name +
        //     ", floorNumber=" + floorNumber +
        //     ", activeSelf=" + gameObject.activeSelf +
        //     ", activeInHierarchy=" + gameObject.activeInHierarchy +
        //     ", spawnPoint=" + (spawnPoint != null ? spawnPoint.name : "NULL") +
        //     ", map=" + (map != null ? map.name : "NULL"),
        //     this
        // );
    }

    private void OnDisable()
    {
        Debug.Log(
            "TowerFloor OnDisable: " +
            "name=" + gameObject.name +
            ", floorNumber=" + floorNumber +
            ", activeSelf=" + gameObject.activeSelf +
            ", activeInHierarchy=" + gameObject.activeInHierarchy,
            this
        );
    }


    // ============================================================
    // Map Reference
    // ============================================================

    /// <summary>
    /// Ensure the serialized Map reference is valid at runtime.
    /// Attempts to find a Map in children.
    /// If none exists, adds one to this TowerFloor.
    /// </summary>
    public void EnsureMapReference()
    {
        // Debug.Log(
        //     "TowerFloor.EnsureMapReference START: " +
        //     "floor=" + floorNumber +
        //     ", name=" + gameObject.name +
        //     ", currentMap=" + (map != null ? map.name : "NULL") +
        //     ", active=" + gameObject.activeInHierarchy +
        //     ", sceneValid=" + gameObject.scene.IsValid(),
        //     this
        // );

        // 이미 Map이 연결되어 있는 경우
        if (map != null)
        {
            // Debug.Log(
            //     "TowerFloor.EnsureMapReference: " +
            //     "Map reference already exists. " +
            //     "floor=" + floorNumber +
            //     ", map=" + map.name +
            //     ", mapObject=" + map.gameObject.name +
            //     ", mapActive=" + map.gameObject.activeInHierarchy +
            //     ", mapEnabled=" + map.enabled,
            //     this
            // );

            return;
        }

        // Debug.Log(
        //     "TowerFloor.EnsureMapReference: " +
        //     "Map reference is NULL. Searching child objects... " +
        //     "floor=" + floorNumber +
        //     ", name=" + gameObject.name,
        //     this
        // );

        // 자식에서 Map 검색
        map = GetComponentInChildren<Map>(true);

        if (map != null)
        {
            Debug.Log(
                "TowerFloor.EnsureMapReference: " +
                "Found Map in children. " +
                "floor=" + floorNumber +
                ", map=" + map.name +
                ", mapObject=" + map.gameObject.name +
                ", mapActive=" + map.gameObject.activeInHierarchy +
                ", mapEnabled=" + map.enabled,
                this
            );

            return;
        }

        Debug.LogWarning(
            "TowerFloor.EnsureMapReference: " +
            "No Map found in children. " +
            "floor=" + floorNumber +
            ", name=" + gameObject.name +
            ", sceneValid=" + gameObject.scene.IsValid(),
            this
        );

        // 씬에 존재하는 오브젝트인 경우에만 Map 추가
        if (gameObject.scene.IsValid())
        {
            Debug.Log(
                "TowerFloor.EnsureMapReference: " +
                "Adding Map component at runtime. " +
                "floor=" + floorNumber +
                ", object=" + gameObject.name,
                this
            );

            map = gameObject.AddComponent<Map>();

            if (map != null)
            {
                Debug.Log(
                    "TowerFloor.EnsureMapReference: " +
                    "Map component successfully added. " +
                    "floor=" + floorNumber +
                    ", map=" + map.name +
                    ", mapObject=" + map.gameObject.name,
                    this
                );
            }
            else
            {
                Debug.LogError(
                    "TowerFloor.EnsureMapReference: " +
                    "Failed to add Map component! " +
                    "floor=" + floorNumber +
                    ", object=" + gameObject.name,
                    this
                );
            }
        }
        else
        {
            Debug.LogWarning(
                "TowerFloor.EnsureMapReference: " +
                "Object is not a valid scene object, so Map will not be added. " +
                "floor=" + floorNumber +
                ", object=" + gameObject.name,
                this
            );
        }

        Debug.Log(
            "TowerFloor.EnsureMapReference END: " +
            "floor=" + floorNumber +
            ", finalMap=" + (map != null ? map.name : "NULL"),
            this
        );
    }


    // ============================================================
    // Assign Map
    // ============================================================

    /// <summary>
    /// Assign a Map instance to this TowerFloor.
    /// Used when creating a runtime Map instance to replace
    /// a prefab asset reference.
    /// </summary>
        public void AssignMap(Map m)
    {
        // Debug.Log(
        //     "TowerFloor.AssignMap: " +
        //     "floor=" + floorNumber +
        //     ", object=" + gameObject.name +
        //     ", oldMap=" + (map != null ? map.name : "NULL") +
        //     ", newMap=" + (m != null ? m.name : "NULL"),
        //     this
        // );

        map = m;

        // Debug.Log(
        //     "TowerFloor.AssignMap COMPLETE: " +
        //     "floor=" + floorNumber +
        //     ", finalMap=" + (map != null ? map.name : "NULL") +
        //     ", mapObject=" + (map != null ? map.gameObject.name : "NULL"),
        //     this
        // );
    }

    // ============================================================
    // Runtime Map Conversion
    // ============================================================

    /// <summary>
    /// map이 프리팹 애셋을 직접 참조하고 있다면
    /// 씬 인스턴스로 교체한다.
    /// </summary>
    public void EnsureRuntimeMap()
    {
        if (map != null && !map.gameObject.scene.IsValid())
        {
            // Debug.Log(
            //     "TowerFloor.EnsureRuntimeMap: " +
            //     "Map is a prefab asset reference. Creating runtime instance. " +
            //     "floor=" + floorNumber +
            //     ", prefabMap=" + map.name,
            //     this
            // );

            Map prefabMap = map;
            Map newMap = gameObject.AddComponent<Map>();
            newMap.SetupFrom(prefabMap);
            AssignMap(newMap);
        }
    }
#if UNITY_EDITOR

    // ============================================================
    // Editor Validation
    // ============================================================

    private void OnValidate()
    {
        if (spawnPoint == null)
        {
            spawnPoint =
                GetComponentInChildren<SpawnPoint>(true);
        }

        if (map == null)
        {
            map =
                GetComponentInChildren<Map>(true);
        }

        // Debug.Log(
        //     "TowerFloor OnValidate: " +
        //     "name=" + gameObject.name +
        //     ", floorNumber=" + floorNumber +
        //     ", spawnPoint=" + (spawnPoint != null ? spawnPoint.name : "NULL") +
        //     ", map=" + (map != null ? map.name : "NULL"),
        //     this
        // );
    }

#endif
}