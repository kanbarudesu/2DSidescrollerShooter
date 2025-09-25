using UnityEngine;

[CreateAssetMenu(fileName = "ShopConfig", menuName = "ShopConfig", order = 0)]
public class ShopConfig : ScriptableObject
{
    public ShopItemData[] Items;
}