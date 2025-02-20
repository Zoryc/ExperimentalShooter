using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ThrowableScr;

public class WeaponManScr : MonoBehaviour
{
    public static WeaponManScr Instance { get; set; }

    public List<GameObject> weaponSlots;

    internal GameObject activeWeaponSlot;

    [Header("Ammo")]
    public int totalRifleAmmo = 0;
    public int totalPistolAmmo = 0;

    [Header("Throwables")]
    public GameObject grenadePrefab;

    [Header("Lethal")]
    public GameObject throwableSpawn;
    public float throwForce = 10f;
    public float forceMultiplier = 0;
    public float forceMulLimit = 2f;
    public int lethalCount = 0;
    public ThrowableScr.ThrowableType equippedLethalType;

    [Header("Tacticals")]
    public int tacticalCount = 0;
    public ThrowableScr.ThrowableType equippedTacticalType;
    public GameObject smokeGrenadePrefab;

    private void Awake() // Called when loaded!
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        activeWeaponSlot = weaponSlots[0]; // start with the first one

        equippedLethalType = ThrowableScr.ThrowableType.None;
        equippedTacticalType = ThrowableScr.ThrowableType.None;
    }

    private void Update()
    {
        foreach (GameObject obj in weaponSlots) {
            if (obj == activeWeaponSlot) {
                obj.SetActive(true);
            } else {
                obj.SetActive(false);
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchActiveSlot(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchActiveSlot(1);
        }

        if (Input.GetKey(KeyCode.G) || Input.GetKey(KeyCode.T)) {
            forceMultiplier += Time.deltaTime;

            if (forceMultiplier > forceMulLimit) {
                forceMultiplier = forceMulLimit;
            }
        }

        if (Input.GetKeyUp(KeyCode.G)) {
            if (lethalCount > 0) {
                ThrowLethal();
            }
            forceMultiplier = 0;
        }

        if (Input.GetKeyUp(KeyCode.T))
        {
            if (tacticalCount > 0)
            {
                ThrowTactical();
            }
            forceMultiplier = 0;
        }
    }

    private void ThrowTactical() // why two same method?
    {
        GameObject tacticalPreFab = getThrowablePreFab(equippedTacticalType);

        GameObject throwable = Instantiate(tacticalPreFab, throwableSpawn.transform.position, Camera.main.transform.rotation);
        Rigidbody rb = throwable.GetComponent<Rigidbody>();

        rb.AddForce(Camera.main.transform.forward * (throwForce * forceMultiplier), ForceMode.Impulse);

        throwable.GetComponent<ThrowableScr>().hasBeenThrown = true;
        tacticalCount -= 1;

        if (tacticalCount == 0)
        {
            equippedTacticalType = ThrowableScr.ThrowableType.None;
        }

        HUBManScr.Instance.updateThrowableUI();
    }

    private void ThrowLethal()
    {
        GameObject lethalPreFab = getThrowablePreFab(equippedLethalType);

        GameObject throwable = Instantiate(lethalPreFab, throwableSpawn.transform.position, Camera.main.transform.rotation);
        Rigidbody rb = throwable.GetComponent<Rigidbody>();

        rb.AddForce(Camera.main.transform.forward * (throwForce * forceMultiplier), ForceMode.Impulse);

        throwable.GetComponent<ThrowableScr>().hasBeenThrown = true;
        lethalCount -= 1;

        if (lethalCount == 0) {
            equippedLethalType = ThrowableScr.ThrowableType.None;
        }

        HUBManScr.Instance.updateThrowableUI();
    }

    private GameObject getThrowablePreFab(ThrowableType type)
    {
        switch (type) {
            case ThrowableScr.ThrowableType.Grenade:
                return grenadePrefab;
            case ThrowableType.Smoke_Grenade:
                return smokeGrenadePrefab;
        }

        return null;
    }

    public void PickupWeapon(GameObject pWeapon) {
        AddWeaponIntoActiveSlot(pWeapon);
    }

    private void AddWeaponIntoActiveSlot(GameObject pWeapon)
    {
        DropCurrentWeapon(pWeapon);

        pWeapon.transform.SetParent(activeWeaponSlot.transform, false);

        WeaponScr weapon = pWeapon.GetComponent<WeaponScr>();

        pWeapon.transform.localPosition = new Vector3(weapon.spawnPosition.x, weapon.spawnPosition.y, weapon.spawnPosition.z);
        pWeapon.transform.localRotation = Quaternion.Euler(weapon.spawnRotation.x, weapon.spawnRotation.y, weapon.spawnRotation.z);

        weapon.animator.enabled = true;
        weapon.isActiveWeapon = true; // enable the weapon in the script
    }

    private void DropCurrentWeapon(GameObject pWeapon)
    {
        if (activeWeaponSlot.transform.childCount > 0) {
            var weaponToDrop = activeWeaponSlot.transform.GetChild(0).gameObject;

            weaponToDrop.GetComponent<WeaponScr>().isActiveWeapon = false;
            weaponToDrop.GetComponent<WeaponScr>().animator.enabled = false;

            weaponToDrop.transform.SetParent(pWeapon.transform.parent);
            weaponToDrop.transform.localPosition = pWeapon.transform.localPosition;
            weaponToDrop.transform.localRotation = pWeapon.transform.localRotation;
        }
    }

    internal void PuckupAmmoBox(GameObject obj)
    {

        AmmoBoxScr ammoBox = obj.GetComponent<AmmoBoxScr>();
        switch (ammoBox.ammoType) {
            case AmmoBoxScr.AmmoType.PistolAmmo:
                totalPistolAmmo += ammoBox.ammoAmount;
                break;
            case AmmoBoxScr.AmmoType.RifleAmmo:
                totalRifleAmmo += ammoBox.ammoAmount;
                break;
        }
    }

    public void SwitchActiveSlot(int slotNumber)
    {
        if (activeWeaponSlot.transform.childCount > 0) {
            WeaponScr currentWeapon = activeWeaponSlot.transform.GetChild(0).GetComponent<WeaponScr>();
            currentWeapon.isActiveWeapon = false;
            // Deactivate the current weapon
        }

        activeWeaponSlot = weaponSlots[slotNumber];

        if (activeWeaponSlot.transform.childCount > 0) {
            WeaponScr newWeapon = activeWeaponSlot.transform.GetChild(0).GetComponent<WeaponScr>();
            newWeapon.isActiveWeapon = true;
            // Activate the new weapon
        }
    }

    public void DecreaseTotalAmmo(int magazineSize, WeaponScr.WeaponModel weaponModel)
    {
        switch (weaponModel) {
            case WeaponScr.WeaponModel.Pistol1911:
                totalPistolAmmo -= magazineSize;
                break;
            case WeaponScr.WeaponModel.AK47:
                totalRifleAmmo -= magazineSize;
                break;
        }
    }

    public int checkAmmoLeft(WeaponScr.WeaponModel weaponModel)
    {
        switch (weaponModel)
        {
            case WeaponScr.WeaponModel.Pistol1911:
                return totalPistolAmmo;
            case WeaponScr.WeaponModel.AK47:
                return totalRifleAmmo;
        }

        return 0;
    }

    #region | ---- Throwables ---- |
    public void PickupThrowable(ThrowableScr throwable)
    {
        switch (throwable.throwableType) {
            case ThrowableScr.ThrowableType.Grenade:
                pickupThrowableAsLethal(throwable.throwableType);
                break;
            case ThrowableScr.ThrowableType.Smoke_Grenade:
                PickupThrowableAsTactical(throwable.throwableType);
                break;
        }
    }

    private void PickupThrowableAsTactical(ThrowableScr.ThrowableType tactical)
    {
        print("Picked up tactical");

        if (equippedTacticalType == tactical || equippedTacticalType == ThrowableScr.ThrowableType.None)
        {
            equippedTacticalType = tactical;

            if (tacticalCount < 2) // 2 max
            {
                tacticalCount += 1;
                Destroy(InteractionManScr.Instance.hoveringThrowable.gameObject); // Really?
                HUBManScr.Instance.updateThrowableUI();
            }
            else
            {
                print("tactical is max");

            }
        }
    }

    private void pickupThrowableAsLethal(ThrowableScr.ThrowableType throwableType)
    {
        print("Picked up Throwable");

        if (equippedLethalType == throwableType || equippedLethalType == ThrowableScr.ThrowableType.None) {

            equippedLethalType = throwableType;

            if (lethalCount < 2) // 2 max
            {
                lethalCount += 1;
                Destroy(InteractionManScr.Instance.hoveringThrowable.gameObject); // Really?
                HUBManScr.Instance.updateThrowableUI();
            } else {
                print("Lethal is max");
            
            }
        }
    }
    #endregion
}
