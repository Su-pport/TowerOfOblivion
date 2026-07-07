using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BSPTilemapGenerator : MonoBehaviour
{
    public GameObject monsterPrefab; // 몬스터 프리팹
    public int monsterCount = 5;

    [Header("Map Settings")]
    public int mapWidth = 50;          // 전체 맵의 가로 크기
    public int mapHeight = 50;         // 전체 맵의 세로 크기
    public int minRoomSize = 6;        // 최소 방 크기
    public int maxIterations = 5;      // BSP 분할 횟수, 방 갯수

    [Header("Tilemap Settings")]
    public Tilemap tilemap;            // Tilemap 컴포넌트
    public TileBase floorTile;         // 바닥 타일
    public TileBase wallTile;          // 벽 타일

    private List<Node> nodes = new List<Node>();   // BSP 노드 리스트
    private List<RectInt> rooms = new List<RectInt>(); // 생성된 방 리스트

    void Start()
    {
        GenerateMap(); // 시작 시 맵 생성
    }

    void GenerateMap()
    {
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
        DrawMap();
        //AddBoundaryWalls();
        ConnectRooms(root);
        EnsureAllRoomsConnected(); // 추가 연결

        SpawnMonsters(); // 몹 스폰

    }

    /// <summary>
    /// BSP 분할 함수 (재귀적으로 맵을 나눔)
    /// </summary>
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
        int roomWidth = Random.Range(minRoomSize, rect.width);
        int roomHeight = Random.Range(minRoomSize, rect.height);
        int roomX = rect.x + Random.Range(0, rect.width - roomWidth);
        int roomY = rect.y + Random.Range(0, rect.height - roomHeight);

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

    /// <summary>
    /// 두 방을 L자 형태 복도로 연결 (복도 폭 2타일)
    /// </summary>
    void CreateCorridor(Vector2Int a, Vector2Int b)
    {
        List<Vector3Int> corridorTiles = new List<Vector3Int>();

        if (Random.value > 0.5f)
        {
            for (int x = Mathf.Min(a.x, b.x); x <= Mathf.Max(a.x, b.x); x++)
            {
                corridorTiles.Add(new Vector3Int(x, a.y, 0));
                corridorTiles.Add(new Vector3Int(x, a.y + 1, 0));
            }

            for (int y = Mathf.Min(a.y, b.y); y <= Mathf.Max(a.y, b.y); y++)
            {
                corridorTiles.Add(new Vector3Int(b.x, y, 0));
                corridorTiles.Add(new Vector3Int(b.x + 1, y, 0));
            }
        }
        else
        {
            for (int y = Mathf.Min(a.y, b.y); y <= Mathf.Max(a.y, b.y); y++)
            {
                corridorTiles.Add(new Vector3Int(a.x, y, 0));
                corridorTiles.Add(new Vector3Int(a.x + 1, y, 0));
            }

            for (int x = Mathf.Min(a.x, b.x); x <= Mathf.Max(a.x, b.x); x++)
            {
                corridorTiles.Add(new Vector3Int(x, b.y, 0));
                corridorTiles.Add(new Vector3Int(x, b.y + 1, 0));
            }
        }

        foreach (var pos in corridorTiles)
            tilemap.SetTile(pos, floorTile);

        foreach (var pos in corridorTiles)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    Vector3Int wallPos = new Vector3Int(pos.x + dx, pos.y + dy, 0);
                    if (tilemap.GetTile(wallPos) == null)
                        tilemap.SetTile(wallPos, wallTile);
                }
            }
        }
    }

    void DrawMap()
    {
        foreach (RectInt room in rooms)
        {
            for (int x = room.x; x < room.x + room.width; x++)
            {
                for (int y = room.y; y < room.y + room.height; y++)
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), floorTile);
                }
            }
        }

        foreach (RectInt room in rooms)
        {
            for (int x = room.x - 1; x <= room.x + room.width; x++)
            {
                for (int y = room.y - 1; y <= room.y + room.height; y++)
                {
                    Vector3Int pos = new Vector3Int(x, y, 0);
                    if (tilemap.GetTile(pos) == null)
                    {
                        tilemap.SetTile(pos, wallTile);
                    }
                }
            }
        }
    }

    void EnsureAllRoomsConnected()
    {
        // 모든 방 중심 좌표 모으기
        List<Vector2Int> centers = new List<Vector2Int>();
        foreach (RectInt room in rooms)
        {
            centers.Add(new Vector2Int(room.x + room.width / 2, room.y + room.height / 2));
        }

        // 가까운 방끼리 연결 (간단한 방식)
        for (int i = 0; i < centers.Count - 1; i++)
        {
            CreateCorridor(centers[i], centers[i + 1]);
        }
    }

    void SpawnMonsters()
    {
        for (int i = 0; i < monsterCount; i++)
        {
            // 랜덤 방 선택
            RectInt room = rooms[Random.Range(0, rooms.Count)];
            Vector2Int center = new Vector2Int(room.x + room.width / 2, room.y + room.height / 2);

            // 몬스터 생성
            GameObject monster = Instantiate(monsterPrefab, new Vector3(center.x, center.y, 0), Quaternion.identity);

            // 플레이어를 타겟으로 지정 (Player 태그 필요)
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Enemy enemyScript = monster.GetComponent<Enemy>();
                enemyScript.targetUnit = player;
            }
        }
    }



    void AddBoundaryWalls()
    {
        for (int x = -1; x <= mapWidth; x++)
        {
            tilemap.SetTile(new Vector3Int(x, -1, 0), wallTile);
            tilemap.SetTile(new Vector3Int(x, mapHeight, 0), wallTile);
        }
        for (int y = -1; y <= mapHeight; y++)
        {
            tilemap.SetTile(new Vector3Int(-1, y, 0), wallTile);
            tilemap.SetTile(new Vector3Int(mapWidth, y, 0), wallTile);
        }
    }

    /// <summary>
    /// BSP 노드 클래스
    /// </summary>
    public class Node
    {
        public RectInt rect;   // 노드 영역
        public Node left;      // 왼쪽 자식 노드
        public Node right;     // 오른쪽 자식 노드
        public RectInt room;   // 방 정보

        public Node(RectInt rect)
        {
            this.rect = rect;
        }

        public bool IsLeaf()
        {
            return left == null && right == null;
        }

        public Vector2Int GetRoomCenter()
        {
            return new Vector2Int(room.x + room.width / 2, room.y + room.height / 2);
        }
    }
}
