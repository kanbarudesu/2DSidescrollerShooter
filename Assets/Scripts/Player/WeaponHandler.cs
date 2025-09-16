using UnityEngine;
using UnityEngine.EventSystems;

public class WeaponHandler : MonoBehaviour
{
    [Header("Initial Weapon")]
    [SerializeField] private GameObject initialWeaponPrefab;
    [SerializeField] private Transform weaponHolder;

    private WeaponBase currentWeapon;
    private GameObject currentWeaponObj;

    private void Start()
    {
        if (initialWeaponPrefab != null)
        {
            EquipWeapon(initialWeaponPrefab);
        }
    }

    private void Update()
    {
        if (currentWeapon == null) return;

        bool isHoveringUI = EventSystem.current.IsPointerOverGameObject();

        if (Input.GetButtonDown("Fire1") && !isHoveringUI)
        {
            if (currentWeapon.CanFire())
            {
                currentWeapon.Fire(transform);
            }
        }

        if (Input.GetButtonUp("Fire1") && !isHoveringUI)
        {
            currentWeapon.StopFire();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            currentWeapon.Reload();
        }
    }

    public void EquipWeapon(GameObject newWeaponPrefab)
    {
        if (newWeaponPrefab == null) return;

        if (currentWeaponObj != null)
            Destroy(currentWeaponObj);

        currentWeaponObj = Instantiate(newWeaponPrefab, weaponHolder.position, weaponHolder.rotation, weaponHolder);

        currentWeapon = currentWeaponObj.GetComponent<WeaponBase>();
        if (currentWeapon == null)
        {
            Debug.LogError("Equipped weapon does not inherit WeaponBase!");
        }
    }

    public WeaponBase GetCurrentWeapon()
    {
        return currentWeapon;
    }
}
