using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;
using System.Linq;

public class GameFashion : MonoBehaviour
{
    public SpriteList spriteObject;
    [Header("UI")]
    public GameObject spritePanel;
    public Transform content;
    public GameObject itemPrefab;
    public GameObject colorPicker;

    void Start()
    {
        SetInit();
    }

    public void SetInit()
    {
        colorButton[0].color = basicColor;
        colorButton[1].color = basicColor;
        spriteObject.eyeList[0].color = basicColor;
        spriteObject.eyeList[1].color = basicColor;
        spriteObject.hairList[0].color = basicColor;
    }

    public void OpenHair()
    {
        foreach( Transform child in content)
            Destroy(child.gameObject);

        spritePanel.SetActive(true);

        Sprite[] hairs = Resources.LoadAll<Sprite>("SPUM/SPUM_Sprites/Items/0_Hair");

        // Debug.Log("Hair Count : " + hairs.Length);

        //빈슬롯 생성
        GameObject emptyItem = Instantiate(itemPrefab,content);
        Image emptyImage = emptyItem.transform.Find("Basic/Image").GetComponent<Image>();
        emptyImage.sprite = null;
        Button emptyButton = emptyItem.GetComponent<Button>();
        emptyButton.onClick.AddListener(() =>
        {
            RemoveHair();
        });

        //진짜 생성
        foreach (Sprite hair in hairs)
        {
            GameObject item = Instantiate(itemPrefab, content);

            Image image = item.transform.Find("Basic/Image").GetComponent<Image>();

            item.transform.Find("Basic").gameObject.SetActive(true);
            image.sprite = hair;
            image.preserveAspect = true;

            Sprite selectedHair = hair;

            Button button = item.GetComponent<Button>();

            button.onClick.AddListener(() =>
            {
                ChangeHair(selectedHair);
            });
        }
    }

    public void ChangeHair(Sprite hairSprite)
    {
        spriteObject.hairList[0].sprite = hairSprite;
        CloseSpritePanel();
    }

    public void RemoveHair()
    {
        spriteObject.hairList[0].sprite = null;
        CloseSpritePanel();
    }

    public void OpenCloth()
    {
        foreach(Transform child in content)
            Destroy(child.gameObject);

        spritePanel.SetActive(true);

        Texture2D[] cloths = Resources.LoadAll<Texture2D>("SPUM/SPUM_Sprites/Items/2_Cloth");
        // Debug.Log("Cloth Count : " + cloths.Length);

        //빈슬롯 생성
        GameObject emptyItem = Instantiate(itemPrefab,content);
        Image emptyImage = emptyItem.transform.Find("Basic/Image").GetComponent<Image>();
        emptyImage.sprite = null;
        Button emptyButton = emptyItem.GetComponent<Button>();
        emptyButton.onClick.AddListener(() =>
        {
            RemoveCloth();
        });

        foreach (Texture2D cloth in cloths)
        {
            GameObject item = Instantiate(itemPrefab, content);

            item.transform.Find("Basic").gameObject.SetActive(true);

            Image image = item.transform.Find("Basic/Image").GetComponent<Image>();

            Sprite[] sprites = Resources.LoadAll<Sprite>("SPUM/SPUM_Sprites/Items/2_Cloth/" + cloth.name);

            foreach (Sprite sprite in sprites)
            {
                if(sprite.name == "Body")
                {
                    image.sprite = sprite;
                    break;
                }
            }

            string clothName = cloth.name;

            Button button = item.GetComponent<Button>();

            button.onClick.AddListener(() =>
            {
                ChangeCloth(clothName);
            });
        }
    }

    public void ChangeCloth(string clothName)
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>("SPUM/SPUM_Sprites/Items/2_Cloth/" + clothName);

        foreach (Sprite sprite in sprites)
        {
            switch (sprite.name)
            {
                case "Body":
                    spriteObject.clothList[0].sprite = sprite;
                    break;
                case "Left":
                    spriteObject.clothList[1].sprite = sprite;
                    break;
                case "Right":
                    spriteObject.clothList[2].sprite =sprite;
                    break;
            }
        }

