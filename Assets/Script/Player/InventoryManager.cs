using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class InventoryManager : MonoBehaviour {
    public InventoryAreaUI inventoryAreaUI;


    public List<Item> items = new List<Item>();

    private int currentMoney = 0;

    // 장비 슬롯
    public Item equippedWeapon;
    public Item equippedHelmet;
    public Item equippedArmor;
    public Item equippedAccessory;

    [SerializeField] private int maxInventorySize = 42;



    private void Start()
    {
        InitInventory();
        inventoryAreaUI.CreateButtons(maxInventorySize);
    }

    //임시코드
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            AddMaxInventorySize(10);
            Debug.Log("");
        }
    }

    private void InitInventory()
    {
        for ( int i = 0; i< maxInventorySize; i++)
        {
            AddItem(null);
        }
    }

    public void AddMaxInventorySize(int size)
    {
        maxInventorySize += size;
        inventoryAreaUI.AddButtons(size);
    }


    public void AddItem(Item item) {
        if( items.Count < maxInventorySize)
            items.Add(item);
    }

    public void AddGold(int amount)
    {
        currentMoney += amount;
    }
}