using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class WeaponScr : MonoBehaviour
{
    public bool isActiveWeapon;
    public int weaponDamage;

    [Header("Shooting")]
    // Shooting
    public bool isShooting,
        readyToShoot;
    bool allowReset = true;
    public float shootingDelay = 2f;

    [Header("Burst")]
    // Burst
    [Min(1)]
    public int bulletsPerBurst; // Can't be zero!
    public int burstBulletLeft; // Remove that?

    [Header("Spread")]
    // Spread
    public float spreadIntensity;
    public float hipSpreadIntensity;
    public float adsSpreadIntensity;

    [Header("Bullet")]
    // Bullet
    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    public float bulletVelocity = 30;
    public float bulletPrefabLifeTime = 3f;

    [Header("Others")]
    //Effect
    public GameObject muzzleEffect;

    // Animation
    internal Animator animator; //Script is accessible but not from inspector

    // Loading
    public float reloadTime;
    public int magazineSize,
        bulletsLeft;
    public bool isReloading;

    public enum WeaponModel
    {
        Pistol1911,
        AK47,
    }

    public enum ShootingMode
    {
        Single,
        Burst,
        Auto,
    }

    public WeaponModel weaponModel;
    public ShootingMode currentShootingMode;

    bool isADS;

    [Header("Spawn properties")]
    public Vector3 spawnPosition;
    public Vector3 spawnRotation;

    private  Outline outline;

    private static int recoil_id = Animator.StringToHash("RECOIL");
    private static int recoil_ads_id = Animator.StringToHash("RECOIL_ADS");
    private static int reload_id = Animator.StringToHash("RELOAD");

    private void Awake() // Called when loaded!
    {
        readyToShoot = true;
        burstBulletLeft = bulletsPerBurst;
        animator = this.transform.GetChild(0).GetComponent<Animator>(); // get the child one (AKA Model)

        bulletsLeft = magazineSize;

        spreadIntensity = hipSpreadIntensity;

        // cache
        outline = this.GetComponent<Outline>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isActiveWeapon)
        {
            if (Input.GetMouseButtonDown(1))
            {
                EnterADS();
            }

            if (Input.GetMouseButtonUp(1))
            {
                ExitADS();
            }

            outline.enabled = false;

            if (bulletsLeft == 0 && isShooting)
            {
                SoundManager.Instance.shootingChannel.clip = SoundManager.Instance.emptyMagazine;
                SoundManager.Instance.shootingChannel.Play();
            }

            if (currentShootingMode == ShootingMode.Auto)
            {
                isShooting = Input.GetKey(KeyCode.Mouse0); // Holding
            }
            else if (
                currentShootingMode == ShootingMode.Single
                || currentShootingMode == ShootingMode.Burst
            )
            {
                isShooting = Input.GetKeyDown(KeyCode.Mouse0); // clicking
            }

            if (
                Input.GetKeyDown(KeyCode.R)
                && bulletsLeft < magazineSize
                && !isReloading
                && WeaponManScr.Instance.checkAmmoLeft(weaponModel) > 0
            )
            {
                Reload();
            }

            if (readyToShoot && isShooting && bulletsLeft > 0 && !isReloading)
            {
                burstBulletLeft = bulletsPerBurst;
                FireWeapon();
            }
        }
    }

    private void EnterADS()
    {
        animator.SetBool("ADS_MODE", true);
        isADS = true;
        HUBManScr.Instance.middleDot.SetActive(false);
        spreadIntensity = adsSpreadIntensity;
    }

    private void ExitADS()
    {
        animator.SetBool("ADS_MODE", false);
        isADS = false;
        HUBManScr.Instance.middleDot.SetActive(true);
        spreadIntensity = hipSpreadIntensity;
    }

    private void FireWeapon()
    {
        bulletsLeft--;

        muzzleEffect.GetComponent<ParticleSystem>().Play();

        if (isADS)
        {
            animator.SetTrigger(recoil_ads_id);
        }
        else
        {
            animator.SetTrigger(recoil_id);
        }

        SoundManager.Instance.PlayShootingSound(weaponModel);

        readyToShoot = false;

        Vector3 shootingDirection = calculateDirectionAndSpread().normalized;

        // Create the bullet
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity); // Quaternion = rotation math

        // Setting the damage of a weapon
        Bullet bul = bullet.GetComponent<Bullet>();
        bul.bulletDamage = weaponDamage;

        bullet.transform.forward = shootingDirection;
        bullet
            .GetComponent<Rigidbody>()
            .AddForce(shootingDirection * bulletVelocity, ForceMode.Impulse);
        // Destroy the bullet after his lifetime
        StartCoroutine(DestroyBulletAfterTime(bullet, bulletPrefabLifeTime));

        // Done shooting?
        if (allowReset)
        {
            Invoke("resetShot", shootingDelay); // wth is that
            allowReset = false;
        }

        // Burst
        if (currentShootingMode == ShootingMode.Burst && burstBulletLeft > 1)
        {
            burstBulletLeft--;
            Invoke("FireWeapon", shootingDelay);
        }
    }

    private void Reload()
    {
        SoundManager.Instance.PlayReloadSound(weaponModel);

        animator.SetTrigger(reload_id);

        isReloading = true;
        Invoke("ReloadCompleted", reloadTime); // Why invoke?
    }

    private void ReloadCompleted()
    {
        int ammoAvailable = WeaponManScr.Instance.checkAmmoLeft(weaponModel);
        int bulletsNeeded = magazineSize - bulletsLeft;
        int bulletsToLoad = Mathf.Min(bulletsNeeded, ammoAvailable);

        bulletsLeft += bulletsToLoad;
        WeaponManScr.Instance.DecreaseTotalAmmo(bulletsToLoad, weaponModel);
        isReloading = false;
    }

    private void resetShot()
    {
        readyToShoot = true;
        allowReset = true;
    }

    public Vector3 calculateDirectionAndSpread()
    {
        // Main Camera!
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        Vector3 targetPoint;

        if (Physics.Raycast(ray, out hit)) // reference type
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(100);
        }

        Vector3 direction = targetPoint - bulletSpawn.position;

        // Spread direction
        float z = UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);
        float y = UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);

        // Shooting direction and spread
        return direction + new Vector3(0, y, z);
    }

    private IEnumerator DestroyBulletAfterTime(GameObject bullet, float bulletTime)
    {
        yield return new WaitForSeconds(bulletTime);
        Destroy(bullet);
    }
}
