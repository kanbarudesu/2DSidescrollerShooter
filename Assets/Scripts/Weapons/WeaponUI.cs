using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WeaponHandler weaponHandler;
    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private Image reloadBar;

    private WeaponBase currentWeapon;

    private void Update()
    {
        if (weaponHandler == null) return;

        WeaponBase weapon = weaponHandler.GetCurrentWeapon();
        if (weapon == null)
        {
            ammoText.text = "- / -";
            reloadBar.fillAmount = 0f;
            return;
        }

        if (weapon != currentWeapon)
        {
            currentWeapon = weapon;
        }

        ammoText.text = $"{weapon.CurrentAmmo.ToString("0")} / {weapon.CurrentMaxAmmo}";

        if (weapon.IsReloading)
        {
            reloadBar.fillAmount = weapon.ReloadProgress;
        }
        else
        {
            reloadBar.fillAmount = 0f;
        }
    }
}