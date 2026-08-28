using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// BSP 방식으로 던전 맵을 생성하고 Tilemap에 그린다.
/// 방과 통로를 생성한 뒤 벽 Tile을 주변 Floor 배치에 따라 결정한다.
/// </summary>
public class Map : MonoBehaviour
{
    [Header("Tilemaps")]
    [SerializeField] private Tilemap backgroundTilemap;
    [SerializeField] private Tilemap floorTilemap;
    [SerializeField] private Tilemap wallRenderTilemap;

    [Header("Base Tiles")]
    [SerializeField] private TileBase blackTile;
    [SerializeField] private TileBase floorTile;

    [Header("Straight Wall Tiles")]
    [SerializeField] private TileBase topWallTile;
    [SerializeField] private TileBase bottomWallTile;
    [SerializeField] private TileBase leftWallTile;
    [SerializeField] private TileBase rightWallTile;

    [Header("Outer Corner Wall Tiles")]
    [SerializeField] private TileBase outerCornerTopLeftTile;
    [SerializeField] private TileBase outerCornerTopRightTile;
    [SerializeField] private TileBase outerCornerBottomLeftTile;
    [SerializeField] private TileBase outerCornerBottomRightTile;

    [Header("Inner Corner Wall Tiles")]
    [SerializeField] private TileBase innerCornerTopLeftTile;
    [SerializeField] private TileBase innerCornerTopRightTile;
    [SerializeField] private TileBase innerCornerBottomLeftTile;
    [SerializeField] private TileBase innerCornerBottomRightTile;

    [Header("Map Settings")]
    [Min(16)]
    [SerializeField] private int mapWidth = 80;

    [Min(16)]
    [SerializeField] private int mapHeight = 60;

    [Min(1)]
    [SerializeField] private int maxDepth = 4;

    [Min(6)]
    [SerializeField] private int minNodeSize = 15;

    [Min(4)]
    [SerializeField] private int minRoomSize = 6;

    [Min(1)]
    [SerializeField] private int roomPadding = 2;

    [Header("Extra Nearby Connections")]
    [SerializeField] private bool createNearbySharedConnections = true;

    [Min(0)]
    [SerializeField] private int maximumNearbySharedConnections = 2;

    [Min(1)]
    [SerializeField] private int nearbyConnectionDistance = 18;

    [Header("Spawning (Optional)")]
    [SerializeField] private GameObject monsterPrefab;

    [Min(0)]
    [SerializeField] private int monsterCount = 6;

    [Header("Portal (Optional)")]
    [SerializeField] private GameObject portalPrefab;
    [SerializeField] private bool spawnPortal = true;

    [Header("Player Placement")]
    [SerializeField] private bool placePlayerInRandomRoom = true;

    private enum RoomSide
    {
        Top,
        Bottom,
        Left,
        Right
    }

    private TileType[,] mapData;
    private bool[,] visible;
    private bool[,] discovered;

    private BSPNode root;

    private readonly List<Room> rooms = new List<Room>();
    private readonly List<GameObject> spawnedObjects = new List<GameObject>();
    private readonly Dictionary<Room, HashSet<RoomSide>> usedRoomSides =
        new Dictionary<Room, HashSet<RoomSide>>();

    private bool hasGenerated;

    public TileType[,] MapData => mapData;
    public bool[,] Visible => visible;
    public bool[,] Discovered => discovered;
    public IReadOnlyList<Room> Rooms => rooms;

    /// <summary>
    /// 월드 좌표를 Floor Tilemap의 셀 좌표로 변환한다.
    /// </summary>
    public Vector2Int WorldToCell(Vector3 worldPos)
    {
        if (floorTilemap == null)
        {
            Debug.LogWarning(
                "Map.WorldToCell: floorTilemap is null.",
                this
            );

            return new Vector2Int(
                Mathf.FloorToInt(worldPos.x),
                Mathf.FloorToInt(worldPos.y)
            );
        }

        Vector3Int cell = floorTilemap.WorldToCell(worldPos);
        return new Vector2Int(cell.x, cell.y);
    }

    /// <summary>
    /// 게임 시작 시 Tilemap을 준비하고 필요한 경우 맵을 생성한다.
    /// TowerManager가 있는 TowerFloor에서는 TowerManager가 생성 순서를 관리한다.
    /// </summary>
    private void Start()
    {
        EnsureTilemaps();

        TowerFloor parentFloor = GetComponentInParent<TowerFloor>();
        TowerManager manager = Object.FindFirstObjectByType<TowerManager>();

        if (parentFloor != null && manager != null)
            return;

        GenerateMap();
    }

