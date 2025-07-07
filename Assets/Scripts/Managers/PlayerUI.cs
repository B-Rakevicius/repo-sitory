using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private GameObject _itemPickupTextMesh;
    [SerializeField] private GameObject _inventoryItemImage;
    
    
    private void Start()
    {
        _itemPickupTextMesh.SetActive(false);
        _inventoryItemImage.GetComponent<Image>().enabled = false;
        
        GameManager.Instance.OnItemPickupTextShow += GameManager_OnItemPickupTextShow;
        GameManager.Instance.OnItemPickupTextHide += GameManager_OnItemPickupTextHide;
        GameManager.Instance.OnItemEquipped += GameManager_OnItemEquipped;
        GameManager.Instance.OnClearInventorySlotImage += GameManager_OnClearInventorySlotImage;
    }

    private void GameManager_OnClearInventorySlotImage(object sender, EventArgs e)
    {
        _inventoryItemImage.GetComponent<Image>().enabled = false;
    }

    private void GameManager_OnItemEquipped(object sender, GameManager.OnItemEquippedEventArgs e)
    {
        _inventoryItemImage.GetComponent<Image>().enabled = true;
        _inventoryItemImage.GetComponent<Image>().sprite = e.itemIcon;
    }

    private void GameManager_OnItemPickupTextHide(object sender, EventArgs e)
    {
        HideItemPickupText();
    }

    private void GameManager_OnItemPickupTextShow(object sender, EventArgs e)
    {
        ShowItemPickupText();
    }

    private void ShowItemPickupText()
    {
        _itemPickupTextMesh.SetActive(true);
    }

    private void HideItemPickupText()
    {
        _itemPickupTextMesh.SetActive(false);
    }
}
