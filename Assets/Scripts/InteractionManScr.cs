using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionManScr : MonoBehaviour
{
    public static InteractionManScr Instance { get; set; }

    private GameObject hoveringWeapon = null;
    private GameObject hoveringAmmoBox = null;

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
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit)) {
            GameObject hitByRayCast = hit.transform.gameObject;

            // Weapon
            if (hitByRayCast.GetComponent<WeaponScr>() != null && hitByRayCast.GetComponent<WeaponScr>().isActiveWeapon == false)
            {
                //print("Weapon selected/pointed!");
                hoveringWeapon = hitByRayCast;
                hoveringWeapon.GetComponent<Outline>().enabled = true;

                if (Input.GetKeyDown(KeyCode.E)) {
                    WeaponManScr.Instance.PickupWeapon(hoveringWeapon);
                }
            } else {
                if (hoveringWeapon != null) {
                    hoveringWeapon.GetComponent<Outline>().enabled = false;
                }
            }

            // AmmoBox
            if (hitByRayCast.GetComponent<AmmoBoxScr>() != null)
            {
                //print("Weapon selected/pointed!");
                hoveringAmmoBox = hitByRayCast;
                hoveringAmmoBox.GetComponent<Outline>().enabled = true;

                if (Input.GetKeyDown(KeyCode.E))
                {
                    WeaponManScr.Instance.PuckupAmmoBox(hoveringAmmoBox);
                    Destroy(hoveringAmmoBox);
                }
            }
            else
            {
                if (hoveringAmmoBox != null)
                {
                    hoveringAmmoBox.GetComponent<Outline>().enabled = false;
                }
            }
        }
    }
}