    /// <summary>
    /// Tilemap 참조가 없으면 자식 오브젝트에서 찾고,
    /// 그래도 없으면 필요한 Grid와 Tilemap을 생성한다.
    /// </summary>
    private void EnsureTilemaps()
    {
        if (backgroundTilemap != null &&
            floorTilemap != null &&
            wallRenderTilemap != null)
        {
            return;
        }

        Tilemap[] foundTilemaps = GetComponentsInChildren<Tilemap>(true);

        foreach (Tilemap tilemap in foundTilemaps)
        {
            string name = tilemap.gameObject.name.ToLower();

            if (backgroundTilemap == null && name.Contains("background"))
                backgroundTilemap = tilemap;
            else if (floorTilemap == null && name.Contains("floor"))
                floorTilemap = tilemap;
            else if (wallRenderTilemap == null &&
                     (name.Contains("wall") || name.Contains("wallrender")))
                wallRenderTilemap = tilemap;
        }

        if (backgroundTilemap != null &&
            floorTilemap != null &&
            wallRenderTilemap != null)
        {
            return;
        }

        Transform gridTransform = transform.Find("Grid");
        GameObject gridObject;

        if (gridTransform == null)
        {
            if (!gameObject.scene.IsValid())
            {
                Debug.LogWarning(
                    "Map.EnsureTilemaps: Map is a prefab asset. " +
                    "Skipping Tilemap creation.",
                    this
                );

                return;
            }

            gridObject = new GameObject("Grid");
            gridObject.transform.SetParent(transform, false);
            gridObject.transform.localPosition = Vector3.zero;
            gridObject.AddComponent<Grid>();
        }
        else
        {
            gridObject = gridTransform.gameObject;
            gridObject.transform.localPosition = Vector3.zero;
        }

        Tilemap CreateTilemapChild(string name)
        {
            Transform child = gridObject.transform.Find(name);

            if (child != null)
            {
                Tilemap existingTilemap = child.GetComponent<Tilemap>();

                if (existingTilemap != null)
                    return existingTilemap;
            }

            GameObject tilemapObject = new GameObject(name);
            tilemapObject.transform.SetParent(gridObject.transform, false);
            tilemapObject.transform.localPosition = Vector3.zero;

            Tilemap tilemap = tilemapObject.AddComponent<Tilemap>();
            TilemapRenderer renderer = tilemapObject.AddComponent<TilemapRenderer>();

            if (name.Contains("Background"))
                renderer.sortingOrder = 0;
            else if (name.Contains("Floor"))
                renderer.sortingOrder = 1;
            else if (name.Contains("Wall"))
                renderer.sortingOrder = 2;

            if (name.Contains("Wall") || name.Contains("Floor"))
            {
                TilemapCollider2D collider =
                    tilemapObject.AddComponent<TilemapCollider2D>();

                collider.compositeOperation =
                    Collider2D.CompositeOperation.None;

                collider.isTrigger = name.Contains("Floor");
            }

            return tilemap;
        }

        if (backgroundTilemap == null)
            backgroundTilemap = CreateTilemapChild("BackgroundTilemap");

        if (floorTilemap == null)
            floorTilemap = CreateTilemapChild("FloorTilemap");

        if (wallRenderTilemap == null)
            wallRenderTilemap = CreateTilemapChild("WallRenderTilemap");
    }

    /// <summary>
    /// 던전 생성부터 플레이어, 몬스터, 포탈 배치까지 전체 맵 생성 과정을 실행한다.
    /// </summary>
    public void GenerateMap()
    {
        if (hasGenerated)
            return;

        ClearPreviousMap();
        InitializeMapData();
        CreateDungeon();
        CreateWalls();
        CreateCornerWalls();
        DrawMap();

        if (placePlayerInRandomRoom)
            PlacePlayerInRandomRoom();

        SpawnActors();
        SpawnPortalAtFarthestRoom();

        hasGenerated = true;
    }

    /// <summary>
    /// Unity Inspector의 ContextMenu에서 맵을 직접 생성한다.
    /// </summary>
    [ContextMenu("Generate Map (Editor)")]
    public void EditorGenerateMap()
    {
        EnsureTilemaps();
        GenerateMap();
    }

    /// <summary>
    /// 이미 생성된 맵이라도 다시 생성한다.
    /// </summary>
    public void GenerateMapForce()
    {
        hasGenerated = false;

        EnsureTilemaps();
        GenerateMap();
    }

