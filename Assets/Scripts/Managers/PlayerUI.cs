using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private GameObject _itemPickupTextMesh;
    [SerializeField] private GameObject _inventoryItemSlotMain;
    [SerializeField] private GameObject _inventoryItemSlot1;
    [SerializeField] private GameObject _inventoryItemSlot2;
    [SerializeField] private GameObject _inventoryItemSlot3;
    
    // Health UI
    [SerializeField] private TextMeshProUGUI _healthText;
    
    
    private void Start()
    {
        _itemPickupTextMesh.SetActive(false);
        _inventoryItemSlotMain.GetComponent<Image>().enabled = false;
        _inventoryItemSlot1.GetComponent<Image>().enabled = false;
        _inventoryItemSlot2.GetComponent<Image>().enabled = false;
        _inventoryItemSlot3.GetComponent<Image>().enabled = false;
        
        GameManager.Instance.OnItemPickupTextShow += GameManager_OnItemPickupTextShow;
        GameManager.Instance.OnItemPickupTextHide += GameManager_OnItemPickupTextHide;
        GameManager.Instance.OnItemEquipped += GameManager_OnItemEquipped;
        GameManager.Instance.OnItemPickedUp += GameManager_OnItemPickedUp;
        GameManager.Instance.OnClearInventorySlotImage += GameManager_OnClearInventorySlotImage;
        GameManager.Instance.OnHealthChanged += GameManager_OnHealthChanged;
        
        // TODO: Change health number to max health from player stats.
    }

    private void GameManager_OnHealthChanged(object sender, GameManager.OnHealthChangedEventArgs e)
    {
        _healthText.text = e.newHealth.ToString();
    }

    private void GameManager_OnItemPickedUp(object sender, GameManager.OnItemPickedUpEventArgs e)
    {
        switch (e.pos)
        {
            case 0:
                _inventoryItemSlot1.GetComponent<Image>().enabled = true;
                _inventoryItemSlot1.GetComponent<Image>().sprite = e.itemIcon;
                break;
            case 1:
                _inventoryItemSlot2.GetComponent<Image>().enabled = true;
                _inventoryItemSlot2.GetComponent<Image>().sprite = e.itemIcon;
                break;
            case 2:
                _inventoryItemSlot3.GetComponent<Image>().enabled = true;
                _inventoryItemSlot3.GetComponent<Image>().sprite = e.itemIcon;
                break;
        }
    }

    private void GameManager_OnClearInventorySlotImage(object sender, GameManager.OnClearInventorySlotImageEventArgs e)
    {
        switch (e.pos)
        {
            case 0:
                _inventoryItemSlot1.GetComponent<Image>().enabled = false;
                break;
            case 1:
                _inventoryItemSlot2.GetComponent<Image>().enabled = false;
                break;
            case 2:
                _inventoryItemSlot3.GetComponent<Image>().enabled = false;
                break;
        }
        _inventoryItemSlotMain.GetComponent<Image>().enabled = false;
    }

    private void GameManager_OnItemEquipped(object sender, GameManager.OnItemEquippedEventArgs e)
    {
        _inventoryItemSlotMain.GetComponent<Image>().enabled = true;
        _inventoryItemSlotMain.GetComponent<Image>().sprite = e.itemIcon;
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
