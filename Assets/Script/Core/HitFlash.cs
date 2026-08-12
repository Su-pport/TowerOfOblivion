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
        if(faceHair != null)
            faceHairColor = faceHair.color;
        if(rightEye != null)
            rightEyeColor = rightEye.color;
        if(leftEye != null)
            leftEyeColor = leftEye.color;

        renderers = root.GetComponentsInChildren<SpriteRenderer>(true);

        foreach (var sr in renderers)
        {
            sr.material = new Material(hitMaterial);
        }   
    }

    public void Flash(float duration = 0.1f)
    {
        StopAllCoroutines();
        StartCoroutine(FlashRoutine(duration));
    }

    private IEnumerator FlashRoutine(float duration)
    {
        if(faceHair != null)
            faceHair.color = Color.white;
        if(rightEye != null)
            rightEye.color = Color.white;
        if(leftEye != null)
            leftEye.color = Color.white;
        foreach (var sr in renderers)
        {
            sr.material.SetFloat("_Flash", 1f);
        }

        yield return new WaitForSeconds(duration);

        if(faceHair != null)
            faceHair.color = faceHairColor;
        if(rightEye != null)
            rightEye.color = rightEyeColor;
        if(leftEye != null)
            leftEye.color = leftEyeColor;

        foreach (var sr in renderers)
        {
            sr.material.SetFloat("_Flash", 0f);
        }
    }
}