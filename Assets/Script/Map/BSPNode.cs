using UnityEngine;

/// <summary>
/// BSP(Binary Space Partitioning) 공간 분할 노드
/// </summary>
public class BSPNode
{
    public RectInt Area;

    public BSPNode Left;
    public BSPNode Right;

    public Room Room;

    public bool IsLeaf => Left == null && Right == null;


    public BSPNode(RectInt area)
    {
        Area = area;
    }


    /// <summary>
    /// 현재 영역을 두 개의 영역으로 분할
    /// </summary>
    public bool Split(int minNodeSize)
    {
        if (!IsLeaf)
            return false;


        bool splitHorizontal;


        if (Area.width > Area.height * 1.25f)
        {
            splitHorizontal = false;
        }
        else if (Area.height > Area.width * 1.25f)
        {
            splitHorizontal = true;
        }
        else
        {
            splitHorizontal = Random.value > 0.5f;
        }


        if (splitHorizontal)
        {
            if (Area.height < minNodeSize * 2)
                return false;


            int splitY = Random.Range(
                minNodeSize,
                Area.height - minNodeSize
            );


            Left = new BSPNode(
                new RectInt(
                    Area.x,
                    Area.y,
                    Area.width,
                    splitY
                )
            );


            Right = new BSPNode(
                new RectInt(
                    Area.x,
                    Area.y + splitY,
                    Area.width,
                    Area.height - splitY
                )
            );
        }
        else
        {
            if (Area.width < minNodeSize * 2)
                return false;


            int splitX = Random.Range(
                minNodeSize,
                Area.width - minNodeSize
            );


            Left = new BSPNode(
                new RectInt(
                    Area.x,
                    Area.y,
                    splitX,
                    Area.height
                )
            );


            Right = new BSPNode(
                new RectInt(
                    Area.x + splitX,
                    Area.y,
                    Area.width - splitX,
                    Area.height
                )
            );
        }


        return true;
    }


    /// <summary>
    /// 지정된 깊이까지 재귀 분할
    /// </summary>
    public void SplitRecursive(int depth, int minNodeSize)
    {
        if (depth <= 0)
            return;


        if (Split(minNodeSize))
        {
            Left.SplitRecursive(depth - 1, minNodeSize);
            Right.SplitRecursive(depth - 1, minNodeSize);
        }
    }


    /// <summary>
    /// Leaf 노드에 방 생성
    /// </summary>
    public void CreateRooms(int minRoomSize, int padding)
    {
        if (!IsLeaf)
        {
            Left?.CreateRooms(minRoomSize, padding);
            Right?.CreateRooms(minRoomSize, padding);
            return;
        }


        int maxWidth = Area.width - padding * 2;
        int maxHeight = Area.height - padding * 2;


        if (maxWidth < minRoomSize || maxHeight < minRoomSize)
            return;


        int width = Random.Range(
            minRoomSize,
            maxWidth
        );


        int height = Random.Range(
            minRoomSize,
            maxHeight
        );


        int x = Random.Range(
            Area.x + padding,
            Area.xMax - width - padding
        );


        int y = Random.Range(
            Area.y + padding,
            Area.yMax - height - padding
        );


        Room = new Room(
            new RectInt(
                x,
                y,
                width,
                height
            )
        );
    }


    /// <summary>
    /// 현재 노드 아래의 방 반환
    /// </summary>
    public Room GetRoom()
    {
        if (Room != null)
            return Room;


        Room leftRoom = null;
        Room rightRoom = null;


        if (Left != null)
            leftRoom = Left.GetRoom();


        if (Right != null)
            rightRoom = Right.GetRoom();


        if (leftRoom != null && rightRoom != null)
        {
            return Random.value > 0.5f
                ? leftRoom
                : rightRoom;
        }


        return leftRoom ?? rightRoom;
    }


    /// <summary>
    /// 두 자식 노드 사이의 방 연결용
    /// </summary>
    public void GetLeafRooms(System.Collections.Generic.List<Room> rooms)
    {
        if (IsLeaf)
        {
            if (Room != null)
                rooms.Add(Room);

            return;
        }


        Left?.GetLeafRooms(rooms);
        Right?.GetLeafRooms(rooms);
    }
}