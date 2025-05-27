using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {
    public int bulletDamage;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Target"))
        {
            print("Hit " + collision.gameObject.name + " !");
            createBulletImpactEffect(collision);
            Destroy(gameObject);
        }
        else if (collision.gameObject.CompareTag("Wall"))
        {
            print("Hit a wall!");
            createBulletImpactEffect(collision);
            Destroy(gameObject);
        }
        else if (collision.gameObject.CompareTag("Beer")) {
            print("Hit a beer");
            collision.gameObject.GetComponent<Beer>().Shatter(); // Cool!
        } else if (collision.gameObject.CompareTag("Zombie"))
        {
            print("Hit a zombie");
            collision.gameObject.GetComponent<Zombie>().TakeDamage(bulletDamage);
        }
    }

    void createBulletImpactEffect(Collision objectHit) {
        ContactPoint contact = objectHit.contacts[0]; // OK ?
        GameObject hole = Instantiate(GlobalRefs.Instance.bulletImpactEffectPrefab, contact.point, Quaternion.LookRotation(contact.normal));
        hole.transform.SetParent(objectHit.gameObject.transform);
    }
}
