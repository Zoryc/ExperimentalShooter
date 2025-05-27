using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Continuious - Static object
// Continuious - Dynamic and static objects
// Speculative - Less cool Continuious

public class Throwable : MonoBehaviour
{
    [SerializeField] float delay = 3f;
    [SerializeField] float damageRadius = 20f;
    [SerializeField] float explosionForce = 1200f;

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
        GameObject explosionEffect = GlobalRefs.Instance.smokeGrenadeEffect;
        Instantiate(explosionEffect, transform.position, transform.rotation);

        // Explosion field ?
    }

    private void GrenadeEffect()
    {
        // Visual Effect
        GameObject explosionEffect = GlobalRefs.Instance.grenadeExplosionEffect;
        Instantiate(explosionEffect, transform.position, transform.rotation);

        // Sound
        SoundManager.Instance.ShootingChannel.PlayOneShot(SoundManager.Instance.throwableClip);

        // Collider :O
        Collider[] colliders = Physics.OverlapSphere(transform.position, damageRadius); // get all the colliders near
        foreach (Collider coll in colliders) { 
            Rigidbody rb = coll.GetComponent<Rigidbody>();

            if (rb != null) {
                rb.AddExplosionForce(explosionForce, transform.position, damageRadius);
            }

            if (coll.GetComponent<Zombie>() != null)
                coll.GetComponent<Zombie>().TakeDamage(100);
        }

        // damage done here
    }
}