        CloseSpritePanel();
    }

    public void RemoveCloth()
    {
        spriteObject.clothList[0].sprite = null;
        spriteObject.clothList[1].sprite = null;
        spriteObject.clothList[2].sprite = null;
        CloseSpritePanel();
    }

    public void OpenPant()
    {
        foreach(Transform child in content)
            Destroy(child.gameObject);

        spritePanel.SetActive(true);

        Texture2D[] pants = Resources.LoadAll<Texture2D>("SPUM/SPUM_Sprites/Items/3_Pant/");
        // Debug.Log("Pant Count : " + Pant.Length);

        //빈슬롯 생성
        GameObject emptyItem = Instantiate(itemPrefab,content);
        Image emptyImage = emptyItem.transform.Find("Basic/Image").GetComponent<Image>();
        emptyImage.sprite = null;
        Button emptyButton = emptyItem.GetComponent<Button>();
        emptyButton.onClick.AddListener(() =>
        {
            RemovePant();
        });

        foreach (Texture2D pant in pants)
        {
            GameObject item = Instantiate(itemPrefab, content);

            item.transform.Find("Basic").gameObject.SetActive(true);

            Image image = item.transform.Find("Basic/Image").GetComponent<Image>();

            Sprite[] sprites = Resources.LoadAll<Sprite>("SPUM/SPUM_Sprites/Items/3_Pant/" + pant.name);

            foreach (Sprite sprite in sprites)
            {
                if(sprite.name == "Left")
                {
                    image.sprite = sprite;
                    break;
                }
            }

            string pantName = pant.name;

            Button button = item.GetComponent<Button>();

            button.onClick.AddListener(() =>
            {
                ChangePant(pantName);
            });
        }
    }

    public void ChangePant(string pantName)
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>("SPUM/SPUM_Sprites/Items/3_Pant/" + pantName);

        foreach (Sprite sprite in sprites)
        {
            switch (sprite.name)
            {
                case "Left":
                    spriteObject.pantList[0].sprite = sprite;
                    break;
                case "Right":
                    spriteObject.pantList[1].sprite =sprite;
                    break;
            }
        }

        CloseSpritePanel();
    }

    public void RemovePant()
    {
        spriteObject.pantList[0].sprite = null;
        spriteObject.pantList[1].sprite = null;
        CloseSpritePanel();
    }

    public void OpenWeapon()
    {
        foreach (Transform child in content)
            Destroy(child.gameObject);
        
        spritePanel.SetActive(true);

        Sprite[] weapons = Resources.LoadAll<Sprite>("SPUM/SPUM_Sprites/Items/6_Weapons/");
        Debug.Log(" Weapon Count : " + weapons.Length);
    }

    public int nowColorNum;
    public List<Image> colorButton = new List<Image>();
    public Color basicColor;
    public Color nowColor;
    public void OpenColorPick(int num)
    {
        colorPicker.SetActive(true);
        nowColorNum = num;
    }

    public void CloseColorPicker()
    {
        colorPicker.SetActive(false);
    }

    Texture2D tex;

    public void PickColor()
    {
        tex = new Texture2D(1, 1);
        StartCoroutine(CaptureTempArea());
    }

    IEnumerator CaptureTempArea()
    {
        yield return new WaitForEndOfFrame();
        Vector2 pos = EventSystem.current.currentInputModule.input.mousePosition;
        tex.ReadPixels(new Rect(pos.x, pos.y, 1, 1), 0, 0);
        tex.Apply();
        nowColor = tex.GetPixel(0, 0);
        SetObjColor();
    }

    public void SetObjColor()
    {
        switch (nowColorNum)
        {
            case 0: //eye
            colorButton[0].color = nowColor;
            spriteObject.eyeList[0].color = nowColor;
            spriteObject.eyeList[1].color = nowColor;
            break;

            case 1: //hair
            colorButton[1].color = nowColor;
            spriteObject.hairList[0].color = nowColor;
            break;
        }
        CloseColorPicker();
    }

    public void CloseSpritePanel()
    {
        spritePanel.SetActive(false);
    }


}
