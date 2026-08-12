using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour {
    public List<Item> items = new List<Item>();

    private int currentMoney = 0;

    // 장비 슬롯
    public Item equippedWeapon;
    public Item equippedHelmet;
    public Item equippedArmor;
    public Item equippedAccessory;

    public void AddItem(Item item) {
        items.Add(item);
    }

    public void AddGold(int amount)
    {
        currentMoney += amount;
    }
}