using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Portal : MonoBehaviour
{
    [SerializeField] private TowerManager towerManager;
    [SerializeField] private TowerFloor destinationFloor;

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
}
