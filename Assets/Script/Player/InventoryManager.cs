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
    public int currentItemCount = 0;


    private void Start()
    {
        // InitInventory();
        inventoryAreaUI.CreateButtons(maxInventorySize);
    }

    //임시코드
    public Item sword1;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {

            AddItem(sword1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            AddMaxInventorySize(10);
        }

    }

    // 여 기 까 지 임 시 코 드


    // private void InitInventory()
    // {
    //     for ( int i = 0; i< maxInventorySize; i++)
    //     {
    //         items.Add(null);
    //     }
    // }

    public void AddMaxInventorySize(int size)
    {
        maxInventorySize += size;
        inventoryAreaUI.AddButtons(size);
    }


    public void AddItem(Item item) {
        if( currentItemCount < maxInventorySize) {
            items.Add(item);
            currentItemCount++;

            inventoryAreaUI.RefreshInventory();
        }
    }

    public void AddGold(int amount)
    {
        currentMoney += amount;
    }
}