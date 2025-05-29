using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WeaponScr : MonoBehaviour
{
    public bool isActiveWeapon;
    public int weaponDamage;

    [Header("Shooting")]
    // Shooting
    public bool isShooting, readyToShoot;
    bool allowReset = true;
    public float shootingDelay = 2f;

    [Header("Burst")]
    // Burst
    public int bulletsPerBurst = 1; // Can't be zero!
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
    public int magazineSize, bulletsLeft;
    public bool isReloading;

    public enum WeaponModel { 
        Pistol1911,
        AK47
    }

    public enum ShootingMode { 
        Single,
        Burst,
        Auto
    }

    public WeaponModel weaponModel;
    public ShootingMode currentShootingMode;

    bool isABS;

    [Header("Spawn properties")]
    public Vector3 spawnPosition;
    public Vector3 spawnRotation;

    private void Awake() // Called when loaded!
    {
        readyToShoot = true;
        burstBulletLeft = bulletsPerBurst;
        animator = this.transform.GetChild(0).GetComponent<Animator>(); // get the child one (AKA Model)

        bulletsLeft = magazineSize;

        spreadIntensity = hipSpreadIntensity;
    }

    // Update is called once per frame
    void Update()
    {
        if (isActiveWeapon) {

            if (Input.GetMouseButtonDown(1))
            {
                EnterADS();
            }

            if (Input.GetMouseButtonUp(1))
            {
                ExitADS();
            }

            GetComponent<Outline>().enabled = false;

            if (bulletsLeft == 0 && isShooting)
            {
                SoundManager.Instance.shootingChannel.clip = SoundManager.Instance.emptyMagazine;
                SoundManager.Instance.shootingChannel.Play();
            }

            if (currentShootingMode == ShootingMode.Auto)
            {
                isShooting = Input.GetKey(KeyCode.Mouse0); // Holding
            }
            else if (currentShootingMode == ShootingMode.Single || currentShootingMode == ShootingMode.Burst)
            {

                isShooting = Input.GetKeyDown(KeyCode.Mouse0); // clicking
            }

            if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !isReloading && WeaponManScr.Instance.checkAmmoLeft(weaponModel) > 0)
            {
                Reload();
            }

            if (readyToShoot && isShooting && bulletsLeft > 0)
            {
                burstBulletLeft = bulletsPerBurst;
                FireWeapon();
            }
        }
    }

    private void EnterADS() {
        animator.SetTrigger("enterADS");
        isABS = true;
        HUBManScr.Instance.middleDot.SetActive(false);
        spreadIntensity = adsSpreadIntensity;
    }

    private void ExitADS() {
        animator.SetTrigger("exitADS");
        isABS = false;
        HUBManScr.Instance.middleDot.SetActive(true);
        spreadIntensity = hipSpreadIntensity;
    }

    private void FireWeapon()
    {
        bulletsLeft--;

        muzzleEffect.GetComponent<ParticleSystem>().Play();

        if (isABS)
        {
            animator.SetTrigger("RECOIL_ADS");
        } else {
            animator.SetTrigger("RECOIL");
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
        bullet.GetComponent<Rigidbody>().AddForce(bulletSpawn.forward.normalized * bulletVelocity, ForceMode.Impulse);
        // Destroy the bullet after his lifetime
        StartCoroutine(DestroyBulletAfterTime(bullet, bulletPrefabLifeTime));

        // Done shooting?
        if (allowReset) {
            Invoke("resetShot", shootingDelay); // wth is that
            allowReset = false;
        }

        // Burst 
        if (currentShootingMode == ShootingMode.Burst && burstBulletLeft > 1) {
            burstBulletLeft--;
            Invoke("FireWeapon", shootingDelay);
        }
    }

    private void Reload()
    {
        SoundManager.Instance.PlayReloadSound(weaponModel);

        // Problem with 1911
        animator.SetTrigger("RELOAD");

        isReloading = true;
        Invoke("ReloadCompleted", reloadTime); // Why invoke?
    }

    private void ReloadCompleted() {

        if (WeaponManScr.Instance.checkAmmoLeft(weaponModel) > 0)
        {
            bulletsLeft = magazineSize;
        } else {
            bulletsLeft = WeaponManScr.Instance.checkAmmoLeft(weaponModel);
        }

        WeaponManScr.Instance.DecreaseTotalAmmo(magazineSize, weaponModel);
        isReloading = false;
    }

    private void resetShot() {
        readyToShoot = true;
        allowReset = true;
    }

    public Vector3 calculateDirectionAndSpread() {

        // Main Camera!
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0f, 0f, 0));
        RaycastHit hit;

        Vector3 targetPoint;

        if (Physics.Raycast(ray, out hit)) // reference type
        {
            targetPoint = hit.point;
        }
        else {
            targetPoint = ray.GetPoint(100); // ok...
        }

        Vector3 direction = targetPoint - bulletSpawn.position;

        // Spread direction
        float z = UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);
        float y = UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);

        // Shooting direction and spread
        return direction + new Vector3(0, y, z);
    }

    private IEnumerator DestroyBulletAfterTime(GameObject bullet, float bulletTime) {
        yield return new WaitForSeconds(bulletTime);
        Destroy(bullet);
    }
}
