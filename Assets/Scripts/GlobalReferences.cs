using UnityEngine;

public class GlobalReferences : MonoBehaviour
{
    public static GlobalReferences Instance { get; set; }
    public GameObject bulletImpactEffectPrefab;
    public GameObject grenadeExplosionEffect;
    public GameObject smokeGrenadeEffect;
    public GameObject bloodTrayEffect;

    public int waveNumer;

    private void Awake() // Called when loaded! Not even started
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else {
            Instance = this;
        }
        DontDestroyOnLoad(Instance);
    }
}
