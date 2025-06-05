using System;
using UnityEngine;

public class Bullet : MonoBehaviour {
    public int bulletDamage;
    public LayerMask impactSurfaceLayer;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Beer"))
        {
            Debug.Log("Hit a beer");
            collision.gameObject.GetComponent<Beer>().Shatter();
            Destroy(gameObject);
        }
        else if (collision.gameObject.CompareTag("Zombie"))
        {
            Debug.Log("Hit a zombie");

            if (collision.gameObject.GetComponent<Enemy>().isDead == false)
                collision.gameObject.GetComponent<Enemy>().TakeDamage(bulletDamage);

            CreateBloodSprayEffect(collision);

            Destroy(gameObject);
        }
        else if (((1 << collision.gameObject.layer) & impactSurfaceLayer) != 0)
        {
            Debug.Log("Hit " + collision.gameObject.name + " !");
            CreateBulletImpactEffect(collision);
            Destroy(gameObject);
        }
    }

    private void CreateBloodSprayEffect(Collision objectHit)
    {
        ContactPoint contact = objectHit.contacts[0];
        GameObject bloodSprayPrefab = Instantiate(GlobalReferences.Instance.bloodTrayEffect, contact.point, Quaternion.LookRotation(contact.normal));
        bloodSprayPrefab.transform.SetParent(objectHit.gameObject.transform);
    }

    void CreateBulletImpactEffect(Collision objectHit) 
    {
        ContactPoint contact = objectHit.contacts[0];
        GameObject hole = Instantiate(GlobalReferences.Instance.bulletImpactEffectPrefab, contact.point, Quaternion.LookRotation(contact.normal));
        hole.transform.SetParent(objectHit.gameObject.transform);
    }
}
