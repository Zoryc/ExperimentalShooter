using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManScr : MonoBehaviour
{
    public static WeaponManScr Instance { get; set; }

    public List<GameObject> weaponSlots;

    internal GameObject activeWeaponSlot;

    [Header("Ammo")]
    public int totalRifleAmmo = 0;
    public int totalPistolAmmo = 0;

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

        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            SwitchActiveSlot(0);
        } else if (Input.GetKeyDown(KeyCode.Alpha2)) {
            SwitchActiveSlot(1);
        }
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

    internal void DecreaseTotalAmmo(int magazineSize, WeaponScr.WeaponModel weaponModel)
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
}
