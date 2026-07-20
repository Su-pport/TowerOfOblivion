git using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BSPTilemapGenerator : MonoBehaviour
{
    public enum TileType
    {
        Empty,      // 아무 것도 없는 공간
        Floor,      // 방/복도 바닥
        Wall,       // 벽
        Dark,       // 아직 보지 못한 암흑 상태
        Visible,    // 현재 플레이어 시야에 들어온 타일
        Explored    // 과거에 봤지만 지금은 시야 밖인 타일 (흐릿하게 표시)
    }


    [Header("Prefabs")]
    public GameObject monsterPrefab;
    public int monsterCount = 5;

    [Header("Map Settings")]
    public int mapWidth = 50;
    public int mapHeight = 50;
    public int minRoomSize = 6;
    public int maxIterations = 5;

    [Header("Tilemap Settings")]
    public Tilemap floorTilemap;
    public Tilemap wallTilemap;
    public TileBase floorTile;
    public TileBase wallTile;

    private List<Node> nodes = new List<Node>();
    private List<RectInt> rooms = new List<RectInt>();
    private TileType[,] mapData;

    List<Vector3Int> corridorTiles = new List<Vector3Int>();    // 복도는 바닥만 찍고, 벽은 나중에 DrawWalls에서 처리

    int visionRadius = 6; // 시야 반경

    // Tilemap Renderer에 적용할 색상
    Color darkColor = Color.black;              // 완전 암흑
    Color visibleColor = Color.white;           // 밝게 표시
    Color exploredColor = new Color(0.5f, 0.5f, 0.5f); // 흐릿한 회색


    void Start()
    {
        int testSeed = 12345; // 🧩 테스트용 시드 값 (고정)
        GenerateMap(testSeed); // 시드 기반 맵 생성
        //CreateMapBorder(); // 맵 외벽 생성
        
        // RenderMap(); // 초기 렌더링
    }

    void GenerateMap(int seed)
    {
        Random.InitState(seed); // 🎲 시드 초기화: 같은 시드면 항상 동일한 맵 생성

        // 이후 모든 Random.Range 호출은 동일한 시퀀스를 따름
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();

        nodes.Clear();
        rooms.Clear();

        mapData = new TileType[mapWidth, mapHeight];

        Node root = new Node(new RectInt(0, 0, mapWidth, mapHeight));
        nodes.Add(root);

        Split(root, maxIterations);

        foreach (Node node in nodes)
        {
            if (node.IsLeaf())
            {
                RectInt room = CreateRoom(node.rect);
                node.room = room;
                rooms.Add(room);
            }
        }

        ConnectRooms(root);
        EnsureAllRoomsConnected();

        DrawRooms();       // 방 바닥만 생성
        DrawCorridors();   // 복도 바닥만 생성
        DrawWalls();       // 마지막에 빈 칸을 벽으로 채움

        SpawnMonsters();
    }

    // BSP 분할
    void Split(Node node, int iterations)
    {
        if (iterations <= 0 || (node.rect.width < minRoomSize * 2 && node.rect.height < minRoomSize * 2))
            return;

        bool splitHorizontally = Random.value > 0.5f;

        if (node.rect.width > node.rect.height && node.rect.width / node.rect.height >= 1.25f)
            splitHorizontally = false;
        else if (node.rect.height > node.rect.width && node.rect.height / node.rect.width >= 1.25f)
            splitHorizontally = true;

        if (splitHorizontally)
        {
            int split = Random.Range(minRoomSize, node.rect.height - minRoomSize);
            node.left = new Node(new RectInt(node.rect.x, node.rect.y, node.rect.width, split));
            node.right = new Node(new RectInt(node.rect.x, node.rect.y + split, node.rect.width, node.rect.height - split));
        }
        else
        {
            int split = Random.Range(minRoomSize, node.rect.width - minRoomSize);
            node.left = new Node(new RectInt(node.rect.x, node.rect.y, split, node.rect.height));
            node.right = new Node(new RectInt(node.rect.x + split, node.rect.y, node.rect.width - split, node.rect.height));
        }

        nodes.Add(node.left);
        nodes.Add(node.right);

        Split(node.left, iterations - 1);
        Split(node.right, iterations - 1);
    }

    RectInt CreateRoom(RectInt rect)
    {
        // 방 크기를 최소/최대 범위 내에서 안전하게 설정
        int roomWidth = Random.Range(minRoomSize, Mathf.Max(minRoomSize, rect.width));
        int roomHeight = Random.Range(minRoomSize, Mathf.Max(minRoomSize, rect.height));

        // 방 크기가 rect보다 커지지 않도록 보정
        roomWidth = Mathf.Min(roomWidth, rect.width);
        roomHeight = Mathf.Min(roomHeight, rect.height);

        // 방 위치를 rect 내부에서 안전하게 랜덤 배치
        int roomX = rect.x + Random.Range(0, Mathf.Max(1, rect.width - roomWidth));
        int roomY = rect.y + Random.Range(0, Mathf.Max(1, rect.height - roomHeight));

        // 맵 경계 안쪽으로 보정
        if (roomX + roomWidth > mapWidth)
            roomX = mapWidth - roomWidth;
        if (roomY + roomHeight > mapHeight)
            roomY = mapHeight - roomHeight;

        return new RectInt(roomX, roomY, roomWidth, roomHeight);
    }

    void ConnectRooms(Node node)
    {
        if (node.left != null && node.right != null)
        {
            ConnectRooms(node.left);
            ConnectRooms(node.right);

            Vector2Int leftCenter = node.left.GetRoomCenter();
            Vector2Int rightCenter = node.right.GetRoomCenter();

            CreateCorridor(leftCenter, rightCenter);
        }
    }
    void CreateMapBorder()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            // 위쪽과 아래쪽 라인
            mapData[x, 0] = TileType.Wall;
            mapData[x, mapHeight - 1] = TileType.Wall;
        }

        for (int y = 0; y < mapHeight; y++)
        {
            // 왼쪽과 오른쪽 라인
            mapData[0, y] = TileType.Wall;
            mapData[mapWidth - 1, y] = TileType.Wall;
        }
    }



    // MST 기반 연결 + 추가 연결 (성능 최적화)
    void EnsureAllRoomsConnected()
    {
        var centers = rooms.Select(r => new Vector2Int(r.x + r.width / 2, r.y + r.height / 2)).ToList();

        // 모든 간선 계산 (O(n²)) → 성능이 문제되면 근처 방만 후보로 제한 가능
        var edges = new List<(int, int, float)>();
        for (int i = 0; i < centers.Count; i++)
        {
            for (int j = i + 1; j < centers.Count; j++)
            {
                float dist = Vector2Int.Distance(centers[i], centers[j]);
                edges.Add((i, j, dist));
            }
        }
        edges.Sort((a, b) => a.Item3.CompareTo(b.Item3));

        // Kruskal MST
        int[] parent = Enumerable.Range(0, centers.Count).ToArray();
        int Find(int x) => parent[x] == x ? x : (parent[x] = Find(parent[x]));
        void Union(int a, int b) => parent[Find(a)] = Find(b);

        foreach (var (a, b, _) in edges)
            if (Find(a) != Find(b)) { Union(a, b); CreateCorridor(centers[a], centers[b]); }

        // 🔥 추가 연결: 방 개수의 1/5만 연결, 최소 거리 조건 강화
        for (int i = 0; i < centers.Count / 5; i++)
        {
            int a = Random.Range(0, centers.Count);
            int b = Random.Range(0, centers.Count);

            if (a != b && (centers[a] - centers[b]).sqrMagnitude > minRoomSize * minRoomSize)
                CreateCorridor(centers[a], centers[b]);
        }
    }

    // 복도 생성 (직선/L자/Z자 랜덤)
    void CreateCorridor(Vector2Int a, Vector2Int b)
    {
        int style = Random.Range(0, 5); // 0=직선, 1~2=L자, 3~4=Z자

        if (style == 0) // 직선
            DrawLine(a, b, Random.value > 0.5f ? "x" : "y");

        else if (style <= 2) // L자
        {
            DrawLine(a, new Vector2Int(b.x, a.y), "x");
            DrawLine(new Vector2Int(b.x, a.y), b, "y");
        }
        else // Z자
        {
            var mid = new Vector2Int(
                Mathf.Clamp((a.x + b.x) / 2 + Random.Range(-3, 3), 0, mapWidth - 1),
                Mathf.Clamp((a.y + b.y) / 2 + Random.Range(-3, 3), 0, mapHeight - 1)
            );
            DrawLine(a, mid, "x");
            DrawLine(mid, b, "y");
        }
    }

    // 공통 라인 그리기 (경계 검증 포함)
    void DrawLine(Vector2Int start, Vector2Int end, string axis)
    {
        if (axis == "x")
        {
            for (int x = Mathf.Min(start.x, end.x); x <= Mathf.Max(start.x, end.x); x++)
                if (IsInsideMap(x, start.y)) AddCorridorTile(x, start.y);
        }
        else
        {
            for (int y = Mathf.Min(start.y, end.y); y <= Mathf.Max(start.y, end.y); y++)
                if (IsInsideMap(start.x, y)) AddCorridorTile(start.x, y);
        }
    }




    // 안전하게 corridorTiles에 추가하는 헬퍼 함수
    void AddCorridorTile(int x, int y)
{
    if (IsInsideMap(x, y))
    {
        corridorTiles.Add(new Vector3Int(x, y, 0));
    }
}

    void DrawRooms()
    {
        foreach (RectInt room in rooms)
        {
            for (int x = room.x; x < room.x + room.width; x++)
            {
                for (int y = room.y; y < room.y + room.height; y++)
                {
                    floorTilemap.SetTile(new Vector3Int(x, y, 0), floorTile);
                    mapData[x, y] = TileType.Floor;
                }
            }
        }
    }

    void DrawCorridors()
    {
        foreach (var pos in corridorTiles)
        {
            floorTilemap.SetTile(pos, floorTile);
            if (IsInsideMap(pos.x, pos.y))
                mapData[pos.x, pos.y] = TileType.Floor;
        }
    }

    void DrawWalls()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (mapData[x, y] == TileType.Empty)
                {
                    wallTilemap.SetTile(new Vector3Int(x, y, 0), wallTile);
                    mapData[x, y] = TileType.Wall;
                }
            }
        }
    }

   


    void SpawnMonsters()
    {
        for (int i = 0; i < monsterCount; i++)
        {
            RectInt room = rooms[Random.Range(0, rooms.Count)];
            int spawnX = Random.Range(room.x, room.x + room.width);
            int spawnY = Random.Range(room.y, room.y + room.height);
            Vector2Int spawnPos = new Vector2Int(spawnX, spawnY);

            GameObject monster = Instantiate(monsterPrefab, new Vector3(spawnPos.x, spawnPos.y, 0), Quaternion.identity);

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Enemy enemyScript = monster.GetComponent<Enemy>();
                enemyScript.targetUnit = player;
            }
        }
    }

    bool IsInsideMap(int x, int y)
    {
        return x >= 0 && x < mapWidth && y >= 0 && y < mapHeight;
    }

    // 시야 업데이트
    void UpdateVision(Vector2Int playerPos)
    {
        // 기존 Visible → Explored로 변경
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (mapData[x, y] == TileType.Visible)
                    mapData[x, y] = TileType.Explored;
            }
        }

        // 360도 방향으로 광선 쏘기
        for (int angle = 0; angle < 360; angle++)
        {
            float rad = angle * Mathf.Deg2Rad;
            float dx = Mathf.Cos(rad);
            float dy = Mathf.Sin(rad);

            float x = playerPos.x;
            float y = playerPos.y;

            for (int step = 0; step <= visionRadius; step++)
            {
                int ix = Mathf.RoundToInt(x);
                int iy = Mathf.RoundToInt(y);

                if (!IsInsideMap(ix, iy)) break;

                // 현재 타일을 Visible로 표시
                mapData[ix, iy] = TileType.Visible;

                // 벽 만나면 그 뒤는 밝히지 않음
                if (mapData[ix, iy] == TileType.Wall) break;

                x += dx;
                y += dy;
            }
        }
    }
    void RenderMap(Tilemap tilemap)
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);

                switch (mapData[x, y])
                {
                    case TileType.Wall:
                        tilemap.SetTile(pos, wallTile);
                        tilemap.SetColor(pos, Color.gray); // 벽은 항상 보이게
                        break;

                    case TileType.Floor:
                    case TileType.Visible:
                        tilemap.SetTile(pos, floorTile);
                        tilemap.SetColor(pos, visibleColor);
                        break;

                    case TileType.Explored:
                        tilemap.SetTile(pos, floorTile);
                        tilemap.SetColor(pos, exploredColor);
                        break;

                    case TileType.Dark:
                        tilemap.SetTile(pos, floorTile);
                        tilemap.SetColor(pos, darkColor);
                        break;
                }
            }
        }
    }

    public class Node
    {
        public RectInt rect;
        public Node left;
        public Node right;
        public RectInt room;

        public Node(RectInt rect) { this.rect = rect; }
        public bool IsLeaf() => left == null && right == null;
        public Vector2Int GetRoomCenter() => new Vector2Int(room.x + room.width / 2, room.y + room.height / 2);
    }
}
