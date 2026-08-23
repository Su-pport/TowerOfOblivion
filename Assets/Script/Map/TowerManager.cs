using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TowerManager : MonoBehaviour
{
    [Header("Floors")]
    [SerializeField] private TowerFloor startingFloor;
    [SerializeField] private TowerFloor[] floors;

    [Header("Player")]
    [SerializeField] private string playerTag = "Player";

    private bool isTransitioning;

    private void Awake()
    {
        Debug.Log("TowerManager Awake: starting InitializeFloors on " + gameObject.name, this);
        try
        {
            InitializeFloors();
        }
        catch (System.Exception ex)
        {
            Debug.LogError("TowerManager.InitializeFloors threw an exception: " + ex.Message + "\n" + ex.StackTrace, this);
        }
    }

    public void TravelToFloor(TowerFloor destination, GameObject player)
    {
        Debug.Log("TowerManager.TravelToFloor called on " + gameObject.name + " active=" + gameObject.activeInHierarchy + " enabled=" + enabled + " destination=" + (destination?destination.name:"null") + " player=" + (player?player.name:"null"), this);
        if (isTransitioning || destination == null || player == null)
            return;

        StartCoroutine(
            TravelRoutine(destination, player)
        );
    }

    private void InitializeFloors()
    {
        // Disable any Grid or Map components directly under TowerManager
        // to avoid coordinate system conflicts with TowerFloor's Grid.
        Transform gridT = transform.Find("Grid");
        if (gridT != null)
        {
            gridT.gameObject.SetActive(false);
        }

        // 자동으로 씬의 모든 TowerFloor를 찾아 `floors`에 연결합니다.
        // Find all TowerFloor instances that belong to loaded scenes.
        var allFloors = Resources.FindObjectsOfTypeAll<TowerFloor>();
        TowerFloor[] foundFloors = System.Array.FindAll(allFloors, f => f != null && f.gameObject != null && f.gameObject.scene.IsValid());

        if (foundFloors == null || foundFloors.Length == 0)
        {
            // 씬에 직접 배치된 TowerFloor를 찾지 못한 경우,
            // 인스펙터에 수동으로 할당한 `floors` 배열을 폴백으로 사용합니다.
            TowerFloor[] validSerialized = null;
            if (floors != null && floors.Length > 0)
            {
                validSerialized = System.Array.FindAll(floors, f => f != null && f.gameObject != null && f.gameObject.scene.IsValid());
            }

            if (validSerialized != null && validSerialized.Length > 0)
            {
                foundFloors = validSerialized;
            }
            else
            {
                Debug.LogError("TowerManager: 씬에 TowerFloor가 없습니다.", this);
                return;
            }
        }

        // 정렬: floorNumber 오름차순으로 정렬하면 1층을 쉽게 찾을 수 있습니다.
        System.Array.Sort(foundFloors, (a, b) => a.FloorNumber.CompareTo(b.FloorNumber));

        // 런타임에서 floors 배열이 비어있거나 길이가 다르면 덮어씁니다.
        if (floors == null || floors.Length != foundFloors.Length)
            floors = foundFloors;

        // 시작 층 결정: 지정되어 있으면 사용, 아니면 floorNumber==1 또는 첫 번째 요소
        if (startingFloor == null)
            startingFloor = System.Array.Find(foundFloors, f => f.FloorNumber == 1) ?? foundFloors[0];

        // 모든 층에 대해 Map 참조를 확보하고, 시작 층만 활성화
        foreach (TowerFloor floor in foundFloors)
        {
            if (floor == null) continue;
            floor.EnsureMapReference();
            bool shouldBeActive = floor == startingFloor;
            if (floor.gameObject.activeSelf != shouldBeActive)
                floor.gameObject.SetActive(shouldBeActive);
        }

        // 플레이어 초기 위치 설정
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null)
        {
            Debug.LogError("TowerManager: Player 태그를 가진 오브젝트를 찾을 수 없습니다.", this);
            return;
        }

        if (startingFloor.SpawnPoint == null)
        {
            Debug.LogError("TowerManager: Starting Floor에 SpawnPoint가 없습니다.", startingFloor);
            return;
        }

        // 시작 층의 맵을 강제 생성하고 플레이어 위치 설정
        // If the assigned Map is a prefab asset (not a scene instance),
        // create a runtime Map instance on the TowerFloor GameObject and
        // copy settings from the prefab.
        if (startingFloor.Map != null && !startingFloor.Map.gameObject.scene.IsValid())
        {
            Map prefabMap = startingFloor.Map;
            Map newMap = startingFloor.gameObject.AddComponent<Map>();
            newMap.SetupFrom(prefabMap);
            startingFloor.AssignMap(newMap);
        }

        if (startingFloor.Map != null)
            startingFloor.Map.GenerateMapForce();

        // Set player spawn position AFTER map generation
        // (PlacePlayerInRandomRoom runs during GenerateMap, so player is already placed)
        // We'll leave the player where GenerateMap put them (random room) unless placePlayerInRandomRoom is false
        bool playerPlacedByMap = startingFloor.Map != null && startingFloor.Map.Rooms.Count > 0;
        if (!playerPlacedByMap && startingFloor.SpawnPoint != null)
        {
            // Only use SpawnPoint if Map didn't place the player
            player.transform.position = startingFloor.SpawnPoint.WorldPosition;
        }

        // Verify player is in a room
        Vector2Int playerCell = startingFloor.Map != null ? startingFloor.Map.WorldToCell(player.transform.position) : new Vector2Int(Mathf.FloorToInt(player.transform.position.x), Mathf.FloorToInt(player.transform.position.y));
        bool playerInRoom = false;
        foreach (Room room in startingFloor.Map.Rooms)
        {
            if (room.Contains(playerCell))
            {
                playerInRoom = true;
                Debug.Log($"TowerManager: player verified in room. Position: {player.transform.position}, Cell: {playerCell}, Room: {room.Bounds}", this);
                break;
            }
        }
        if (!playerInRoom && startingFloor.Map.Rooms.Count > 0)
        {
            Debug.LogWarning($"TowerManager: player NOT in any room. Position: {player.transform.position}, Cell: {playerCell}. Total rooms: {startingFloor.Map.Rooms.Count}", this);
            foreach (Room room in startingFloor.Map.Rooms)
            {
                Debug.Log($"  Room bounds: {room.Bounds}", this);
            }
        }

        // Disable stray Map and TilemapRenderer instances that may be
        // placed under the TowerManager GameObject (designer left-over Grid/Map).
        // These cause duplicate visible maps when runtime floors also create their own Grid/Tilemaps.
        var ownMaps = GetComponentsInChildren<Map>(true);
        foreach (var m in ownMaps)
        {
            if (m == null) continue;
            bool underAnyFloor = false;
            foreach (var f in floors)
            {
                if (f != null && m.transform.IsChildOf(f.transform))
                {
                    underAnyFloor = true;
                    break;
                }
            }
            if (!underAnyFloor)
                m.enabled = false;
        }

        var ownTms = GetComponentsInChildren<TilemapRenderer>(true);
        foreach (var tm in ownTms)
        {
            if (tm == null) continue;
            bool underAnyFloor = false;
            foreach (var f in floors)
            {
                if (f != null && tm.transform.IsChildOf(f.transform))
                {
                    underAnyFloor = true;
                    break;
                }
            }
            if (!underAnyFloor && tm.enabled)
                tm.enabled = false;
        }

        player.transform.position = startingFloor.SpawnPoint.WorldPosition;
    }

    private IEnumerator TravelRoutine(
        TowerFloor destination,
        GameObject player)
    {
        isTransitioning = true;

        ActivateFloor(
            destination,
            player.transform
        );

        // 한 프레임 대기
        yield return null;

        isTransitioning = false;
    }

    private void ActivateFloor(
        TowerFloor destination,
        Transform player)
    {
        if (destination == null)
            return;

        if (destination.SpawnPoint == null)
        {
            Debug.LogError(
                "TowerManager: " +
                destination.name +
                "에 SpawnPoint가 없습니다.",
                destination
            );

            return;
        }

        // 1. 목적지 층 활성화
        // First, clear maps/objects from all other floors so they don't linger.
        var allFloorsAll = Resources.FindObjectsOfTypeAll<TowerFloor>();
        TowerFloor[] allFloors = System.Array.FindAll(allFloorsAll, f => f != null && f.gameObject != null && f.gameObject.scene.IsValid());
        Debug.Log("ActivateFloor: destination=" + destination.name + ", total floors found=" + allFloors.Length, this);
        foreach (TowerFloor floor in allFloors)
        {
            if (floor == null) continue;

            if (floor != destination)
            {
                if (floor.Map != null)
                {
                    Debug.Log("ActivateFloor: clearing map of '" + floor.gameObject.name + "'", this);
                    floor.Map.ClearPreviousMap();
                }

                if (floor.gameObject.activeSelf)
                {
                    floor.gameObject.SetActive(false);
                    Debug.Log("ActivateFloor: deactivated '" + floor.gameObject.name + "'", this);
                }
            }
        }

        // Activate destination floor now (it will be generated next)
        if (!destination.gameObject.activeSelf)
        {
            destination.gameObject.SetActive(true);
            Debug.Log("ActivateFloor: activated destination '" + destination.gameObject.name + "'", this);
        }

        // Additionally ensure only tilemap renderers that belong to the destination
        // floor are enabled so lingering renderers from other floors don't remain visible.
        var allTms = Resources.FindObjectsOfTypeAll<TilemapRenderer>();
        TilemapRenderer[] tms = System.Array.FindAll(allTms, t => t != null && t.gameObject != null && t.gameObject.scene.IsValid());
        Debug.Log("ActivateFloor: found TilemapRenderers=" + tms.Length, this);
        foreach (var tm in tms)
        {
            if (tm == null) continue;
            bool shouldEnable = tm.transform.IsChildOf(destination.transform);
            if (tm.enabled != shouldEnable)
            {
                tm.enabled = shouldEnable;
                Debug.Log("ActivateFloor: set TilemapRenderer '" + tm.gameObject.name + "' enabled=" + shouldEnable, this);
            }
        }

        // Ensure the destination floor has a Map and generate it. This
        // recovers from cases where the Map or its Tilemap children were
        // removed from the Hierarchy at authoring time.
        destination.EnsureMapReference();
        if (destination.Map != null)
        {
            Debug.Log("ActivateFloor: ensuring tilemaps and forcing GenerateMap on " + destination.name, this);
            destination.Map.GenerateMapForce();
        }

        // 2. 플레이어 이동
        player.position =
            destination.SpawnPoint.WorldPosition;
    }
}