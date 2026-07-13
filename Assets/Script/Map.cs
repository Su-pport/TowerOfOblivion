using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BSPTilemapGenerator : MonoBehaviour
{
    public enum TileType { Empty, Floor, Wall }

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

    void Start()
    {
        GenerateMap();
    }

    void GenerateMap()
    {
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

    // 복도는 바닥만 찍고, 벽은 나중에 DrawWalls에서 처리
    List<Vector3Int> corridorTiles = new List<Vector3Int>();
    void CreateCorridor(Vector2Int a, Vector2Int b)
    {
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

    void EnsureAllRoomsConnected()
    {
        List<Vector2Int> centers = new List<Vector2Int>();
        foreach (RectInt room in rooms)
        {
            centers.Add(new Vector2Int(room.x + room.width / 2, room.y + room.height / 2));
        }

        for (int i = 0; i < centers.Count - 1; i++)
        {
            CreateCorridor(centers[i], centers[i + 1]);
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
