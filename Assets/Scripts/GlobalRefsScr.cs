using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalRefsScr : MonoBehaviour
{
    public static GlobalRefsScr Instance { get; set; }
    public GameObject bulletImpactEffectPrefab;
    public GameObject grenadeExplosionEffect;
    public GameObject smokeGrenadeEffect;

    private void Awake() // Called when loaded! Not even started
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else {
            Instance = this;
        }
    }
}
