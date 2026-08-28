using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Portal : MonoBehaviour
{
    [SerializeField] private TowerManager towerManager;
    [SerializeField] private TowerFloor destinationFloor;

    [Header("Fallback Resolution")]
    [Tooltip("destinationFloor가 프리팹 참조이거나 비어있을 때, 이 층 번호로 씬에서 자동으로 찾습니다.")]
    [SerializeField] private int destinationFloorNumber = -1;

    [Min(0f)]
    [SerializeField] private float reentryCooldown = 0.25f;

    private bool isOnCooldown;

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;

        if (towerManager == null)
            towerManager = FindActiveTowerManagerInScenes();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isOnCooldown || !other.CompareTag("Player"))
            return;

        // Ensure we have a valid, active TowerManager instance to call.
        if (towerManager == null || !towerManager.gameObject.activeInHierarchy)
        {
            TowerManager found = FindActiveTowerManagerInScenes();
            if (found != null)
            {
                Debug.Log("Portal: replacing inactive TowerManager reference with active instance '" + found.gameObject.name + "'", this);
                towerManager = found;
            }
            else if (towerManager == null)
            {
                Debug.Log("Portal: towerManager reference is null and no active TowerManager found.", this);
            }
            else
            {
                Debug.LogWarning("Portal: referenced TowerManager is inactive and no alternative active instance found.", this);
            }
        }

        if (towerManager == null)
        {
            Debug.LogError("Portal: TowerManager가 지정되지 않았습니다.", this);
            return;
        }

        // destinationFloor가 프리팹 애셋을 참조 중이거나
        // 아예 비어있으면 씬에서 실제 인스턴스를 찾아 교체한다.
        if (destinationFloor == null ||
            !destinationFloor.gameObject.scene.IsValid())
        {
            TowerFloor resolved = ResolveDestinationFloor();

            if (resolved != null)
            {
                Debug.Log(
                    "Portal: destinationFloor를 씬 오브젝트로 자동 교체합니다. " +
                    "resolved=" + resolved.name,
                    this
                );

                destinationFloor = resolved;
            }
        }

        if (destinationFloor == null)
        {
            Debug.LogError("Portal: Destination Floor가 지정되지 않았습니다.", this);
            return;
        }

        isOnCooldown = true;

        towerManager.TravelToFloor(
            destinationFloor,
            other.gameObject
        );

        StartCoroutine(ResetCooldown());
    }

    private IEnumerator ResetCooldown()
    {
        yield return new WaitForSeconds(reentryCooldown);
        isOnCooldown = false;
    }

    private TowerManager FindActiveTowerManagerInScenes()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded) continue;

            var roots = scene.GetRootGameObjects();
            foreach (var root in roots)
            {
                var managers = root.GetComponentsInChildren<TowerManager>(true);
                foreach (var m in managers)
                {
                    if (m != null && m.gameObject.activeInHierarchy)
                        return m;
                }
            }
        }

        // Fallback: search all loaded objects (less preferred)
            var all = Resources.FindObjectsOfTypeAll<TowerManager>();
        foreach (var m in all)
            if (m != null && m.gameObject.activeInHierarchy)
            return m;

        return null;
    }
        /// <summary>
    /// destinationFloor가 프리팹 참조이거나 null일 때,
    /// destinationFloorNumber(또는 기존 destinationFloor의 FloorNumber)를 기준으로
    /// 씬에 실제 존재하는 TowerFloor를 찾는다.
    /// </summary>
    private TowerFloor ResolveDestinationFloor()
    {
        int targetFloorNumber = destinationFloorNumber;

        // 층 번호가 명시되지 않았다면
        // 기존 destinationFloor(프리팹일 수 있음)의 번호를 참고한다.
        if (targetFloorNumber < 0 && destinationFloor != null)
            targetFloorNumber = destinationFloor.FloorNumber;

        if (targetFloorNumber < 0)
        {
            Debug.LogWarning(
                "Portal: destinationFloorNumber가 설정되지 않아 " +
                "자동 탐색을 할 수 없습니다.",
                this
            );

            return null;
        }

        TowerFloor[] allFloors =
            Resources.FindObjectsOfTypeAll<TowerFloor>();

        foreach (TowerFloor floor in allFloors)
        {
            if (floor == null ||
                floor.gameObject == null ||
                !floor.gameObject.scene.IsValid())
            {
                continue;
            }

            if (floor.FloorNumber == targetFloorNumber)
                return floor;
        }

        Debug.LogWarning(
            "Portal: 층 번호 " + targetFloorNumber +
            "에 해당하는 씬 TowerFloor를 찾지 못했습니다.",
            this
        );

        return null;
    }
}
