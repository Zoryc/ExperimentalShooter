using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SManagerScr : MonoBehaviour
{
    public static SManagerScr Instance { get; set; }

    public AudioSource ShootingChannel;

    public AudioClip shot_1911;
    public AudioClip shot_AK47;

    public AudioClip reload_1911;
    public AudioClip reload_AK47;

    // ----
    public AudioSource emptySound1911;

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

    public void playShootingSound(WeaponScr.WeaponModel weapon) {
        switch (weapon) {
            case WeaponScr.WeaponModel.Pistol1911:
                ShootingChannel.PlayOneShot(shot_1911);
                break;
            case WeaponScr.WeaponModel.AK47:
                ShootingChannel.PlayOneShot(shot_AK47);
                break;
        }
    }

    public void playReloadSound(WeaponScr.WeaponModel weapon)
    {
        switch (weapon)
        {
            case WeaponScr.WeaponModel.Pistol1911:
                ShootingChannel.PlayOneShot(reload_1911);
                break;
            case WeaponScr.WeaponModel.AK47:
                ShootingChannel.PlayOneShot(reload_AK47);
                break;
        }
    }
}
