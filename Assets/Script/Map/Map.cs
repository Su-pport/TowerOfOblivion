using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Creates a BSP dungeon, draws it on Tilemaps, and selects wall tiles from
/// the surrounding logical Floor / Wall / Empty cells.
/// Requires the existing BSPNode, Room, and TileType classes.
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
    [Tooltip("Wall with its floor directly below it.")]
    [SerializeField] private TileBase topWallTile;
    [Tooltip("Wall with its floor directly above it.")]
    [SerializeField] private TileBase bottomWallTile;
    [Tooltip("Wall with its floor directly to the right.")]
    [SerializeField] private TileBase leftWallTile;
    [Tooltip("Wall with its floor directly to the left.")]
    [SerializeField] private TileBase rightWallTile;

    [Header("Outer Corner Wall Tiles")]
    [Tooltip("Wall is at the top-left of the floor corner (floor is right and below).")]
    [SerializeField] private TileBase outerCornerTopLeftTile;
    [Tooltip("Wall is at the top-right of the floor corner (floor is left and below).")]
    [SerializeField] private TileBase outerCornerTopRightTile;
    [Tooltip("Wall is at the bottom-left of the floor corner (floor is right and above).")]
    [SerializeField] private TileBase outerCornerBottomLeftTile;
    [Tooltip("Wall is at the bottom-right of the floor corner (floor is left and above).")]
    [SerializeField] private TileBase outerCornerBottomRightTile;

    [Header("Inner Corner Wall Tiles")]
    [Tooltip("Wall is diagonally top-left of the floor (floor is down-right).")]
    [SerializeField] private TileBase innerCornerTopLeftTile;
    [Tooltip("Wall is diagonally top-right of the floor (floor is down-left).")]
    [SerializeField] private TileBase innerCornerTopRightTile;
    [Tooltip("Wall is diagonally bottom-left of the floor (floor is up-right).")]
    [SerializeField] private TileBase innerCornerBottomLeftTile;
    [Tooltip("Wall is diagonally bottom-right of the floor (floor is up-left).")]
    [SerializeField] private TileBase innerCornerBottomRightTile;

    [Header("Map Settings")]
    [Min(16)][SerializeField] private int mapWidth = 80;
    [Min(16)][SerializeField] private int mapHeight = 60;
    [Min(1)][SerializeField] private int maxDepth = 4;
    [Min(6)][SerializeField] private int minNodeSize = 15;
    [Min(4)][SerializeField] private int minRoomSize = 6;
    [Min(1)][SerializeField] private int roomPadding = 2;

    [Header("Extra Nearby Connections")]
    [Tooltip("Adds short shared corridor sections that can join three nearby rooms.")]
    [SerializeField] private bool createNearbySharedConnections = true;
    [Min(0)][SerializeField] private int maximumNearbySharedConnections = 2;
    [Min(1)][SerializeField] private int nearbyConnectionDistance = 18;

    [Header("Spawning (Optional)")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject monsterPrefab;
    [Min(0)][SerializeField] private int monsterCount = 6;

    private enum RoomSide { Top, Bottom, Left, Right }

    private TileType[,] mapData;
    private bool[,] visible;
    private bool[,] discovered;
    private BSPNode root;
    private readonly List<Room> rooms = new List<Room>();
    private readonly List<GameObject> spawnedObjects = new List<GameObject>();
    private readonly Dictionary<Room, HashSet<RoomSide>> usedRoomSides = new Dictionary<Room, HashSet<RoomSide>>();

    public TileType[,] MapData => mapData;
    public bool[,] Visible => visible;
    public bool[,] Discovered => discovered;
    public IReadOnlyList<Room> Rooms => rooms;

    private void Start() => GenerateMap();

    public void GenerateMap()
    {
        ClearPreviousMap();
        InitializeMapData();
        CreateDungeon();
        CreateWalls();
        CreateCornerWalls();
        DrawMap();
        SpawnActors();
    }

    private void InitializeMapData()
    {
        mapData = new TileType[mapWidth, mapHeight];
        visible = new bool[mapWidth, mapHeight];
        discovered = new bool[mapWidth, mapHeight];
        rooms.Clear();
        usedRoomSides.Clear();
    }

    private void CreateDungeon()
    {
        int border = roomPadding + 1;
        int usableWidth = mapWidth - border * 2;
        int usableHeight = mapHeight - border * 2;

        if (usableWidth < minNodeSize || usableHeight < minNodeSize)
        {
            Debug.LogError("Map is too small for the current BSP settings.");
            return;
        }

        root = new BSPNode(new RectInt(border, border, usableWidth, usableHeight));
        root.SplitRecursive(maxDepth, minNodeSize);
        root.CreateRooms(minRoomSize, roomPadding);
        root.GetLeafRooms(rooms);

        foreach (Room room in rooms)
            CarveRoom(room.Bounds);

        ConnectNodeChildren(root);

        if (createNearbySharedConnections)
            CreateNearbySharedConnections();
    }

    // Each BSP split still joins its two child groups, preserving the normal BSP tree connectivity.
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

    private List<Room> GetRoomsBelow(BSPNode node)
    {
        List<Room> result = new List<Room>();
        if (node != null)
            node.GetLeafRooms(result);
        return result;
    }

    private bool TryConnectRoomGroups(List<Room> firstGroup, List<Room> secondGroup)
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
                if (!TryGetFacingSides(first, second, out firstSide, out secondSide) ||
                    !CanUseSide(first, firstSide) || !CanUseSide(second, secondSide))
                    continue;

                int score = ManhattanDistance(first.Center, second.Center);
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

        CarveRoomConnection(bestFirst, bestFirstSide, bestSecond, bestSecondSide);
        ReserveSide(bestFirst, bestFirstSide);
        ReserveSide(bestSecond, bestSecondSide);
        return true;
    }

    private bool TryGetFacingSides(Room first, Room second, out RoomSide firstSide, out RoomSide secondSide)
    {
        Vector2Int delta = second.Center - first.Center;
        if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
        {
            firstSide = delta.x >= 0 ? RoomSide.Right : RoomSide.Left;
            secondSide = delta.x >= 0 ? RoomSide.Left : RoomSide.Right;
        }
        else
        {
            firstSide = delta.y >= 0 ? RoomSide.Top : RoomSide.Bottom;
            secondSide = delta.y >= 0 ? RoomSide.Bottom : RoomSide.Top;
        }
        return true;
    }

    private void CarveRoomConnection(Room first, RoomSide firstSide, Room second, RoomSide secondSide)
    {
        Vector2Int start = GetDoorCell(first.Bounds, firstSide, second.Center);
        Vector2Int end = GetDoorCell(second.Bounds, secondSide, first.Center);

        // Use the dominant separation so a corridor leaves each room through its reserved side.
        if (firstSide == RoomSide.Left || firstSide == RoomSide.Right)
        {
            CarveHorizontal(start.x, end.x, start.y);
            CarveVertical(start.y, end.y, end.x);
        }
        else
        {
            CarveVertical(start.y, end.y, start.x);
            CarveHorizontal(start.x, end.x, end.y);
        }
    }

    private Vector2Int GetDoorCell(RectInt bounds, RoomSide side, Vector2Int toward)
    {
        switch (side)
        {
            case RoomSide.Top:
                return new Vector2Int(ClampTwoWide(toward.x, bounds.xMin, bounds.xMax), bounds.yMax - 1);
            case RoomSide.Bottom:
                return new Vector2Int(ClampTwoWide(toward.x, bounds.xMin, bounds.xMax), bounds.yMin);
            case RoomSide.Left:
                return new Vector2Int(bounds.xMin, ClampTwoWide(toward.y, bounds.yMin, bounds.yMax));
            default:
                return new Vector2Int(bounds.xMax - 1, ClampTwoWide(toward.y, bounds.yMin, bounds.yMax));
        }
    }

    // The returned coordinate is always valid for a two-cell-wide segment.
    private int ClampTwoWide(int value, int min, int maxExclusive)
    {
        return Mathf.Clamp(value, min, Mathf.Max(min, maxExclusive - 2));
    }

    private bool CanUseSide(Room room, RoomSide side)
    {
        HashSet<RoomSide> sides;
        return !usedRoomSides.TryGetValue(room, out sides) || !sides.Contains(side);
    }

    private void ReserveSide(Room room, RoomSide side)
    {
        HashSet<RoomSide> sides;
        if (!usedRoomSides.TryGetValue(room, out sides))
        {
            sides = new HashSet<RoomSide>();
            usedRoomSides.Add(room, sides);
        }
        sides.Add(side);
    }

    // Adds a T-shaped shared corridor: two rooms share one horizontal trunk and a third nearby room joins it.
    private void CreateNearbySharedConnections()
    {
        int created = 0;
        for (int i = 0; i < rooms.Count && created < maximumNearbySharedConnections; i++)
        {
            for (int j = i + 1; j < rooms.Count && created < maximumNearbySharedConnections; j++)
            {
                if (TryCreateSharedHorizontalConnection(rooms[i], rooms[j]))
                    created++;
            }
        }
    }

    private bool TryCreateSharedHorizontalConnection(Room a, Room b)
    {
        Room left = a.Center.x <= b.Center.x ? a : b;
        Room right = left == a ? b : a;
        RectInt leftBounds = left.Bounds;
        RectInt rightBounds = right.Bounds;

        if (!CanUseSide(left, RoomSide.Right) || !CanUseSide(right, RoomSide.Left) ||
            rightBounds.xMin <= leftBounds.xMax || rightBounds.xMin - leftBounds.xMax > nearbyConnectionDistance)
            return false;

        int sharedMinY = Mathf.Max(leftBounds.yMin, rightBounds.yMin);
        int sharedMaxY = Mathf.Min(leftBounds.yMax - 2, rightBounds.yMax - 2);
        if (sharedMinY > sharedMaxY)
            return false;

        int trunkY = Mathf.Clamp((left.Center.y + right.Center.y) / 2, sharedMinY, sharedMaxY);
        Room branch = FindNearbyBranchRoom(left, right, trunkY);
        if (branch == null)
            return false;

        Vector2Int leftDoor = new Vector2Int(leftBounds.xMax - 1, trunkY);
        Vector2Int rightDoor = new Vector2Int(rightBounds.xMin, trunkY);
        CarveHorizontal(leftDoor.x, rightDoor.x, trunkY);
        ReserveSide(left, RoomSide.Right);
        ReserveSide(right, RoomSide.Left);

        bool branchIsAbove = branch.Center.y > trunkY;
        RoomSide branchSide = branchIsAbove ? RoomSide.Bottom : RoomSide.Top;
        RectInt branchBounds = branch.Bounds;
        int branchX = ClampTwoWide((leftDoor.x + rightDoor.x) / 2, branchBounds.xMin, branchBounds.xMax);
        int branchY = branchIsAbove ? branchBounds.yMin : branchBounds.yMax - 1;
        CarveVertical(trunkY, branchY, branchX);
        ReserveSide(branch, branchSide);
        return true;
    }

    private Room FindNearbyBranchRoom(Room left, Room right, int trunkY)
    {
        int trunkMinX = left.Bounds.xMax - 1;
        int trunkMaxX = right.Bounds.xMin;
        foreach (Room candidate in rooms)
        {
            if (candidate == left || candidate == right)
                continue;

            RectInt bounds = candidate.Bounds;
            int candidateX = Mathf.Clamp((trunkMinX + trunkMaxX) / 2, bounds.xMin, bounds.xMax - 1);
            if (candidateX < trunkMinX || candidateX > trunkMaxX)
                continue;

            bool above = bounds.yMin > trunkY;
            bool below = bounds.yMax - 1 < trunkY;
            if (!above && !below)
                continue;

            int gap = above ? bounds.yMin - trunkY : trunkY - (bounds.yMax - 1);
            RoomSide side = above ? RoomSide.Bottom : RoomSide.Top;
            if (gap <= nearbyConnectionDistance && CanUseSide(candidate, side))
                return candidate;
        }
        return null;
    }

    private void CarveRoom(RectInt bounds)
    {
        for (int x = bounds.xMin; x < bounds.xMax; x++)
            for (int y = bounds.yMin; y < bounds.yMax; y++)
                SetFloor(x, y);
    }

    private void CarveHorizontal(int x1, int x2, int y)
    {
        int from = Mathf.Min(x1, x2);
        int to = Mathf.Max(x1, x2);
        for (int x = from; x <= to; x++)
        {
            SetFloor(x, y);
            SetFloor(x, y + 1);
        }
    }

    private void CarveVertical(int y1, int y2, int x)
    {
        int from = Mathf.Min(y1, y2);
        int to = Mathf.Max(y1, y2);
        for (int y = from; y <= to; y++)
        {
            SetFloor(x, y);
            SetFloor(x + 1, y);
        }
    }

    private void SetFloor(int x, int y)
    {
        if (IsInBounds(x, y)) mapData[x, y] = TileType.Floor;
    }

    private void CreateWalls()
    {
        List<Vector2Int> wallPositions = new List<Vector2Int>();
        for (int x = 1; x < mapWidth - 1; x++)
            for (int y = 1; y < mapHeight - 1; y++)
                if (mapData[x, y] == TileType.Empty && IsNearFloor(x, y))
                    wallPositions.Add(new Vector2Int(x, y));

        foreach (Vector2Int position in wallPositions)
            mapData[position.x, position.y] = TileType.Wall;
    }

    private void CreateCornerWalls()
    {
        List<Vector2Int> cornerWalls = new List<Vector2Int>();

        // Keep the complete 3x3 corner pattern away from the outer map boundary.
        for (int x = 2; x < mapWidth - 2; x++)
        {
            for (int y = 2; y < mapHeight - 2; y++)
            {
                if (mapData[x, y] != TileType.Empty)
                    continue;

                Vector2Int position = new Vector2Int(x, y);
                // Room perimeter corners stay Empty; corridor bends do not match a room corner.
                if (IsRoomExteriorCorner(position))
                    continue;

                if ((IsWall(x + 1, y) && IsWall(x, y - 1) && IsFloor(x + 1, y - 1)) ||
                    (IsWall(x - 1, y) && IsWall(x, y - 1) && IsFloor(x - 1, y - 1)) ||
                    (IsWall(x + 1, y) && IsWall(x, y + 1) && IsFloor(x + 1, y + 1)) ||
                    (IsWall(x - 1, y) && IsWall(x, y + 1) && IsFloor(x - 1, y + 1)))
                    cornerWalls.Add(position);
            }
        }

        foreach (Vector2Int position in cornerWalls)
            mapData[position.x, position.y] = TileType.Wall;
    }

    private bool IsRoomExteriorCorner(Vector2Int position)
    {
        foreach (Room room in rooms)
        {
            RectInt bounds = room.Bounds;
            bool isLeftOrRight = position.x == bounds.xMin - 1 || position.x == bounds.xMax;
            bool isBottomOrTop = position.y == bounds.yMin - 1 || position.y == bounds.yMax;
            if (isLeftOrRight && isBottomOrTop)
                return true;
        }

        return false;
    }

    private void DrawMap()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                backgroundTilemap?.SetTile(cell, blackTile);
                if (mapData[x, y] == TileType.Floor)
                    floorTilemap?.SetTile(cell, floorTile);
                else if (mapData[x, y] == TileType.Wall)
                    wallRenderTilemap?.SetTile(cell, GetWallTile(x, y));
            }
        }
    }

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

        // Priority 1: direct perpendicular Floor contacts are the most specific outer corners.
        if (floorRight && floorDown) return outerCornerBottomRightTile;
        if (floorLeft && floorDown) return outerCornerBottomLeftTile;
        if (floorRight && floorUp) return outerCornerTopRightTile;
        if (floorLeft && floorUp) return outerCornerTopLeftTile;

        // Priority 2: diagonal Floor enclosed by two Wall cells is an inner-corner fill.
        if (IsFloor(x + 1, y - 1) && wallRight && wallDown) return innerCornerBottomRightTile;
        if (IsFloor(x - 1, y - 1) && wallLeft && wallDown) return innerCornerBottomLeftTile;
        if (IsFloor(x + 1, y + 1) && wallRight && wallUp) return innerCornerTopRightTile;
        if (IsFloor(x - 1, y + 1) && wallLeft && wallUp) return innerCornerTopLeftTile;

        // Priority 3: straight faces. Opposite Floor contacts are resolved in a fixed order.
        if (floorDown) return topWallTile;
        if (floorUp) return bottomWallTile;
        if (floorLeft) return rightWallTile;
        if (floorRight) return leftWallTile;

        // Priority 4: defensive fallback for a wall created only as a corner support cell.
        if (wallDown || wallLeft || wallRight || wallUp) return topWallTile;
        return topWallTile;
    }

    private bool IsWall(int x, int y) => IsInBounds(x, y) && mapData[x, y] == TileType.Wall;
    private bool IsFloor(int x, int y) => IsInBounds(x, y) && mapData[x, y] == TileType.Floor;
    private bool IsNearFloor(int x, int y) => IsFloor(x + 1, y) || IsFloor(x - 1, y) || IsFloor(x, y + 1) || IsFloor(x, y - 1);

    private void SpawnActors()
    {
        if (rooms.Count == 0)
            return;

        Room playerRoom = null;

        // 기존 Player가 있다면 랜덤 방의 랜덤 위치로 이동
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerRoom = rooms[Random.Range(0, rooms.Count)];

            Vector2Int spawnPosition = playerRoom.GetRandomPosition();
            player.transform.position = ToWorld(spawnPosition);
        }

        // Player가 없다면 아무것도 하지 않음
        if (playerRoom == null)
            return;

        // Player가 있는 방을 제외하고 몬스터 생성
        if (monsterPrefab == null || rooms.Count <= 1)
            return;

        for (int i = 0; i < monsterCount; i++)
        {
            Room monsterRoom;

            do
            {
                monsterRoom = rooms[Random.Range(0, rooms.Count)];
            }
            while (monsterRoom == playerRoom);

            Vector2Int position = monsterRoom.GetRandomPosition();

            spawnedObjects.Add(
                Instantiate(
                    monsterPrefab,
                    ToWorld(position),
                    Quaternion.identity
                )
            );
        }
    }

    private Vector3 ToWorld(Vector2Int cell) => new Vector3(cell.x + 0.5f, cell.y + 0.5f, 0f);

    private void ClearPreviousMap()
    {
        backgroundTilemap?.ClearAllTiles();
        floorTilemap?.ClearAllTiles();
        wallRenderTilemap?.ClearAllTiles();
        foreach (GameObject spawnedObject in spawnedObjects)
            if (spawnedObject != null) Destroy(spawnedObject);
        spawnedObjects.Clear();
    }

    public bool IsWalkable(Vector2Int position) => IsInBounds(position.x, position.y) && mapData[position.x, position.y] == TileType.Floor;
    private bool IsInBounds(int x, int y) => x >= 0 && x < mapWidth && y >= 0 && y < mapHeight;
    private int ManhattanDistance(Vector2Int a, Vector2Int b) => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
}
