using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Ammo Settings")]
    [SerializeField] protected int initialMaxAmmo = 20;
    [SerializeField] protected int ammoPerClip = 10;
    [SerializeField] protected float reloadTime = 1.5f;
    [SerializeField] protected bool infiniteAmmo = false;
    [SerializeField] protected bool autoReload = false;

    [Header("Cooldown")]
    [SerializeField] protected float fireRate = 0.2f;

    [Header("Event")]
    [SerializeField] protected UnityEvent onReload;
    [SerializeField] protected UnityEvent onOutOfAmmo;
    [SerializeField] protected UnityEvent onFire;

    protected float nextFireTime = 0f;
    public float ReloadProgress { get; private set; }
    public float CurrentAmmo { get; protected set; }
    public bool IsReloading { get; protected set; }
    public int CurrentMaxAmmo { get; protected set; }

    protected virtual void Awake()
    {
        CurrentMaxAmmo = initialMaxAmmo;
        CurrentAmmo = ammoPerClip;
    }

    public virtual bool HasAmmo => infiniteAmmo || CurrentAmmo > 0;

    protected void ConsumeAmmo(float amount)
    {
        if (!infiniteAmmo)
        {
            CurrentAmmo = Mathf.Max(0, CurrentAmmo - amount);
        }
    }

    public void AddAmmo(int amount)
    {
        CurrentMaxAmmo += amount;
    }

    public bool CanFire()
    {
        return Time.time >= nextFireTime && !IsReloading && HasAmmo;
    }

    public virtual void Fire(Transform playerTransform)
    {
        if (!CanFire()) return;

        PerformAttack(playerTransform);
        onFire.Invoke();

        CurrentAmmo--;
        nextFireTime = Time.time + fireRate;

        if (CurrentAmmo <= 0 && autoReload)
        {
            Reload();
        }
    }

    public virtual void StopFire() { }

    public virtual void Reload()
    {
        if (infiniteAmmo) return;
        if (CurrentMaxAmmo <= 0)
        {
            onOutOfAmmo.Invoke();
            return;
        }
        if (!IsReloading)
        {
            StartCoroutine(ReloadRoutine());
        }
    }

    protected IEnumerator ReloadRoutine()
    {
        IsReloading = true;
        onReload.Invoke();
        ReloadProgress = 0f;

        float elapsed = 0f;
        while (elapsed < reloadTime)
        {
            elapsed += Time.deltaTime;
            ReloadProgress = Mathf.Clamp01(elapsed / reloadTime);
            yield return null;
        }

        CurrentAmmo = CurrentMaxAmmo >= ammoPerClip ? ammoPerClip : CurrentMaxAmmo;
        CurrentMaxAmmo -= CurrentMaxAmmo >= ammoPerClip ? ammoPerClip : CurrentMaxAmmo;
        IsReloading = false;
        ReloadProgress = 1f;
    }

    protected abstract void PerformAttack(Transform playerTransform);
}
