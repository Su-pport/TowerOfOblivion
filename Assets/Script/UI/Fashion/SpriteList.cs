using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpriteList : MonoBehaviour
{
    public List<SpriteRenderer> eyeList = new List<SpriteRenderer>();
    public List<SpriteRenderer> hairList = new List<SpriteRenderer>();
    public List<SpriteRenderer> bodyList = new List<SpriteRenderer>();
    public List<SpriteRenderer> clothList = new List<SpriteRenderer>();
    public List<SpriteRenderer> pantList = new List<SpriteRenderer>();
    public List<SpriteRenderer> weaponList = new List<SpriteRenderer>();

    public void Reset(){
        foreach (var list in new[] { hairList, clothList, pantList, weaponList })
        {
            foreach (var renderer in list)
            {
                if (renderer != null) renderer.sprite = null;
            }
        }
    }

    void Start()
    {
        //커스터마잊징 신에서는 데이터 적용 건너뛰기
        if (SceneManager.GetActiveScene().name == "GameFashion")
            return;

        StartCoroutine(ApplyCharacterData());
    }
    IEnumerator ApplyCharacterData()
    {
        // 씬 로드 후 잠깐 대기
        yield return new WaitForSeconds(0.1f); // Wait for a short time to ensure CharacterDataManager is initialized
        
        // 커스터마이징 데이터 불러오기
        var data = CharacterDataManager.Instance.data;
        if (data == null) yield break;

        //머리
        if (hairList.Count > 0 && hairList[0] != null)
        {
            hairList[0].sprite = data.hairSprite;
            hairList[0].color = data.hairColor;
        }

        //눈
        if (eyeList != null && eyeList.Count > 0)
        {
            foreach (var eye in eyeList)
            {
                if (eye != null)
                    eye.color = data.eyeColor;
            }
        }

        //옷
        for (int i = 0; i < clothList.Count && i < data.clothSprite.Length; i++)
            if (clothList[i] != null)
                clothList [i].sprite = data.clothSprite[i];
        
        //바지
        for (int i = 0; i < pantList.Count && i < data.pantSprite.Length; i++)
            if (pantList[i] != null)
                pantList[i].sprite = data.pantSprite[i];
        
        //무기
        if (weaponList != null && weaponList.Count > 0)
            weaponList[0].sprite = data.weaponSprite;
    }
}
