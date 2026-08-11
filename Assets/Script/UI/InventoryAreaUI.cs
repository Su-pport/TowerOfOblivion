using NUnit.Framework;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;


public class InventoryAreaUI : MonoBehaviour
{
    public GameObject buttonPrefab;   // 버튼 프리팹
    public Transform contentParent;   // Content 오브젝트
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
            Debug.Log("*******************");

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
}
    