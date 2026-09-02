using System.Collections.Generic;
using UnityEngine;
using static Throwable;

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
    public Throwable.ThrowableType equippedLethalType;

    [Header("Tacticals")]
    public int tacticalCount = 0;
    public Throwable.ThrowableType equippedTacticalType;
    public GameObject smokeGrenadePrefab;

    private void Awake()
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
        activeWeaponSlot = weaponSlots[0]; // start with the first one //

        equippedLethalType = Throwable.ThrowableType.None;
        equippedTacticalType = Throwable.ThrowableType.None;
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

        throwable.GetComponent<Throwable>().hasBeenThrown = true;
        tacticalCount -= 1;

        if (tacticalCount == 0)
        {
            equippedTacticalType = Throwable.ThrowableType.None;
        }

        HUBManScr.Instance.updateThrowableUI();
    }

    private void ThrowLethal()
    {
        GameObject lethalPreFab = getThrowablePreFab(equippedLethalType);

        GameObject throwable = Instantiate(lethalPreFab, throwableSpawn.transform.position, Camera.main.transform.rotation);
        Rigidbody rb = throwable.GetComponent<Rigidbody>();

        rb.AddForce(Camera.main.transform.forward * (throwForce * forceMultiplier), ForceMode.Impulse);

        throwable.GetComponent<Throwable>().hasBeenThrown = true;
        lethalCount -= 1;

        if (lethalCount == 0) {
            equippedLethalType = Throwable.ThrowableType.None;
        }

        HUBManScr.Instance.updateThrowableUI();
    }

    private GameObject getThrowablePreFab(ThrowableType type)
    {
        switch (type) {
            case Throwable.ThrowableType.Grenade:
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
        // Disable the rigidbody forces
        pWeapon.GetComponent<Rigidbody>().isKinematic = true;

        DropCurrentWeapon(pWeapon);

        pWeapon.transform.SetParent(activeWeaponSlot.transform, false);

        WeaponScr weapon = pWeapon.GetComponent<WeaponScr>();

        pWeapon.transform.localPosition = new Vector3(weapon.spawnPosition.x, weapon.spawnPosition.y, weapon.spawnPosition.z);
        pWeapon.transform.localRotation = Quaternion.Euler(weapon.spawnRotation.x, weapon.spawnRotation.y, weapon.spawnRotation.z);

        weapon.animator.enabled = true;
        weapon.isActiveWeapon = true;
    }

    private void DropCurrentWeapon(GameObject pWeapon)
    {
        if (activeWeaponSlot.transform.childCount > 0) {
            GameObject weaponToDrop = activeWeaponSlot.transform.GetChild(0).gameObject;

            // Enable the rigidbody forces
            weaponToDrop.GetComponent<Rigidbody>().isKinematic = false;

            weaponToDrop.GetComponent<WeaponScr>().isActiveWeapon = false;
            weaponToDrop.GetComponent<WeaponScr>().animator.enabled = false;

            weaponToDrop.transform.SetParent(pWeapon.transform.parent);
            weaponToDrop.transform.localPosition = pWeapon.transform.localPosition;
            weaponToDrop.transform.localRotation = pWeapon.transform.localRotation;
        }
    }

    public void PuckupAmmoBox(GameObject obj)
    {
        AmmoBox ammoBox = obj.GetComponent<AmmoBox>();
        switch (ammoBox.ammoType) {
            case AmmoBox.AmmoType.PistolAmmo:
                totalPistolAmmo += ammoBox.ammoAmount;
                break;
            case AmmoBox.AmmoType.RifleAmmo:
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
    public void PickupThrowable(Throwable throwable)
    {
        switch (throwable.throwableType) {
            case Throwable.ThrowableType.Grenade:
                pickupThrowableAsLethal(throwable.throwableType);
                break;
            case Throwable.ThrowableType.Smoke_Grenade:
                PickupThrowableAsTactical(throwable.throwableType);
                break;
        }
    }

    private void PickupThrowableAsTactical(Throwable.ThrowableType tactical)
    {
        Debug.Log("Picked up tactical");

        if (equippedTacticalType == tactical || equippedTacticalType == Throwable.ThrowableType.None)
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
                Debug.Log("tactical is max");

            }
        }
    }

    private void pickupThrowableAsLethal(Throwable.ThrowableType throwableType)
    {
        Debug.Log("Picked up Throwable");

        if (equippedLethalType == throwableType || equippedLethalType == Throwable.ThrowableType.None) {

            equippedLethalType = throwableType;

            if (lethalCount < 2) // 2 max
            {
                lethalCount += 1;
                Destroy(InteractionManScr.Instance.hoveringThrowable.gameObject); // Really?
                HUBManScr.Instance.updateThrowableUI();
            } else {
                Debug.Log("Lethal is max");
            
            }
        }
    }
    #endregion
}
