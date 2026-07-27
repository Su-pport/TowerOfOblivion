using UnityEngine;
using System.Collections;

public class HitFlash : MonoBehaviour
{
    private SpriteRenderer[] renderers;

    [SerializeField] private Material hitMaterial;

    void Awake()
    {
        renderers = GetComponentsInChildren<SpriteRenderer>(true);

        foreach (var sr in renderers)
        {
            sr.material = new Material(hitMaterial);
        }
    }

    public void Flash(float duration = 1f)
    {
        StopAllCoroutines();
        StartCoroutine(FlashRoutine(duration));
    }

    private IEnumerator FlashRoutine(float duration)
    {
        foreach (var sr in renderers)
        {
            sr.material.SetFloat("_Flash", 1f);
        }

        yield return new WaitForSeconds(duration);

        foreach (var sr in renderers)
        {
            sr.material.SetFloat("_Flash", 0f);
        }
    }
}