using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Button buyButton;

    public void SetItem(ShopItemData data, Action onClick)
    {
        icon.sprite = data.Icon;
        priceText.text = $"${data.Price} Coins";
        descriptionText.text = data.Description;

        buyButton.onClick.AddListener(() => onClick());
    }
}