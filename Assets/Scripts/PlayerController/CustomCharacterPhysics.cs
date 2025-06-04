using Assets.Scripts.Math;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CustomCharacterPhysics : MonoBehaviour
{
    public float skinWidth = 0.15f;
    public int maxBounces = 5;
    public float distanceTheshold = 0.2f;

    public LayerMask colLayer;
    public GameObject meshObject;

    public bool OnGround = false;

    public bool debugView = true;

    public Vector3 playerVelocity;

    private CapsuleCollider coll;
    private Vector3 deltaMiddleColl;

    private void Start()
    {
        coll = meshObject.GetComponent<CapsuleCollider>();
        deltaMiddleColl = Vector3.up * ((coll.height * 0.5f));
    }

    private void FixedUpdate()
    {
        if (!OnGround)
        {
            playerVelocity.y += -Physics.gravity.magnitude * Time.fixedDeltaTime;
        }
        else if (playerVelocity.magnitude > 0) 
        {
            playerVelocity = Vector3.zero;
        }
    }

    private void Update()
    {
        // -- Apply the gravity -- //
        Move(Vector2.zero);
    }

    private Vector3 CollideAndSlide(Vector3 mov, Vector3 pos, int depth, bool gravityPass, Vector3 vecInit)
    {
        if (depth >= maxBounces)
            return Vector3.zero;

        float mDist = mov.magnitude + skinWidth;

        RaycastHit hit;
        if (Physics.CapsuleCast(pos + deltaMiddleColl, pos - deltaMiddleColl, coll.radius, (gravityPass) ? Vector3.down : mov.normalized, out hit, mDist, colLayer))
        {
            Vector3 vecToPlane = mov.normalized * (hit.distance - skinWidth);
            Vector3 vecLeftOver = mov - vecToPlane;

            if (vecToPlane.magnitude <= skinWidth)
                vecToPlane = Vector3.zero;

            Vector3 onPlaneVector = VectorOperation.ProjectOnSurface(vecLeftOver, hit.normal).normalized * vecLeftOver.magnitude;
            float floorAngle = VectorOperation.GetAngle(Vector3.up, hit.normal);

            #region DEBUG
            Debug.Log("// -------- //");

            Debug.Log($"[CCP] Depth {depth}");
            Debug.Log($"[CCP] fAngle {floorAngle}");
            Debug.Log($"[CCP] Angle {(VectorOperation.GetAngle(mov, hit.normal) - 90).ToString("F1")}");
            Debug.Log($"[CCP] distance {vecToPlane.magnitude.ToString("F1")}");

            Debug.DrawLine(pos, pos + vecToPlane, Color.red, 0.1f);
            Debug.DrawLine(pos + vecToPlane, pos + vecToPlane + onPlaneVector, Color.red, 0.1f);

            Debug.Log("// -------- //");
            #endregion

            float scale = 1 - VectorOperation.DotProduct(
                new Vector3(hit.normal.x, 0, hit.normal.z).normalized,
                -new Vector3(vecInit.x, 0, vecInit.z).normalized);

            if (floorAngle <= 90) // Ground //
            {
                scale = 1;

                if (gravityPass)
                {
                    OnGround = true;
                    return vecToPlane; /* snap to the ground */
                }
            }
            else // Wall - Slope //
            {
                
                // Treat slopes as a wall...

            }

            return vecToPlane + CollideAndSlide(onPlaneVector * scale, pos + vecToPlane, depth + 1, gravityPass, vecInit);
        } else if (gravityPass)
            OnGround = false;

        return mov;
    }

    public void Move(Vector2 vel)
    {
        Vector3 speed = new Vector3(vel.x, 0, vel.y);
        Vector3 pos = meshObject.transform.position;

        Vector3 movGravity = playerVelocity * Time.deltaTime;
        Vector3 movAmount = speed * Time.deltaTime;

        movAmount = CollideAndSlide(movAmount, pos, 0, false, movAmount);
        movAmount += CollideAndSlide(movGravity, pos + movAmount, 0, true, movGravity);
        this.transform.position += movAmount;

        // --- DEBUG --- //
        Debug.DrawLine(pos + movAmount, pos + (movAmount.normalized * speed.magnitude), Color.blue);
        // --- DEBUG --- //
    }

    public void Jump(float desiredHeight)
    {
        throw new NotImplementedException();
    }

    private void OnDrawGizmos()
    {
    }
}
