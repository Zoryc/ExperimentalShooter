using UnityEngine;

public class Player : MonoBehaviour
{
    public int HP = 100;

    private void Start()
    {
        
    }

    public void TakeDamage(int damageAmount) {
        HP -= damageAmount;

        if (HP <= 0)
        {
            Debug.LogWarning("Player is dead");
        }
        else 
        {
            Debug.LogWarning("Player hit");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.name);
        if (collision.gameObject.CompareTag("EnemyHand"))
        {
            TakeDamage((int)collision.gameObject.GetComponent<ZombieHand>().damage);
        }
    }
}
