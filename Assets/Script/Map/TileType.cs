/// <summary>
/// 맵 타일 종류
/// </summary>
public enum TileType
{
    Empty,  // 아직 생성되지 않은 공간

    Floor,  // 이동 가능한 바닥

    Wall,   // 이동 불가능한 벽

    Door,   // 문

    Stair   // 계단 / 출구
}