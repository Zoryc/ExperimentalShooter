using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalRefsScr : MonoBehaviour
{
    public static GlobalRefsScr Instance { get; set; }
    public GameObject bulletImpactEffectPrefab;

    private void Awake() // Called when loaded!
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
