using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeerScr : MonoBehaviour
{
    public List<Rigidbody> allParts = new List<Rigidbody>();

    public void Shatter() {
        foreach (Rigidbody part in allParts) {
            part.isKinematic = false; // enable gravity and other things?
        }
    }
}
