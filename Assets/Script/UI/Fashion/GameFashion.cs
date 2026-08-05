using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
using UnityEditor.Rendering;

public class GameFashion : MonoBehaviour
{
    public enum FashionType
    {
        Hair,
        Cloth,
        Pant,
        Weapon
    }

    public enum ColorType
    {
        Eye,
        Hair
    }
    
    private const string HairPath = "SPUM/SPUM_Sprites/Items/0_Hair";
    private const string ClothPath = "SPUM/SPUM_Sprites/Items/2_Cloth";
    private const string PantPath = "SPUM/SPUM_Sprites/Items/3_Pant";
    private const string WeaponPath = "SPUM/SPUM_Sprites/Items/6_Weapons";

    private const string BodyName = "Body";
    private const string LeftName = "Left";
    private const string RightName = "Right";
    
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
        foreach (Image button in colorButtonImage) 
        {
            if(button == colorButtonImage[2])
                button.color = skinColors[0];
            else
                button.color = basicColor;}
        SetColor(spriteObject.bodyList, skinColors[0]);
        SetColor(spriteObject.eyeList, basicColor);
        SetColor(spriteObject.hairList, basicColor);
        spriteObject.Reset();
    }



    public FashionType nowFashion;
    public void OpenFashion(Sprite[] sprites)
    {
        OpenPanel();

        CreateEmptySlot(RemoveFashion);

        foreach(Sprite sprite in sprites)
        {
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

                sprites = Resources.LoadAll<Sprite>(ClothPath + "/" + texture.name);

                foreach (Sprite sprite in sprites)
                {
                    if(sprite.name == BodyName) body.sprite = sprite;
                    else if(sprite.name == LeftName) left.sprite = sprite;
                    else if(sprite.name == RightName) right.sprite = sprite;
                }
                break;

                case FashionType.Pant:
                item.transform.Find("Pant").gameObject.SetActive(true);

                left = item.transform.Find("Pant/Left").GetComponent<Image>();
                right = item.transform.Find("Pant/Right").GetComponent<Image>();

                sprites = Resources.LoadAll<Sprite>(PantPath + "/" + texture.name);

                foreach (Sprite sprite in sprites)
                {
                    if(sprite.name == LeftName) left.sprite = sprite;
                    else if(sprite.name == RightName) right.sprite = sprite;
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

        OpenFashion(Resources.LoadAll<Sprite>(HairPath));
    }

    public void OpenCloth()
    {
        nowFashion = FashionType.Cloth;

        OpenFashion(Resources.LoadAll<Texture2D>(ClothPath));
    }

    public void OpenPant()
    {
        nowFashion = FashionType.Pant;

        OpenFashion(Resources.LoadAll<Texture2D>(PantPath));
    }

    public void OpenWeapon()
    {
        nowFashion = FashionType.Weapon;

        OpenFashion(GetAvailableWeapons().ToArray());
    }

    public void RemoveFashion()
    {
        switch(nowFashion)
        {
            case FashionType.Hair:
            ClearSprites(spriteObject.hairList);
            break;

            case FashionType.Cloth:
            ClearSprites(spriteObject.clothList);
            break;

            case FashionType.Pant:
            ClearSprites(spriteObject.pantList);
            break;

            case FashionType.Weapon:
            ClearSprites(spriteObject.weaponList);
            break;
        }

        CloseSpritePanel();
    }

    private void ClearSprites(List<SpriteRenderer> renderers)
    {
        foreach (var renderer in renderers)
        {
            if (renderer != null) renderer.sprite = null;
        }
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
                    case BodyName:
                        spriteObject.clothList[0].sprite = s;
                        break;
                    case LeftName:
                        spriteObject.clothList[1].sprite = s;
                        break;
                    case RightName:
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
                    case LeftName:
                        spriteObject.pantList[0].sprite = s;
                        break;
                    case RightName:
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

    public ColorType nowColorType;
    public List<Image> colorButtonImage = new List<Image>();
    public Color basicColor;
    public Color nowColor;
    public List<Color> skinColors = new List<Color>();

    public void OpenColorPick(int num)
    {
        colorPicker.SetActive(true);
        nowColorType = (ColorType)num;
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
        switch (nowColorType)
        {
            case ColorType.Eye: //eye
            colorButtonImage[0].color = nowColor;
            SetColor(spriteObject.eyeList, nowColor);
            break;

            case ColorType.Hair: //hair
            colorButtonImage[1].color = nowColor;
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

    private int currentSkinIndex = 0;
    public void ChangeSkinColor()
    {
        currentSkinIndex++;

        if(currentSkinIndex >= skinColors.Count) currentSkinIndex = 0;
        
        Color skinColor = skinColors[currentSkinIndex];
        SetColor(spriteObject.bodyList, skinColor);

        colorButtonImage[2].color = skinColor;
    }

    public void CloseSpritePanel()
    {
        spritePanel.SetActive(false);
    }

    public Texture2D colorPalette;
    public Color GetRandomPaletteColor()
    {
        Color color = Color.clear;

        while(color.a == 0)
        {
            int x = Random.Range(0, colorPalette.width);
            int y = Random.Range(0, colorPalette.height);

            color = colorPalette.GetPixel(x, y);
        }

        return color;
    }

    private int GetRandomIndex<T>(T[] array)
    {
        if(array == null || array.Length == 0) return -1;

        return Random.Range(0, array.Length);
    }

    public void RandomHair()
    {
        //헤어 랜덤
        nowFashion = FashionType.Hair;

        Sprite[] hairs = Resources.LoadAll<Sprite>(HairPath);
        // Debug.Log("Hair Count : " + hairs.Length);

        int randomIndex = GetRandomIndex(hairs);
        if(randomIndex < 0) return;
        ChangeFashion(sprite: hairs[randomIndex]);

        //헤어 색상 랜덤
        nowColorType = ColorType.Hair;
        Color randomColor = GetRandomPaletteColor();
        SetColor(spriteObject.hairList, randomColor);
        colorButtonImage[1].color = randomColor;
    }

    public void RandomCloth()
    {
        nowFashion = FashionType.Cloth;
        
        Texture2D[] cloths = Resources.LoadAll<Texture2D>(ClothPath);

        int randomIndex = GetRandomIndex(cloths);
        if(randomIndex < 0) return;
        Sprite[] sprites = Resources.LoadAll<Sprite>(ClothPath + "/" + cloths[randomIndex].name);

        ChangeFashion(sprites: sprites);
    }

    public void RandomPant()
    {
        nowFashion = FashionType.Pant;
        
        Texture2D[] pants = Resources.LoadAll<Texture2D>(PantPath);

        int randomIndex = GetRandomIndex(pants);
        if(randomIndex < 0) return;
        Sprite[] sprites = Resources.LoadAll<Sprite>(PantPath + "/" + pants[randomIndex].name);

        ChangeFashion(sprites: sprites);
    }

    public void RandomWeapon()
    {
        nowFashion = FashionType.Weapon;

        List<Sprite> weapons = GetAvailableWeapons();

        int randomIndex = GetRandomIndex(weapons.ToArray());
        if(randomIndex < 0) return;

        ChangeFashion(sprite : weapons[randomIndex]);
    }

    public void RandomEye()
    {
        nowColorType = ColorType.Eye;
        Color randomColor = GetRandomPaletteColor();
        SetColor(spriteObject.eyeList, randomColor);
        colorButtonImage[0].color = randomColor;
    }

    public void RandomSkin()
    {
        int randomIndex = Random.Range(0, skinColors.Count);
        currentSkinIndex = randomIndex;
        Color skinColor = skinColors[randomIndex];

        SetColor(spriteObject.bodyList, skinColor);

        colorButtonImage[2].color = skinColor;
    }

    public void AllRandom()
    {
        RandomSkin();
        RandomEye();
        RandomHair();
        RandomCloth();
        RandomPant();
        RandomWeapon();
    }

    private List<Sprite> GetAvailableWeapons()
    {
        Sprite[] weapons = Resources.LoadAll<Sprite>(WeaponPath);

        List<Sprite> result = new();

        foreach (Sprite weapon in weapons)
        {
            if(weapon.name.Contains("Bow") ||
                weapon.name.Contains("Sword") ||
                weapon.name.Contains("Ward")) result.Add(weapon);
        }

        return result;
    }
}
