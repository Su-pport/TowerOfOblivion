using NUnit.Framework;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.UI;


public class InventoryAreaUI : MonoBehaviour
{
    public InventoryManager inventoryManager;   // 인벤토리 매니저
    public InventoryDescAreaUI inventoryDescAreaUI; // 인벤토리 UI내 선택한 아이템 설명패널

    public GameObject buttonPrefab;   // 버튼 프리팹
    public Transform contentParent;   // Content 오브젝트

    private List<GameObject> buttons = new List<GameObject>();

    [SerializeField] private int maxVisibleButtons = 42;
    public int currentButtonCount = 0;


    public void CreateButtons(int count)
    {
        currentButtonCount = count;
        // 기존 버튼 삭제
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // 새로운 버튼 생성
        for (int i = 0; i < count; i++)
        {
            GameObject newButton = Instantiate(buttonPrefab, contentParent);
            newButton.GetComponentInChildren<TextMeshProUGUI>().text = (i + 1).ToString();
            buttons.Add(newButton);
        }
        ScrollLock();
    }

    public void AddButtons(int count)
    {
        currentButtonCount += count;
        // 새로운 버튼 생성
        for (int i = 0; i < count; i++)
        {
            GameObject newButton = Instantiate(buttonPrefab, contentParent);
            newButton.GetComponentInChildren<TextMeshProUGUI>().text = (contentParent.childCount).ToString();
        }
        ScrollLock();
    }

    public void ScrollLock()
    {
        if (currentButtonCount <= maxVisibleButtons)
        {

            // 스크롤 잠금 로직 구현
            var scrollRect = GetComponent<UnityEngine.UI.ScrollRect>();
            if (scrollRect != null)
            {
                scrollRect.enabled = false;
            }

        }
        else
        {
            // 스크롤 잠금 해제 로직 구현
            var scrollRect = GetComponent<UnityEngine.UI.ScrollRect>();
            if (scrollRect != null)
            {
                scrollRect.enabled = true;
            }
        }
    }

    public void RefreshInventory()
    {
        // 아이템이 있는 공간 해당 아이콘으로 변경
        for(int i = 0; i< inventoryManager.items.Count; i++)
        {
            buttons[i].GetComponent<InventorySlotUI>().SetItem(inventoryManager.items[i]);
        }

        // 아이템이 없는 빈 인벤토리 알파값 0으로 변경
        for(int i = inventoryManager.items.Count; i<maxVisibleButtons; i++)
        {
            buttons[i].GetComponent<InventorySlotUI>().Clear();
        }

    }

    public void ClickedSlot(Item item)
    {
        inventoryDescAreaUI.GetSelectedItem(item);
    }

    public void ClickedEquipButton(Item item)
    {
        EquipResult result;
        result = inventoryManager.EquipItem(item);

        switch (result)
        {
            // 장착한 아이템 보이게 하기
            default:
        }
    }
}
    