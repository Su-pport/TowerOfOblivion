using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private Vector3 playerOffset = Vector3.zero;

    public Vector3 WorldPosition => transform.position + playerOffset;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(WorldPosition, 0.25f);
        Gizmos.DrawLine(WorldPosition, WorldPosition + Vector3.up * 0.6f);
    }
}

