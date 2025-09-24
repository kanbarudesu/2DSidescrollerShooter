using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.AsyncOperations;

public class WeaponHandler : MonoBehaviour
{
    [Header("Initial Weapon")]
    [SerializeField] private AssetReferenceGameObject initialWeaponPrefab;
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

    public void EquipWeapon(AssetReferenceGameObject newWeaponPrefab)
    {
        if (newWeaponPrefab == null) return;
        StartCoroutine(EquipWeaponRoutine(newWeaponPrefab));
    }

    private IEnumerator EquipWeaponRoutine(AssetReferenceGameObject newWeaponPrefab)
    {
        AsyncOperationHandle<GameObject> handle = newWeaponPrefab.InstantiateAsync(weaponHolder.position, Quaternion.identity, weaponHolder);
        yield return handle;

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"Failed to load weapon: {newWeaponPrefab.RuntimeKey}");
            yield break;
        }

        if (currentWeaponObj != null)
        {
            Addressables.ReleaseInstance(currentWeaponObj);
            Destroy(currentWeaponObj);
        }

        currentWeaponObj = handle.Result;
        if (!currentWeaponObj.TryGetComponent(out currentWeapon))
        {
            Debug.LogError("Equipped weapon does not inherit WeaponBase!");
        }
    }

    public WeaponBase GetCurrentWeapon()
    {
        return currentWeapon;
    }
}
