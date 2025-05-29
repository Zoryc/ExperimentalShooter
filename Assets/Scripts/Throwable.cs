using UnityEngine;

// Continuious - Static object
// Continuious - Dynamic and static objects
// Speculative - Less cool Continuious

public class Throwable : MonoBehaviour
{
    public float delay = 3f;
    public float damageRadius = 20f;
    public float explosionForce = 1200f;

    float countdown;
    
    bool hasExploded = false; // Internal is the default
    public bool hasBeenThrown = false;

    public enum ThrowableType {
        Grenade,
        Smoke_Grenade, // Has no sound for now :(
        None
    }

    public ThrowableType throwableType;

    private void Start()
    {
        countdown = delay;
    }

    private void Update()
    {
        if (hasBeenThrown) {
            countdown -= Time.deltaTime;
            if (countdown <= 0 && !hasExploded) {
                Explode();
                hasExploded = true;
            }
        }
    }

    private void Explode()
    {
        GetThrowableEffect();

        Destroy(gameObject); // this game object
    }

    private void GetThrowableEffect()
    {
        switch (throwableType) { 
        
            case ThrowableType.Grenade:
                GrenadeEffect();
                break;
            case ThrowableType.Smoke_Grenade:
                SmokeGrenadeEffect();
                break;
        }
    }

    private void SmokeGrenadeEffect()
    {
        GameObject explosionEffect = GlobalReferences.Instance.smokeGrenadeEffect;
        Instantiate(explosionEffect, transform.position, transform.rotation);
    }

    private void GrenadeEffect()
    {
        // Visual Effect
        GameObject explosionEffect = GlobalReferences.Instance.grenadeExplosionEffect;
        Instantiate(explosionEffect, transform.position, transform.rotation);

        // Sound
        SoundManager.Instance.shootingChannel.PlayOneShot(SoundManager.Instance.throwableClip);

        // Collider :O
        Collider[] colliders = Physics.OverlapSphere(transform.position, damageRadius); // get all the colliders near
        foreach (Collider coll in colliders) { 
            Rigidbody rb = coll.GetComponent<Rigidbody>();

            if (rb != null) {
                rb.AddExplosionForce(explosionForce, transform.position, damageRadius);
            }

            if (coll.GetComponent<Enemy>() != null)
                coll.GetComponent<Enemy>().TakeDamage(100);
        }

        // damage done here
    }
}

