using UnityEngine;
using Unity.Cinemachine;

public class CameraView : MonoBehaviour
{
    private CinemachineCamera cinemachineCamera;
    private Transform player;

    private void Awake()
    {
        cinemachineCamera = GetComponent<CinemachineCamera>();
    }

    private void Start()
    {
        FindPlayer();
    }

    private void FindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
        {
            Debug.LogWarning("Player 태그를 가진 오브젝트를 찾을 수 없습니다.");
            return;
        }

        player = playerObject.transform;
        cinemachineCamera.Follow = player;
    }
}