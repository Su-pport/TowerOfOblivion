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

    public List<string> _hairListString = new List<string>();
    public List<string> _clothListString = new List<string>();
    public List<string> _pantListString = new List<string>();
    public List<string> _weaponListString = new List<string>();
}