    /// <summary>
    /// 다른 Map 객체의 설정과 Tile 참조를 현재 Map에 복사한다.
    /// </summary>
    public void SetupFrom(Map source)
    {
        if (source == null)
            return;

        blackTile = source.blackTile;
        floorTile = source.floorTile;

        topWallTile = source.topWallTile;
        bottomWallTile = source.bottomWallTile;
        leftWallTile = source.leftWallTile;
        rightWallTile = source.rightWallTile;

        outerCornerTopLeftTile = source.outerCornerTopLeftTile;
        outerCornerTopRightTile = source.outerCornerTopRightTile;
        outerCornerBottomLeftTile = source.outerCornerBottomLeftTile;
        outerCornerBottomRightTile = source.outerCornerBottomRightTile;

        innerCornerTopLeftTile = source.innerCornerTopLeftTile;
        innerCornerTopRightTile = source.innerCornerTopRightTile;
        innerCornerBottomLeftTile = source.innerCornerBottomLeftTile;
        innerCornerBottomRightTile = source.innerCornerBottomRightTile;

        mapWidth = source.mapWidth;
        mapHeight = source.mapHeight;
        maxDepth = source.maxDepth;
        minNodeSize = source.minNodeSize;
        minRoomSize = source.minRoomSize;
        roomPadding = source.roomPadding;

        createNearbySharedConnections = source.createNearbySharedConnections;
        maximumNearbySharedConnections = source.maximumNearbySharedConnections;
        nearbyConnectionDistance = source.nearbyConnectionDistance;

        monsterPrefab = source.monsterPrefab;
        monsterCount = source.monsterCount;

        portalPrefab = source.portalPrefab;
        spawnPortal = source.spawnPortal;

        placePlayerInRandomRoom = source.placePlayerInRandomRoom;
    }

    /// <summary>
    /// 플레이어 위치를 기준으로 가장 먼 방에 포탈을 생성한다.
    /// </summary>
    private void SpawnPortalAtFarthestRoom()
    {
        if (!spawnPortal ||
            portalPrefab == null ||
            rooms.Count == 0 ||
            floorTilemap == null)
        {
            return;
        }

        Vector3 playerSpawn = Vector3.zero;

// 실제 플레이어 위치를 기준으로 한다.
GameObject player =
    GameObject.FindGameObjectWithTag("Player");

if (player != null)
{
    playerSpawn = player.transform.position;
}
else
{
    // 플레이어를 찾지 못하면 TowerFloor의 SpawnPoint를 사용한다.
    TowerFloor parentFloor =
        GetComponentInParent<TowerFloor>();

    if (parentFloor != null &&
        parentFloor.SpawnPoint != null)
    {
        playerSpawn =
            parentFloor.SpawnPoint.WorldPosition;
    }
}

float bestDistance = -1f;
Vector3 bestWorldPosition = Vector3.zero;

foreach (Room room in rooms)
{
    // 방의 중심을 FloorTilemap 기준 월드 좌표로 변환한다.
    Vector3 roomWorldPosition =
        floorTilemap.GetCellCenterWorld(
            new Vector3Int(
                room.Center.x,
                room.Center.y,
                0
            )
        );

    float distance =
        Vector3.SqrMagnitude(
            roomWorldPosition - playerSpawn
        );

    if (distance > bestDistance)    
    {
        bestDistance = distance;
        bestWorldPosition = roomWorldPosition;
    }
}

        if (bestDistance < 0f)
            {
        Debug.LogWarning(
            "Map: 포탈을 생성할 적절한 방을 찾지 못했습니다.",
            this
        );

        return;
    }

        GameObject portal =
    Instantiate(
        portalPrefab,
        bestWorldPosition,
        Quaternion.identity
    );

if (gameObject.scene.IsValid())
{
    // 부모를 변경해도 월드 좌표를 유지한다.
    portal.transform.SetParent(transform, true);
}

spawnedObjects.Add(portal);
Debug.Log(
        $"Portal 생성 위치: {portal.transform.position}",
        portal
    );
    }

    /// <summary>
    /// 생성된 방 중 하나를 선택하여 플레이어를 배치한다.
    /// </summary>
    private void PlacePlayerInRandomRoom()
    {
        if (rooms.Count == 0 || floorTilemap == null)
        {
            Debug.LogWarning(
                "Map: PlacePlayerInRandomRoom failed.",
                this
            );

            return;
        }

        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogWarning(
                "Map: Player 태그를 가진 오브젝트를 찾을 수 없습니다."
            );

            return;
        }

        Room selectedRoom =
            rooms[Random.Range(0, rooms.Count)];

        Vector2Int cell =
            selectedRoom.GetRandomPosition();

        Vector3 worldPosition =
            floorTilemap.GetCellCenterWorld(
                new Vector3Int(cell.x, cell.y, 0)
            );

        player.transform.position = worldPosition;

