using System;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private GameObject _itemPickupTextMesh;
    

    private void Start()
    {
        _itemPickupTextMesh.SetActive(false);
        
        GameManager.Instance.OnItemPickupTextShow += GameManager_OnItemPickupTextShow;
        GameManager.Instance.OnItemPickupTextHide += GameManager_OnItemPickupTextHide;
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
