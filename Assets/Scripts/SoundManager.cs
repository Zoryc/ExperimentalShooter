using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; set; }

    public AudioSource shootingChannel;

    public AudioClip shot_1911;
    public AudioClip shot_AK47;

    public AudioClip reload_1911;
    public AudioClip reload_AK47;

    public AudioClip emptyMagazine;

    public AudioClip throwableClip;

    public AudioClip zombieWalking;

    public AudioClip gameOver;

    public AudioClip playerHit;

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

    public void PlayShootingSound(WeaponScr.WeaponModel weapon) {
        switch (weapon) {
            case WeaponScr.WeaponModel.Pistol1911:
                shootingChannel.PlayOneShot(shot_1911);
                break;
            case WeaponScr.WeaponModel.AK47:
                shootingChannel.PlayOneShot(shot_AK47);
                break;
        }
    }

    public void PlayReloadSound(WeaponScr.WeaponModel weapon)
    {
        switch (weapon)
        {
            case WeaponScr.WeaponModel.Pistol1911:
                shootingChannel.PlayOneShot(reload_1911);
                break;
            case WeaponScr.WeaponModel.AK47:
                shootingChannel.PlayOneShot(reload_AK47);
                break;
        }
    }
}