        Debug.Log(
            "Map.PlacePlayerInRandomRoom: 플레이어 이동 완료. " +
            "targetPosition=" + worldPosition +
            ", actualPosition=" + player.transform.position +
            ", playerObject=" + player.name,
            this
        );
    }

    /// <summary>
    /// 맵 데이터와 방 연결 정보를 초기화한다.
    /// </summary>
    private void InitializeMapData()
    {
        mapData = new TileType[mapWidth, mapHeight];
        visible = new bool[mapWidth, mapHeight];
        discovered = new bool[mapWidth, mapHeight];

        rooms.Clear();
        usedRoomSides.Clear();
    }

    /// <summary>
    /// BSP 트리를 생성하고 방과 통로를 만든다.
    /// </summary>
    private void CreateDungeon()
    {
        int border = roomPadding + 1;

        int usableWidth = mapWidth - border * 2;
        int usableHeight = mapHeight - border * 2;

        if (usableWidth < minNodeSize ||
            usableHeight < minNodeSize)
        {
            Debug.LogError(
                "Map is too small for the current BSP settings."
            );

            return;
        }

        root = new BSPNode(
            new RectInt(
                border,
                border,
                usableWidth,
                usableHeight
            )
        );

        root.SplitRecursive(maxDepth, minNodeSize);
        root.CreateRooms(minRoomSize, roomPadding);
        root.GetLeafRooms(rooms);

        foreach (Room room in rooms)
            CarveRoom(room.Bounds);

        ConnectNodeChildren(root);

        if (createNearbySharedConnections)
            CreateNearbySharedConnections();
    }

    /// <summary>
    /// BSP 트리를 따라 내려가면서 두 자식 영역의 방 그룹을 연결한다.
    /// </summary>
    private void ConnectNodeChildren(BSPNode node)
    {
        if (node == null || node.IsLeaf)
            return;

        ConnectNodeChildren(node.Left);
        ConnectNodeChildren(node.Right);

        List<Room> leftRooms = GetRoomsBelow(node.Left);
        List<Room> rightRooms = GetRoomsBelow(node.Right);

        TryConnectRoomGroups(leftRooms, rightRooms);
    }

    /// <summary>
    /// BSP 노드 아래에 존재하는 모든 Leaf Room을 반환한다.
    /// </summary>
    private List<Room> GetRoomsBelow(BSPNode node)
    {
        List<Room> result = new List<Room>();

        if (node != null)
            node.GetLeafRooms(result);

        return result;
    }

    /// <summary>
    /// 두 방 그룹에서 가장 적절한 방을 선택하여 통로로 연결한다.
    /// </summary>
    private bool TryConnectRoomGroups(
        List<Room> firstGroup,
        List<Room> secondGroup)
    {
        Room bestFirst = null;
        Room bestSecond = null;

        RoomSide bestFirstSide = RoomSide.Right;
        RoomSide bestSecondSide = RoomSide.Left;

        int bestScore = int.MaxValue;

        foreach (Room first in firstGroup)
        {
            foreach (Room second in secondGroup)
            {
                RoomSide firstSide;
                RoomSide secondSide;

                if (!TryGetFacingSides(
                        first,
                        second,
                        out firstSide,
                        out secondSide))
                {
                    continue;
                }

                if (!CanUseSide(first, firstSide) ||
                    !CanUseSide(second, secondSide))
                {
                    continue;
                }

                int score =
                    ManhattanDistance(
                        first.Center,
                        second.Center
                    );

                if (score < bestScore)
                {
                    bestScore = score;
                    bestFirst = first;
                    bestSecond = second;
                    bestFirstSide = firstSide;
                    bestSecondSide = secondSide;
                }
            }
        }

        if (bestFirst == null)
            return false;

        CarveRoomConnection(
            bestFirst,
            bestFirstSide,
            bestSecond,
            bestSecondSide
        );

        ReserveSide(bestFirst, bestFirstSide);
        ReserveSide(bestSecond, bestSecondSide);

        return true;
    }

    /// <summary>
    /// 두 방이 서로 바라보는 방향을 계산한다.
    /// </summary>
    private bool TryGetFacingSides(
        Room first,
        Room second,
        out RoomSide firstSide,
        out RoomSide secondSide)
    {
        Vector2Int delta =
            second.Center - first.Center;

        if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
        {
            firstSide =
                delta.x >= 0
                    ? RoomSide.Right
                    : RoomSide.Left;

            secondSide =
                delta.x >= 0
                    ? RoomSide.Left
                    : RoomSide.Right;
        }
        else
        {
            firstSide =
                delta.y >= 0
                    ? RoomSide.Top
                    : RoomSide.Bottom;

            secondSide =
                delta.y >= 0
                    ? RoomSide.Bottom
                    : RoomSide.Top;
        }

        return true;
    }

    /// <summary>
    /// 두 방의 지정된 면에서 시작하여 두 방을 통로로 연결한다.
    /// </summary>
    private void CarveRoomConnection(
        Room first,
        RoomSide firstSide,
        Room second,
        RoomSide secondSide)
    {
        Vector2Int start =
            GetDoorCell(
                first.Bounds,
                firstSide,
                second.Center
            );

        Vector2Int end =
            GetDoorCell(
                second.Bounds,
                secondSide,
                first.Center
            );

        if (firstSide == RoomSide.Left ||
            firstSide == RoomSide.Right)
        {
            CarveHorizontal(
                start.x,
                end.x,
                start.y
            );

            CarveVertical(
                start.y,
                end.y,
                end.x
            );
        }
        else
        {
            CarveVertical(
                start.y,
                end.y,
                start.x
            );

            CarveHorizontal(
                start.x,
                end.x,
                end.y
            );
        }
    }

    /// <summary>
    /// 방의 지정된 방향에 해당하는 통로 시작 셀을 반환한다.
    /// </summary>
    private Vector2Int GetDoorCell(
        RectInt bounds,
        RoomSide side,
        Vector2Int toward)
    {
        switch (side)
        {
            case RoomSide.Top:
                return new Vector2Int(
                    ClampTwoWide(
                        toward.x,
                        bounds.xMin,
                        bounds.xMax
                    ),
                    bounds.yMax - 1
                );

            case RoomSide.Bottom:
                return new Vector2Int(
                    ClampTwoWide(
                        toward.x,
                        bounds.xMin,
                        bounds.xMax
                    ),
                    bounds.yMin
                );

            case RoomSide.Left:
                return new Vector2Int(
                    bounds.xMin,
                    ClampTwoWide(
                        toward.y,
                        bounds.yMin,
                        bounds.yMax
                    )
                );

            default:
                return new Vector2Int(
                    bounds.xMax - 1,
                    ClampTwoWide(
                        toward.y,
                        bounds.yMin,
                        bounds.yMax
                    )
                );
        }
    }

    /// <summary>
    /// 두 칸 너비의 통로가 방의 범위를 벗어나지 않도록 좌표를 제한한다.
    /// </summary>
    private int ClampTwoWide(
        int value,
        int min,
        int maxExclusive)
    {
        return Mathf.Clamp(
            value,
            min,
            Mathf.Max(min, maxExclusive - 2)
        );
    }

    /// <summary>
    /// 해당 방의 지정된 면이 이미 다른 통로에 사용되었는지 확인한다.
    /// </summary>
    private bool CanUseSide(
        Room room,
        RoomSide side)
    {
        HashSet<RoomSide> sides;

        return !usedRoomSides.TryGetValue(
            room,
            out sides
        ) || !sides.Contains(side);
    }

    /// <summary>
    /// 방의 지정된 면을 통로 연결에 사용된 것으로 기록한다.
    /// </summary>
    private void ReserveSide(
        Room room,
        RoomSide side)
    {
        HashSet<RoomSide> sides;

        if (!usedRoomSides.TryGetValue(
                room,
                out sides))
        {
            sides = new HashSet<RoomSide>();
            usedRoomSides.Add(room, sides);
        }

        sides.Add(side);
    }

    /// <summary>
    /// 가까운 세 개의 방을 T자 형태의 공유 통로로 연결한다.
    /// </summary>
    private void CreateNearbySharedConnections()
    {
        int created = 0;

        for (int i = 0;
             i < rooms.Count &&
             created < maximumNearbySharedConnections;
             i++)
        {
            for (int j = i + 1;
                 j < rooms.Count &&
                 created < maximumNearbySharedConnections;
                 j++)
            {
                if (TryCreateSharedHorizontalConnection(
                        rooms[i],
                        rooms[j]))
                {
                    created++;
                }
            }
        }
    }

    /// <summary>
    /// 좌우에 위치한 두 방을 가로 통로로 연결하고
    /// 주변의 세 번째 방을 세로 통로로 연결한다.
    /// </summary>
    private bool TryCreateSharedHorizontalConnection(
        Room a,
        Room b)
    {
        Room left =
            a.Center.x <= b.Center.x
                ? a
                : b;

        Room right =
            left == a
                ? b
                : a;

        RectInt leftBounds = left.Bounds;
        RectInt rightBounds = right.Bounds;

        if (!CanUseSide(left, RoomSide.Right) ||
            !CanUseSide(right, RoomSide.Left) ||
            rightBounds.xMin <= leftBounds.xMax ||
            rightBounds.xMin - leftBounds.xMax >
            nearbyConnectionDistance)
        {
            return false;
        }

        int sharedMinY =
            Mathf.Max(
                leftBounds.yMin,
                rightBounds.yMin
            );

        int sharedMaxY =
            Mathf.Min(
                leftBounds.yMax - 2,
                rightBounds.yMax - 2
            );

        if (sharedMinY > sharedMaxY)
            return false;

        int trunkY =
            Mathf.Clamp(
                (left.Center.y + right.Center.y) / 2,
                sharedMinY,
                sharedMaxY
            );

        Room branch =
            FindNearbyBranchRoom(
                left,
                right,
                trunkY
            );

        if (branch == null)
            return false;

        Vector2Int leftDoor =
            new Vector2Int(
                leftBounds.xMax - 1,
                trunkY
            );

        Vector2Int rightDoor =
            new Vector2Int(
                rightBounds.xMin,
                trunkY
            );

        CarveHorizontal(
            leftDoor.x,
            rightDoor.x,
            trunkY
        );

        ReserveSide(left, RoomSide.Right);
        ReserveSide(right, RoomSide.Left);

        bool branchIsAbove =
            branch.Center.y > trunkY;

        RoomSide branchSide =
            branchIsAbove
                ? RoomSide.Bottom
                : RoomSide.Top;

        RectInt branchBounds = branch.Bounds;

        int branchX =
            ClampTwoWide(
                (leftDoor.x + rightDoor.x) / 2,
                branchBounds.xMin,
                branchBounds.xMax
            );

        int branchY =
            branchIsAbove
                ? branchBounds.yMin
                : branchBounds.yMax - 1;

        CarveVertical(
            trunkY,
            branchY,
            branchX
        );

        ReserveSide(branch, branchSide);

        return true;
    }

    /// <summary>
    /// 두 방 사이의 공유 통로에 연결할 수 있는 세 번째 방을 찾는다.
    /// </summary>
    private Room FindNearbyBranchRoom(
        Room left,
        Room right,
        int trunkY)
    {
        int trunkMinX =
            left.Bounds.xMax - 1;

        int trunkMaxX =
            right.Bounds.xMin;

        foreach (Room candidate in rooms)
        {
            if (candidate == left ||
                candidate == right)
            {
                continue;
            }

            RectInt bounds = candidate.Bounds;

            int candidateX =
                Mathf.Clamp(
                    (trunkMinX + trunkMaxX) / 2,
                    bounds.xMin,
                    bounds.xMax - 1
                );

            if (candidateX < trunkMinX ||
                candidateX > trunkMaxX)
            {
                continue;
            }

            bool above =
                bounds.yMin > trunkY;

            bool below =
                bounds.yMax - 1 < trunkY;

            if (!above && !below)
                continue;

            int gap =
                above
                    ? bounds.yMin - trunkY
                    : trunkY - (bounds.yMax - 1);

            RoomSide side =
                above
                    ? RoomSide.Bottom
                    : RoomSide.Top;

            if (gap <= nearbyConnectionDistance &&
                CanUseSide(candidate, side))
            {
                return candidate;
            }
        }

        return null;
    }

    /// <summary>
    /// 방 전체를 Floor로 만든다.
    /// </summary>
    private void CarveRoom(RectInt bounds)
    {
        for (int x = bounds.xMin;
             x < bounds.xMax;
             x++)
        {
            for (int y = bounds.yMin;
                 y < bounds.yMax;
                 y++)
            {
                SetFloor(x, y);
            }
        }
    }

    /// <summary>
    /// 가로 방향으로 두 칸 너비의 통로를 생성한다.
    /// </summary>
    private void CarveHorizontal(
        int x1,
        int x2,
        int y)
    {
        int from = Mathf.Min(x1, x2);
        int to = Mathf.Max(x1, x2);

        for (int x = from;
             x <= to;
             x++)
        {
            SetFloor(x, y);
            SetFloor(x, y + 1);
        }
    }

    /// <summary>
    /// 세로 방향으로 두 칸 너비의 통로를 생성한다.
    /// </summary>
    private void CarveVertical(
        int y1,
        int y2,
        int x)
    {
        int from = Mathf.Min(y1, y2);
        int to = Mathf.Max(y1, y2);

        for (int y = from;
             y <= to;
             y++)
        {
            SetFloor(x, y);
            SetFloor(x + 1, y);
        }
    }

    /// <summary>
    /// 맵 범위 안에 있을 경우 해당 셀을 Floor로 변경한다.
    /// </summary>
    private void SetFloor(int x, int y)
    {
        if (IsInBounds(x, y))
            mapData[x, y] = TileType.Floor;
    }

    /// <summary>
    /// Floor 주변의 Empty 셀을 Wall로 변경한다.
    /// </summary>
    private void CreateWalls()
    {
        List<Vector2Int> wallPositions =
            new List<Vector2Int>();

        for (int x = 1;
             x < mapWidth - 1;
             x++)
        {
            for (int y = 1;
                 y < mapHeight - 1;
                 y++)
            {
                if (mapData[x, y] == TileType.Empty &&
                    IsNearFloor(x, y))
                {
                    wallPositions.Add(
                        new Vector2Int(x, y)
                    );
                }
            }
        }

        foreach (Vector2Int position in wallPositions)
        {
            mapData[
                position.x,
                position.y
            ] = TileType.Wall;
        }
    }

    /// <summary>
    /// Floor의 대각선 구조를 확인하여 코너용 Wall을 추가한다.
    /// </summary>
    private void CreateCornerWalls()
    {
        List<Vector2Int> cornerWalls =
            new List<Vector2Int>();

        for (int x = 2;
             x < mapWidth - 2;
             x++)
        {
            for (int y = 2;
                 y < mapHeight - 2;
                 y++)
            {
                if (mapData[x, y] != TileType.Empty)
                    continue;

                Vector2Int position =
                    new Vector2Int(x, y);

                if (IsRoomExteriorCorner(position))
                    continue;

                bool isCorner =
                    (IsWall(x + 1, y) &&
                     IsWall(x, y - 1) &&
                     IsFloor(x + 1, y - 1))

                    ||

                    (IsWall(x - 1, y) &&
                     IsWall(x, y - 1) &&
                     IsFloor(x - 1, y - 1))

                    ||

                    (IsWall(x + 1, y) &&
                     IsWall(x, y + 1) &&
                     IsFloor(x + 1, y + 1))

                    ||

                    (IsWall(x - 1, y) &&
                     IsWall(x, y + 1) &&
                     IsFloor(x - 1, y + 1));

                if (isCorner)
                    cornerWalls.Add(position);
            }
        }

        foreach (Vector2Int position in cornerWalls)
        {
            mapData[
                position.x,
                position.y
            ] = TileType.Wall;
        }
    }

    /// <summary>
    /// 해당 위치가 방의 외곽 코너에 해당하는지 확인한다.
    /// </summary>
    private bool IsRoomExteriorCorner(
        Vector2Int position)
    {
        foreach (Room room in rooms)
        {
            RectInt bounds = room.Bounds;

            bool isLeftOrRight =
                position.x == bounds.xMin - 1 ||
                position.x == bounds.xMax;

            bool isBottomOrTop =
                position.y == bounds.yMin - 1 ||
                position.y == bounds.yMax;

            if (isLeftOrRight &&
                isBottomOrTop)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// mapData의 각 셀을 Background, Floor, Wall Tilemap에 그린다.
    /// </summary>
    private void DrawMap()
    {
        for (int x = 0;
             x < mapWidth;
             x++)
        {
            for (int y = 0;
                 y < mapHeight;
                 y++)
            {
                Vector3Int cell =
                    new Vector3Int(x, y, 0);

                if (backgroundTilemap != null)
                    backgroundTilemap.SetTile(
                        cell,
                        blackTile
                    );

                if (mapData[x, y] == TileType.Floor)
                {
                    if (floorTilemap != null)
                    {
                        floorTilemap.SetTile(
                            cell,
                            floorTile
                        );
                    }
                }
                else if (mapData[x, y] == TileType.Wall)
                {
                    if (wallRenderTilemap != null)
                    {
                        wallRenderTilemap.SetTile(
                            cell,
                            GetWallTile(x, y)
                        );
                    }
                }
            }
        }
    }

    /// <summary>
    /// 주변 Floor와 Wall 배치를 분석하여
    /// 해당 위치에 사용할 Wall Tile을 결정한다.
    /// </summary>
    private TileBase GetWallTile(int x, int y)
    {
        bool floorUp = IsFloor(x, y + 1);
        bool floorDown = IsFloor(x, y - 1);
        bool floorLeft = IsFloor(x - 1, y);
        bool floorRight = IsFloor(x + 1, y);

        bool wallUp = IsWall(x, y + 1);
        bool wallDown = IsWall(x, y - 1);
        bool wallLeft = IsWall(x - 1, y);
        bool wallRight = IsWall(x + 1, y);

        // 인접한 두 방향에 Floor가 있으면 외부 코너 Tile을 사용한다.
        if (floorRight && floorDown)
            return outerCornerBottomRightTile;

        if (floorLeft && floorDown)
            return outerCornerBottomLeftTile;

        if (floorRight && floorUp)
            return outerCornerTopRightTile;

        if (floorLeft && floorUp)
            return outerCornerTopLeftTile;

        // 대각선 Floor와 양쪽 Wall이 있으면 내부 코너 Tile을 사용한다.
        if (IsFloor(x + 1, y - 1) &&
            wallRight &&
            wallDown)
        {
            return innerCornerBottomRightTile;
        }

        if (IsFloor(x - 1, y - 1) &&
            wallLeft &&
            wallDown)
        {
            return innerCornerBottomLeftTile;
        }

        if (IsFloor(x + 1, y + 1) &&
            wallRight &&
            wallUp)
        {
            return innerCornerTopRightTile;
        }

        if (IsFloor(x - 1, y + 1) &&
            wallLeft &&
            wallUp)
        {
            return innerCornerTopLeftTile;
        }

        // 한 방향에 Floor가 있으면 해당 방향에 맞는 직선 Wall Tile을 사용한다.
        if (floorDown)
            return topWallTile;

        if (floorUp)
            return bottomWallTile;

        if (floorLeft)
            return rightWallTile;

        if (floorRight)
            return leftWallTile;

        return topWallTile;
    }

    /// <summary>
    /// 해당 좌표가 Wall인지 확인한다.
    /// </summary>
    private bool IsWall(int x, int y)
    {
        return IsInBounds(x, y) &&
               mapData[x, y] == TileType.Wall;
    }

    /// <summary>
    /// 해당 좌표가 Floor인지 확인한다.
    /// </summary>
    private bool IsFloor(int x, int y)
    {
        return IsInBounds(x, y) &&
               mapData[x, y] == TileType.Floor;
    }

    /// <summary>
    /// 해당 좌표의 상하좌우에 Floor가 있는지 확인한다.
    /// </summary>
    private bool IsNearFloor(int x, int y)
    {
        return IsFloor(x + 1, y) ||
               IsFloor(x - 1, y) ||
               IsFloor(x, y + 1) ||
               IsFloor(x, y - 1);
    }

    /// <summary>
    /// 맵 셀 좌표를 월드 좌표로 변환한다.
    /// </summary>
    private Vector3 ToWorld(Vector2Int cell)
    {
        if (floorTilemap != null)
        {
            return floorTilemap.GetCellCenterWorld(
                new Vector3Int(cell.x, cell.y, 0)
            );
        }

        return new Vector3(
            cell.x + 0.5f,
            cell.y + 0.5f,
            0f
        );
    }

    /// <summary>
    /// 플레이어가 있는 방을 제외한 다른 방에 몬스터를 생성한다.
    /// </summary>
    private void SpawnActors()
    {
        if (rooms.Count == 0 ||
            monsterPrefab == null ||
            monsterCount == 0 ||
            floorTilemap == null)
        {
            return;
        }

        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogWarning(
                "Map: Player 태그를 가진 오브젝트가 없습니다."
            );

            return;
        }

        Vector2Int playerCell =
            WorldToCell(
                player.transform.position
            );

        Room playerRoom = null;

        foreach (Room room in rooms)
        {
            if (room.Bounds.Contains(playerCell))
            {
                playerRoom = room;
                break;
            }
        }

        if (playerRoom == null)
        {
            Debug.LogWarning(
                "Map: Player가 이 층의 방 안에 없습니다."
            );

            return;
        }

        if (rooms.Count <= 1)
            return;

        for (int i = 0;
             i < monsterCount;
             i++)
        {
            Room monsterRoom;

            do
            {
                monsterRoom =
                    rooms[
                        Random.Range(
                            0,
                            rooms.Count
                        )
                    ];
            }
            while (monsterRoom == playerRoom);

            Vector2Int cell =
                monsterRoom.GetRandomPosition();

            Vector3 worldPosition =
                floorTilemap.GetCellCenterWorld(
                    new Vector3Int(
                        cell.x,
                        cell.y,
                        0
                    )
                );

            GameObject monster =
                Instantiate(
                    monsterPrefab,
                    worldPosition,
                    Quaternion.identity
                );

            spawnedObjects.Add(monster);
        }
    }

    /// <summary>
    /// 기존 Tilemap과 생성된 몬스터/포탈 등을 제거한다.
    /// </summary>
    public void ClearPreviousMap()
    {
        if (backgroundTilemap != null)
        backgroundTilemap.ClearAllTiles();

    if (floorTilemap != null)
        floorTilemap.ClearAllTiles();

    if (wallRenderTilemap != null)
        wallRenderTilemap.ClearAllTiles();

        foreach (GameObject spawnedObject in spawnedObjects)
        {
            if (spawnedObject != null)
                Destroy(spawnedObject);
        }

        spawnedObjects.Clear();
    }

    /// <summary>
    /// 해당 좌표가 이동 가능한 Floor인지 확인한다.
    /// </summary>
    public bool IsWalkable(Vector2Int position)
    {
        return IsInBounds(position.x, position.y) &&
               mapData[position.x, position.y] ==
               TileType.Floor;
    }

    /// <summary>
    /// 방의 중심 좌표를 월드 좌표로 변환한다.
    /// </summary>
    public Vector3 GetRoomWorldPosition(Room room)
    {
        if (room == null)
            return Vector3.zero;

        return ToWorld(room.Center);
    }

    /// <summary>
    /// 좌표가 맵의 유효한 범위 안에 있는지 확인한다.
    /// </summary>
    private bool IsInBounds(int x, int y)
    {
        return x >= 0 &&
               x < mapWidth &&
               y >= 0 &&
               y < mapHeight;
    }

    /// <summary>
    /// 두 좌표 사이의 맨해튼 거리를 계산한다.
    /// </summary>
    private int ManhattanDistance(
        Vector2Int a,
        Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) +
               Mathf.Abs(a.y - b.y);
    }
}