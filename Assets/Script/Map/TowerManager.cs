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

    /// <summary>
    /// 게임 시작 시 층과 플레이어의 초기 상태를 설정합니다.
    /// </summary>
    private void Awake()
    {
        Debug.Log(
            "TowerManager Awake: starting InitializeFloors on " + gameObject.name,
            this
        );

        try
        {
            InitializeFloors();
        }
        catch (System.Exception ex)
        {
            Debug.LogError(
                "TowerManager.InitializeFloors threw an exception: " +
                ex.Message + "\n" + ex.StackTrace,
                this
            );
        }
    }

    /// <summary>
    /// 지정한 층으로 이동하는 코루틴을 시작합니다.
    /// </summary>
    public void TravelToFloor(TowerFloor destination, GameObject player)
    {
        Debug.Log(
            "TowerManager.TravelToFloor called on " +
            gameObject.name +
            " active=" + gameObject.activeInHierarchy +
            " enabled=" + enabled +
            " destination=" + (destination ? destination.name : "null") +
            " player=" + (player ? player.name : "null"),
            this
        );

        if (isTransitioning || destination == null || player == null)
            return;

        StartCoroutine(TravelRoutine(destination, player));
    }

    /// <summary>
    /// 씬에 존재하는 TowerFloor들을 찾고 시작 층을 활성화합니다.
    /// 시작 층의 맵을 생성한 후 플레이어가 랜덤 방에 배치되었는지 확인합니다.
    /// </summary>
    private void InitializeFloors()
    {
        // TowerManager 아래에 남아 있는 Grid는 좌표 충돌을 방지하기 위해 비활성화합니다.
        Transform gridT = transform.Find("Grid");

        if (gridT != null)
            gridT.gameObject.SetActive(false);

        // 씬에 존재하는 모든 TowerFloor를 찾습니다.
        var allFloors = Resources.FindObjectsOfTypeAll<TowerFloor>();

        TowerFloor[] foundFloors = System.Array.FindAll(
            allFloors,
            f => f != null &&
                 f.gameObject != null &&
                 f.gameObject.scene.IsValid()
        );

        // 자동 검색에 실패하면 Inspector에 지정된 floors를 사용합니다.
        if (foundFloors == null || foundFloors.Length == 0)
        {
            TowerFloor[] validSerialized = null;

            if (floors != null && floors.Length > 0)
            {
                validSerialized = System.Array.FindAll(
                    floors,
                    f => f != null &&
                         f.gameObject != null &&
                         f.gameObject.scene.IsValid()
                );
            }

            if (validSerialized != null && validSerialized.Length > 0)
            {
                foundFloors = validSerialized;
            }
            else
            {
                Debug.LogError(
                    "TowerManager: 씬에 TowerFloor가 없습니다.",
                    this
                );

                return;
            }
        }

        // 층 번호 순서로 정렬합니다.
        System.Array.Sort(
            foundFloors,
            (a, b) => a.FloorNumber.CompareTo(b.FloorNumber)
        );

        // 찾은 층 목록을 런타임 floors 배열에 반영합니다.
        if (floors == null || floors.Length != foundFloors.Length)
            floors = foundFloors;

        // 시작 층이 지정되지 않았다면 1층을 우선 사용합니다.
        if (startingFloor == null)
        {
            startingFloor =
                System.Array.Find(foundFloors, f => f.FloorNumber == 1)
                ?? foundFloors[0];
        }

        // 모든 층의 Map 참조를 확보하고 시작 층만 활성화합니다.
        foreach (TowerFloor floor in foundFloors)
        {
            if (floor == null)
                continue;

            floor.EnsureMapReference();

            bool shouldBeActive = floor == startingFloor;

            if (floor.gameObject.activeSelf != shouldBeActive)
                floor.gameObject.SetActive(shouldBeActive);
        }

        // 플레이어를 찾습니다.
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);

        if (player == null)
        {
            Debug.LogError(
                "TowerManager: Player 태그를 가진 오브젝트를 찾을 수 없습니다.",
                this
            );

            return;
        }

        if (startingFloor.SpawnPoint == null)
        {
            Debug.LogError(
                "TowerManager: Starting Floor에 SpawnPoint가 없습니다.",
                startingFloor
            );

            return;
        }

        // Map이 프리팹에서 참조된 경우 런타임 Map을 생성합니다.
        if (startingFloor.Map != null &&
            !startingFloor.Map.gameObject.scene.IsValid())
        {
            Map prefabMap = startingFloor.Map;

            Map newMap = startingFloor.gameObject.AddComponent<Map>();
            newMap.SetupFrom(prefabMap);

            startingFloor.AssignMap(newMap);
        }

        // 맵을 생성합니다.
        // 플레이어 위치는 Map.GenerateMap() 내부에서 랜덤 방으로 결정됩니다.
        if (startingFloor.Map != null)
            startingFloor.Map.GenerateMapForce();

        // 맵 생성 후 플레이어가 실제 방 안에 배치되었는지 확인합니다.
        if (startingFloor.Map != null &&
            startingFloor.Map.Rooms.Count > 0)
        {
            Vector2Int playerCell =
                startingFloor.Map.WorldToCell(player.transform.position);

            bool playerInRoom = false;

            foreach (Room room in startingFloor.Map.Rooms)
            {
                if (!room.Contains(playerCell))
                    continue;

                playerInRoom = true;

                Debug.Log(
                    $"TowerManager: player verified in room. " +
                    $"Position: {player.transform.position}, " +
                    $"Cell: {playerCell}, " +
                    $"Room: {room.Bounds}",
                    this
                );

                break;
            }

            if (!playerInRoom)
            {
                Debug.LogWarning(
                    $"TowerManager: player NOT in any room. " +
                    $"Position: {player.transform.position}, " +
                    $"Cell: {playerCell}. " +
                    $"Total rooms: {startingFloor.Map.Rooms.Count}",
                    this
                );
            }
        }

        // TowerManager 아래에 잘못 배치된 Map을 비활성화합니다.
        var ownMaps = GetComponentsInChildren<Map>(true);

        foreach (var map in ownMaps)
        {
            if (map == null)
                continue;

            bool underAnyFloor = false;

            foreach (var floor in floors)
            {
                if (floor != null &&
                    map.transform.IsChildOf(floor.transform))
                {
                    underAnyFloor = true;
                    break;
                }
            }

            if (!underAnyFloor)
                map.enabled = false;
        }

        // TowerManager 아래에 남아 있는 잘못된 TilemapRenderer를 비활성화합니다.
        var ownTilemapRenderers =
            GetComponentsInChildren<TilemapRenderer>(true);

        foreach (var tilemapRenderer in ownTilemapRenderers)
        {
            if (tilemapRenderer == null)
                continue;

            bool underAnyFloor = false;

            foreach (var floor in floors)
            {
                if (floor != null &&
                    tilemapRenderer.transform.IsChildOf(floor.transform))
                {
                    underAnyFloor = true;
                    break;
                }
            }

            if (!underAnyFloor && tilemapRenderer.enabled)
                tilemapRenderer.enabled = false;
        }

        // 여기서 SpawnPoint로 플레이어를 이동시키지 않습니다.
        // 플레이어 위치는 Map.GenerateMap()의 랜덤 방 배치를 그대로 사용합니다.
    }

    /// <summary>
    /// 목적지 층으로 이동하고 한 프레임 후 이동 상태를 해제합니다.
    /// </summary>
    private IEnumerator TravelRoutine(
        TowerFloor destination,
        GameObject player)
    {
        isTransitioning = true;

        ActivateFloor(
            destination,
            player.transform
        );

        yield return null;

        isTransitioning = false;
    }

    /// <summary>
    /// 기존 층을 정리하고 목적지 층을 활성화한 뒤 맵을 새로 생성합니다.
    /// 플레이어는 새 맵의 랜덤 방에 배치됩니다.
    /// </summary>
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

        // 씬에 존재하는 모든 층을 가져옵니다.
        var allFloorsAll =
            Resources.FindObjectsOfTypeAll<TowerFloor>();

        TowerFloor[] allFloors = System.Array.FindAll(
            allFloorsAll,
            f => f != null &&
                 f.gameObject != null &&
                 f.gameObject.scene.IsValid()
        );

        Debug.Log(
            "ActivateFloor: destination=" +
            destination.name +
            ", total floors found=" +
            allFloors.Length,
            this
        );

        // 목적지를 제외한 다른 층의 맵과 오브젝트를 정리합니다.
        foreach (TowerFloor floor in allFloors)
        {
            if (floor == null)
                continue;

            if (floor != destination)
            {
                if (floor.Map != null)
                {
                    Debug.Log(
                        "ActivateFloor: clearing map of '" +
                        floor.gameObject.name +
                        "'",
                        this
                    );

                    floor.Map.ClearPreviousMap();
                }

                if (floor.gameObject.activeSelf)
                {
                    floor.gameObject.SetActive(false);

                    Debug.Log(
                        "ActivateFloor: deactivated '" +
                        floor.gameObject.name +
                        "'",
                        this
                    );
                }
            }
        }

        // 목적지 층을 활성화합니다.
        if (!destination.gameObject.activeSelf)
        {
            destination.gameObject.SetActive(true);

            Debug.Log(
                "ActivateFloor: activated destination '" +
                destination.gameObject.name +
                "'",
                this
            );
        }

        // 목적지 층에 속한 TilemapRenderer만 활성화합니다.
        var allTms =
            Resources.FindObjectsOfTypeAll<TilemapRenderer>();

        TilemapRenderer[] tms = System.Array.FindAll(
            allTms,
            t => t != null &&
                 t.gameObject != null &&
                 t.gameObject.scene.IsValid()
        );

        Debug.Log(
            "ActivateFloor: found TilemapRenderers=" +
            tms.Length,
            this
        );

        foreach (var tilemapRenderer in tms)
        {
            if (tilemapRenderer == null)
                continue;

            bool shouldEnable =
                tilemapRenderer.transform.IsChildOf(destination.transform);

            if (tilemapRenderer.enabled != shouldEnable)
            {
                tilemapRenderer.enabled = shouldEnable;

                Debug.Log(
                    "ActivateFloor: set TilemapRenderer '" +
                    tilemapRenderer.gameObject.name +
                    "' enabled=" +
                    shouldEnable,
                    this
                );
            }
        }

        // 목적지 층의 Map을 확보하고 새 맵을 생성합니다.
        destination.EnsureMapReference();

        if (destination.Map != null)
        {
            Debug.Log(
                "ActivateFloor: ensuring tilemaps and forcing GenerateMap on " +
                destination.name,
                this
            );

            destination.Map.GenerateMapForce();
        }

        // GenerateMapForce()가 플레이어를 랜덤 방에 배치하므로
        // 여기서는 SpawnPoint로 플레이어를 다시 이동시키지 않습니다.
    }
}