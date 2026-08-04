using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

public class GameFashion : MonoBehaviour
{
    public enum FashionType
    {
        Hair,
        Cloth,
        Pant,
        Weapon
    }
    
    public SpriteList spriteObject;
    [Header("UI")]
    public GameObject spritePanel;
    public Transform content;
    public GameObject itemPrefab;
    public GameObject colorPicker;

    void Start()
    {
        SetReset();
    }

    public void SetReset()
    {
        foreach (Image button in colorButton) button.color = basicColor;
        SetColor(spriteObject.eyeList, basicColor);
        SetColor(spriteObject.hairList, basicColor);
        spriteObject.Reset();
    }



    public FashionType nowFashion;
    public List<Image> fashionButton = new List<Image>();
    public void OpenFashion(Sprite[] sprites)
    {
        OpenPanel();

        CreateEmptySlot(RemoveFashion);

        foreach(Sprite sprite in sprites)
        {
            if(nowFashion == FashionType.Weapon &&
                !sprite.name.Contains("Bow") &&
                !sprite.name.Contains("Sword") &&
                !sprite.name.Contains("Ward")) continue;
            
            GameObject item = Instantiate(itemPrefab, content);
            item.transform.Find("Basic").gameObject.SetActive(true);
            
            Image image = item.transform.Find("Basic/Image").GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;

            Sprite selectedSprite = sprite;

            Button button = item.GetComponent<Button>();

            button.onClick.AddListener(() =>
            {
                ChangeFashion(sprite: selectedSprite);
            });
        }
    }

    public void OpenFashion(Texture2D[] textures)
    {
        OpenPanel();

        CreateEmptySlot(RemoveFashion);

        foreach (Texture2D texture in textures)
        {
            GameObject item = Instantiate(itemPrefab, content);
            Sprite[] sprites = null;

            switch (nowFashion)
            {
                case FashionType.Cloth:
                item.transform.Find("Cloth").gameObject.SetActive(true);

                Image body = item.transform.Find("Cloth/Body").GetComponent<Image>();
                Image left = item.transform.Find("Cloth/Left").GetComponent<Image>();
                Image right = item.transform.Find("Cloth/Right").GetComponent<Image>();

                sprites = Resources.LoadAll<Sprite>("SPUM/SPUM_Sprites/Items/2_Cloth/" + texture.name);

                foreach (Sprite sprite in sprites)
                {
                    if(sprite.name == "Body") body.sprite = sprite;
                    else if(sprite.name == "Left") left.sprite = sprite;
                    else if(sprite.name == "Right") right.sprite = sprite;
                }
                break;

                case FashionType.Pant:
                item.transform.Find("Pant").gameObject.SetActive(true);

                left = item.transform.Find("Pant/Left").GetComponent<Image>();
                right = item.transform.Find("Pant/Right").GetComponent<Image>();

                sprites = Resources.LoadAll<Sprite>("SPUM/SPUM_Sprites/Items/3_Pant/" + texture.name);

                foreach (Sprite sprite in sprites)
                {
                    if(sprite.name == "Left") left.sprite = sprite;
                    else if(sprite.name == "Right") right.sprite = sprite;
                }
                break;


            }
            Button button = item.GetComponent<Button>();

            button.onClick.AddListener(() =>
            {
                ChangeFashion(sprites : sprites);
            });
        }
    }

    public void OpenHair()
    {
        nowFashion = FashionType.Hair;

        OpenFashion(Resources.LoadAll<Sprite>("SPUM/SPUM_Sprites/Items/0_Hair"));
    }

    public void OpenCloth()
    {
        nowFashion = FashionType.Cloth;

        OpenFashion(Resources.LoadAll<Texture2D>("SPUM/SPUM_Sprites/Items/2_Cloth"));
    }

    public void OpenPant()
    {
        nowFashion = FashionType.Pant;

        OpenFashion(Resources.LoadAll<Texture2D>("SPUM/SPUM_Sprites/Items/3_Pant/"));
    }

    public void OpenWeapon()
    {
        nowFashion = FashionType.Weapon;

        OpenFashion(Resources.LoadAll<Sprite>("SPUM/SPUM_Sprites/Items/6_Weapons/"));
    }

    public void RemoveFashion()
    {
        switch(nowFashion)
        {
            case FashionType.Hair:
            spriteObject.hairList[0].sprite = null;
            break;

            case FashionType.Cloth:
            spriteObject.clothList[0].sprite = null;
            spriteObject.clothList[1].sprite = null;
            spriteObject.clothList[2].sprite = null;
            break;

            case FashionType.Pant:
            spriteObject.pantList[0].sprite = null;
            spriteObject.pantList[1].sprite = null;
            break;

            case FashionType.Weapon:
            spriteObject.weaponList[0].sprite = null;
            break;
        }

        CloseSpritePanel();
    }

    public void ChangeFashion(Sprite[] sprites = null, Sprite sprite = null)
    {
        switch (nowFashion)
        {
            case FashionType.Hair:
            //hair
            if(sprite == null) break;
            spriteObject.hairList[0].sprite = sprite;
            break;

            case FashionType.Cloth:
            //Cloth
            if(sprites == null) break;
            foreach (Sprite s in sprites)
            {
                switch (s.name)
                {
                    case "Body":
                        spriteObject.clothList[0].sprite = s;
                        break;
                    case "Left":
                        spriteObject.clothList[1].sprite = s;
                        break;
                    case "Right":
                        spriteObject.clothList[2].sprite =s;
                        break;
                }
            }
            break;

            case FashionType.Pant:
            //Pant
            if(sprites == null) break;
            foreach (Sprite s in sprites)
            {
                switch (s.name)
                {
                    case "Left":
                        spriteObject.pantList[0].sprite = s;
                        break;
                    case "Right":
                        spriteObject.pantList[1].sprite = s;
                        break;
                }
            }
            break;

            case FashionType.Weapon:
            //Weapon
            if(sprite == null) break;
            spriteObject.weaponList[0].sprite = sprite;
            break;
        }

        CloseSpritePanel();
    }

    private void OpenPanel()
    {
        foreach (Transform child in content)
            Destroy(child.gameObject);
        
        spritePanel.SetActive(true);
    }

    private void CreateEmptySlot(UnityEngine.Events.UnityAction action)
    {
        GameObject item = Instantiate(itemPrefab, content);

        Button button = item.GetComponent<Button>();

        button.onClick.AddListener(action);
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
            SetColor(spriteObject.eyeList, nowColor);
            break;

            case 1: //hair
            colorButton[1].color = nowColor;
            SetColor(spriteObject.hairList, nowColor);
            break;
        }
        CloseColorPicker();
    }

    private void SetColor(List<SpriteRenderer> renderers, Color color)
    {
        foreach(var renderer in renderers)
        {
            if(renderer != null) renderer.color = color;
        }
    }

    public void CloseSpritePanel()
    {
        spritePanel.SetActive(false);
    }
}
