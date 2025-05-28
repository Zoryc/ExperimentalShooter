using UnityEngine;

public class Zombie : MonoBehaviour
{
    public ZombieHand hand;
    public float zombieDamage;

    private void Start()
    {
        hand.damage = zombieDamage;
    }
}
