using UnityEngine;

public class InteractionManScr : MonoBehaviour
{
    public static InteractionManScr Instance { get; set; }

    private GameObject hoveringWeapon = null;
    private GameObject hoveringAmmoBox = null;
    public GameObject hoveringThrowable = null;

    public float interactionDistance = 12f;

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

    private void Update()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            GameObject hitByRayCast = hit.transform.gameObject;

            if (hit.distance < interactionDistance)
            {
                if (hitByRayCast.GetComponent<WeaponScr>() != null && hitByRayCast.GetComponent<WeaponScr>().isActiveWeapon == false)
                {
                    HandleWeaponInteraction(hitByRayCast);
                }
                else if (hitByRayCast.GetComponent<AmmoBox>() != null)
                {
                    HandleAmmoInteraction(hitByRayCast);
                }
                else if (hitByRayCast.GetComponent<Throwable>() != null)
                {
                    HandleThrowableInteraction(hitByRayCast);
                }
                else
                {
                    if (hoveringWeapon != null)
                    hoveringWeapon.GetComponent<Outline>().enabled = false;
                    if (hoveringAmmoBox != null)
                    hoveringAmmoBox.GetComponent<Outline>().enabled = false;
                    if (hoveringThrowable != null)
                    hoveringThrowable.GetComponent<Outline>().enabled = false;
                    HUBManScr.Instance.hintText.gameObject.SetActive(false);
                }
            }
            else if (hoveringWeapon != null || hoveringAmmoBox != null || hoveringThrowable != null)
            {
                if (hoveringWeapon != null)
                    hoveringWeapon.GetComponent<Outline>().enabled = false;
                if (hoveringAmmoBox != null)
                    hoveringAmmoBox.GetComponent<Outline>().enabled = false;
                if (hoveringThrowable != null)
                    hoveringThrowable.GetComponent<Outline>().enabled = false;
                HUBManScr.Instance.hintText.gameObject.SetActive(false);
            }
        }
    }

    private void HandleWeaponInteraction(GameObject weapon)
    {
        // Check if there is a previous item and disable it outline
        if (hoveringWeapon)
        {
            hoveringWeapon.GetComponent<Outline>().enabled = false;
        }

        hoveringWeapon = weapon;
        hoveringWeapon.GetComponent<Outline>().enabled = true;

        HUBManScr.Instance.hintText.gameObject.SetActive(true);
        HUBManScr.Instance.hintText.text = $"Hold 'e' to grab the {weapon.name}";

        if (Input.GetKeyDown(KeyCode.E))
        {
            WeaponManScr.Instance.PickupWeapon(hoveringWeapon);
        }
    }

    private void HandleAmmoInteraction(GameObject ammo)
    {
        // Check if there is a previous item and disable it outline
        if (hoveringAmmoBox)
        {
            hoveringAmmoBox.GetComponent<Outline>().enabled = false;
        }

        hoveringAmmoBox = ammo;
        hoveringAmmoBox.GetComponent<Outline>().enabled = true;

        HUBManScr.Instance.hintText.gameObject.SetActive(true);
        HUBManScr.Instance.hintText.text = $"Hold 'e' to grab the {ammo.name}";

        if (Input.GetKeyDown(KeyCode.E))
        {
            WeaponManScr.Instance.PuckupAmmoBox(hoveringAmmoBox);
            Destroy(hoveringAmmoBox);
        }
    }

    private void HandleThrowableInteraction(GameObject throwable)
    {
        // Check if there is a previous item and disable it outline
        if (hoveringThrowable)
        {
            hoveringThrowable.GetComponent<Outline>().enabled = false;
        }

        hoveringThrowable = throwable;
        hoveringThrowable.GetComponent<Outline>().enabled = true;

        HUBManScr.Instance.hintText.gameObject.SetActive(true);
        HUBManScr.Instance.hintText.text = $"Hold 'e' to grab the {throwable.name}";

        if (Input.GetKeyDown(KeyCode.E))
        {
            WeaponManScr.Instance.PickupThrowable(hoveringThrowable.GetComponent<Throwable>());
        }
    }
}
