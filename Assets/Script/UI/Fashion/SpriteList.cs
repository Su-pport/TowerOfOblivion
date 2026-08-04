using System.Collections.Generic;
using UnityEngine;

public class SpriteList : MonoBehaviour
{
    public List<SpriteRenderer> eyeList = new List<SpriteRenderer>();
    public List<SpriteRenderer> hairList = new List<SpriteRenderer>();
    public List<SpriteRenderer> bodyList = new List<SpriteRenderer>();
    public List<SpriteRenderer> clothList = new List<SpriteRenderer>();
    public List<SpriteRenderer> pantList = new List<SpriteRenderer>();
    public List<SpriteRenderer> weaponList = new List<SpriteRenderer>();

    public void Reset(){
        for(var i = 0; i <hairList.Count; i++)
            if(hairList[i]!=null) hairList[i].sprite = null;

        for(var i = 0; i <clothList.Count; i++)
            if(clothList[i]!=null) clothList[i].sprite = null;

        for(var i = 0; i <pantList.Count; i++)
            if(pantList[i]!=null) pantList[i].sprite = null;

        for(var i = 0; i <weaponList.Count; i++)
            if(weaponList[i]!=null) weaponList[i].sprite = null; 
    }
}
