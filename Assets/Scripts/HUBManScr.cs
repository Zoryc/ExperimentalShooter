using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class HUBManScr : MonoBehaviour
{
    // "encapsulation"
    // property - Compiler separe them
    public static HUBManScr Instance { get; set; }

    [Header("Ammo")]
    public TextMeshProUGUI magazineAmmoUI;
    public TextMeshProUGUI totalAmmoUI;
    public Image ammoTypeUI;

    [Header("Weapon")]
    public Image activeWeaponUI;
    public Image unActiveWeaponUI;

    [Header("Throwable")]
    public Image lethalUI;
    public TextMeshProUGUI lethalAmountUI;

    public Image tacticalUI;
    public TextMeshProUGUI tacticalAmountUI;

    public Sprite emptyImageSlot; // Sprite vs Image?
    public Sprite greySlot;

    public GameObject middleDot;

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

    private void Update()
    {
        WeaponScr activeWeapon = WeaponManScr.Instance.activeWeaponSlot.GetComponentInChildren<WeaponScr>(); // Cool
        WeaponScr unActiveWeapon = GetUnActiveWeaponSlot().GetComponent<WeaponScr>();

        if (activeWeapon)
        {
            magazineAmmoUI.text = $"{activeWeapon.bulletsLeft / activeWeapon.bulletsPerBurst}";
            totalAmmoUI.text = string.Empty + WeaponManScr.Instance.checkAmmoLeft(activeWeapon.weaponModel); // string.Empty

            WeaponScr.WeaponModel model = activeWeapon.weaponModel;
            ammoTypeUI.sprite = GetAmmoSprite(model);

            activeWeaponUI.sprite = GetWeaponSprite(model);

            if (unActiveWeapon)
            {
                unActiveWeaponUI.sprite = GetWeaponSprite(unActiveWeapon.weaponModel);
            }
        }
        else {
            magazineAmmoUI.text = "";
            totalAmmoUI.text = "";

            ammoTypeUI.sprite = emptyImageSlot;

            activeWeaponUI.sprite = emptyImageSlot;
            unActiveWeaponUI.sprite = emptyImageSlot;
        }

        if (WeaponManScr.Instance.lethalCount <= 0) {
            lethalUI.sprite = greySlot;
        }

        if (WeaponManScr.Instance.tacticalCount <= 0)
        {
            tacticalUI.sprite = greySlot;
        }
    }

    private Sprite GetWeaponSprite(WeaponScr.WeaponModel model)
    {
        switch (model) {
            case WeaponScr.WeaponModel.Pistol1911:
                return Resources.Load<GameObject>("1911_Sprite").GetComponent<SpriteRenderer>().sprite;
            case WeaponScr.WeaponModel.AK47:
                return Resources.Load<GameObject>("AK47_Sprite").GetComponent<SpriteRenderer>().sprite;
            default:
                return null;
        }
    }

    private Sprite GetAmmoSprite(WeaponScr.WeaponModel model)
    {
        switch (model)
        {
            case WeaponScr.WeaponModel.Pistol1911:
                return Resources.Load<GameObject>("Pistol_Bullet").GetComponent<SpriteRenderer>().sprite;
            case WeaponScr.WeaponModel.AK47:
                return Resources.Load<GameObject>("Rifle_Bullet").GetComponent<SpriteRenderer>().sprite;
            default:
                return null;
        }
    }

    private GameObject GetUnActiveWeaponSlot()
    {
        foreach (GameObject obj in WeaponManScr.Instance.weaponSlots) {
            if (obj != WeaponManScr.Instance.activeWeaponSlot) {
                return obj;
            }
        }

        return null; // prob never happen
    }

    internal void updateThrowableUI()
    {
        lethalAmountUI.text = $"{WeaponManScr.Instance.lethalCount}";
        tacticalAmountUI.text = $"{WeaponManScr.Instance.tacticalCount}";

        switch (WeaponManScr.Instance.equippedLethalType) {
            case ThrowableScr.ThrowableType.Grenade:
                lethalUI.sprite = Resources.Load<GameObject>("Grenade").GetComponent<SpriteRenderer>().sprite;
                break;
        }

        switch (WeaponManScr.Instance.equippedTacticalType) {
            case ThrowableScr.ThrowableType.Smoke_Grenade:
                tacticalUI.sprite = Resources.Load<GameObject>("Smoke_Grenade").GetComponent<SpriteRenderer>().sprite;
                break;
        }
    }
}
