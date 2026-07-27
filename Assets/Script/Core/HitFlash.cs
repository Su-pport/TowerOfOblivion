using UnityEngine;
using System.Collections;

public class HitFlash : MonoBehaviour
{
    private SpriteRenderer[] renderers;
    
    // root의 하위오브젝트만 하얗게 하기위해
    [SerializeField] private Transform root;

    // 눈, 수염 등 color를 수정하는 오브젝트도 하얗게 하기위한 변수
    [SerializeField] private SpriteRenderer faceHair;
    [SerializeField] private SpriteRenderer rightEye;
    [SerializeField] private SpriteRenderer leftEye;

    Color faceHairColor;
    Color rightEyeColor;
    Color leftEyeColor;

    // 피격 이펙트 메터리얼
    [SerializeField] private Material hitMaterial;


    void Awake()
    {
        faceHairColor = faceHair.color;
        rightEyeColor = rightEye.color;
        leftEyeColor = leftEye.color;

        renderers = root.GetComponentsInChildren<SpriteRenderer>(true);

        foreach (var sr in renderers)
        {
            sr.material = new Material(hitMaterial);
        }   
    }

    public void Flash(float duration = 10f)
    {
        StopAllCoroutines();
        StartCoroutine(FlashRoutine(duration));
    }

    private IEnumerator FlashRoutine(float duration)
    {
        faceHair.color = Color.white;
        rightEye.color = Color.white;
        leftEye.color = Color.white;
        foreach (var sr in renderers)
        {
            sr.material.SetFloat("_Flash", 1f);
        }

        yield return new WaitForSeconds(duration);

        faceHair.color = faceHairColor;
        rightEye.color = rightEyeColor;
        leftEye.color = leftEyeColor;

        foreach (var sr in renderers)
        {
            sr.material.SetFloat("_Flash", 0f);
        }
    }
}