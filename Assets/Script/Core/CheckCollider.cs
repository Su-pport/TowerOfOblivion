using UnityEngine;

[RequireComponent(typeof(CapsuleCollider2D))]
public class CheckCollider : MonoBehaviour
{
    public Color objectColor = Color.white;

    void OnDrawGizmos()
    {
        CapsuleCollider2D col = GetComponent<CapsuleCollider2D>();
        Gizmos.color = objectColor;
        Gizmos.matrix = transform.localToWorldMatrix;

        if (col.direction == CapsuleDirection2D.Vertical)
        {
            // 가운데 직사각형
            Gizmos.DrawWireCube(col.offset, new Vector3(col.size.x, col.size.y - col.size.x, 0));

            // 위쪽 원
            Gizmos.DrawWireSphere(col.offset + new Vector2(0, (col.size.y - col.size.x) / 2), col.size.x / 2);

            // 아래쪽 원
            Gizmos.DrawWireSphere(col.offset - new Vector2(0, (col.size.y - col.size.x) / 2), col.size.x / 2);
        }
        else
        {
            // Horizontal 방향
            Gizmos.DrawWireCube(col.offset, new Vector3(col.size.x - col.size.y, col.size.y, 0));
            Gizmos.DrawWireSphere(col.offset + new Vector2((col.size.x - col.size.y) / 2, 0), col.size.y / 2);
            Gizmos.DrawWireSphere(col.offset - new Vector2((col.size.x - col.size.y) / 2, 0), col.size.y / 2);
        }
    }
}
