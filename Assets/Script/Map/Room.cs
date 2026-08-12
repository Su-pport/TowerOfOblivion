using UnityEngine;

/// <summary>
/// BSP에서 생성되는 실제 방 데이터
/// </summary>
[System.Serializable]
public class Room
{
    // 방 영역
    public RectInt Bounds;


    // 방 방문 여부 (미니맵 / 저장용)
    public bool IsVisited;


    // 방 중심 위치
    public Vector2Int Center
    {
        get
        {
            return new Vector2Int(
                Bounds.x + Bounds.width / 2,
                Bounds.y + Bounds.height / 2
            );
        }
    }


    public Room(RectInt bounds)
    {
        Bounds = bounds;
        IsVisited = false;
    }


    /// <summary>
    /// 방 내부 랜덤 위치 반환
    /// </summary>
    public Vector2Int GetRandomPosition()
    {
        return new Vector2Int(
            Random.Range(
                Bounds.x + 1,
                Bounds.xMax - 1
            ),
            Random.Range(
                Bounds.y + 1,
                Bounds.yMax - 1
            )
        );
    }


    /// <summary>
    /// 해당 좌표가 방 내부인지 확인
    /// </summary>
    public bool Contains(Vector2Int position)
    {
        return Bounds.Contains(position);
    }


    /// <summary>
    /// 방 크기 반환
    /// </summary>
    public Vector2Int Size()
    {
        return new Vector2Int(
            Bounds.width,
            Bounds.height
        );
    }
}