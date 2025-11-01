using Assets.Scripts.Math;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class CustomCharacterPhysics : MonoBehaviour
{
    public float skinWidth = 0.15f;
    public int maxBounces = 5;

    public LayerMask colLayer;
    public GameObject meshObject;

    public bool onGround = false;

    public bool debugView = true;

    public Vector3 playerVelocity;
    private CapsuleCollider coll;
    private Vector3 deltaMiddleColl;

    private void Start()
    {
        coll = meshObject.GetComponent<CapsuleCollider>();
        coll.bounds.Expand(skinWidth * 2);

        deltaMiddleColl = Vector3.up * ((coll.bounds.size.y * 0.5f) - coll.radius);
    }

    private void FixedUpdate()
    {
        if (!onGround)
        {
            playerVelocity += Physics.gravity * Time.fixedDeltaTime;
        }
        else if (playerVelocity.magnitude > 0)
        {
            playerVelocity = Vector3.zero;
        }
    }

    private Vector3 SlideCollider(Vector3 mov, Vector3 pos, int depth, bool gravityPass, Vector3 vecInit)
    {
        if (depth >= maxBounces)
            return Vector3.zero;

        float mDist = mov.magnitude + skinWidth;

        RaycastHit hit;
        if (Physics.CapsuleCast(pos - deltaMiddleColl, pos + deltaMiddleColl, coll.radius, (gravityPass) ? Vector3.down : mov.normalized, out hit, mDist, colLayer))
        {
            Vector3 vecToPlane = mov.normalized * (hit.distance - skinWidth);
            Vector3 vecLeftOver = mov - vecToPlane;

            if (vecToPlane.magnitude <= skinWidth)
                vecToPlane = Vector3.zero;

            Vector3 onPlaneVector = VectorOperation.ProjectOnSurface(vecLeftOver, hit.normal).normalized * vecLeftOver.magnitude;
            float floorAngle = VectorOperation.GetAngle(Vector3.up, hit.normal);

            #region DEBUG
            Debug.Log("// -------- \\\\");

            Debug.Log($"[CCP] Depth {depth}");
            Debug.Log($"[CCP] fAngle {floorAngle}");
            Debug.Log($"[CCP] Angle {(VectorOperation.GetAngle(mov, hit.normal) - 90.0f).ToString("F1")}");
            Debug.Log($"[CCP] distance {vecToPlane.magnitude.ToString("F1")}");

            Debug.DrawLine(pos, pos + vecToPlane, Color.red);
            Debug.DrawLine(pos + vecToPlane, pos + vecToPlane + onPlaneVector, Color.red);

            Debug.Log("// -------- \\\\");
            #endregion

            float scale;

            if (floorAngle <= 90.0f) // Ground //
            {
                scale = 1;

                if (gravityPass)
                {
                    onGround = true;
                    return vecToPlane; /* snap to the ground */
                }
            }
            else // Wall - Slope //
            {
                scale = 1 - VectorOperation.DotProduct(
                new Vector3(hit.normal.x, 0, hit.normal.z).normalized,
                -new Vector3(vecInit.x, 0, vecInit.z).normalized
                );

                if (gravityPass)
                    onGround = false;

                // Treat slopes as a wall...
            }

            return vecToPlane + SlideCollider(onPlaneVector * scale, pos + vecToPlane, depth + 1, gravityPass, vecInit);
        }
        else if (gravityPass) {
            onGround = false;
        }

        return mov;
    }

    public void Move(Vector2 vel)
    {
        RestrictMove(new Vector3(vel.x, 0, vel.y));
    }

    private void RestrictMove(Vector3 vel) {
        Vector3 pos = meshObject.transform.position;

        Vector3 movGravity = playerVelocity * Time.deltaTime;
        Vector3 movAmount = vel * Time.deltaTime;

        movAmount = SlideCollider(movAmount, pos, 0, false, movAmount);
        movAmount += SlideCollider(movGravity, pos + movAmount, 0, true, movGravity);

        this.transform.position += movAmount;

        // --- DEBUG --- //
        Debug.DrawLine(pos + movAmount, pos + (movAmount.normalized * vel.magnitude), Color.blue);
        movAmount = SlideCollider(movAmount, pos, 0, false, movAmount);
        movAmount += SlideCollider(movGravity, pos + movAmount, 0, true, movGravity);

        this.transform.position += movAmount;

        // --- DEBUG --- //
        Debug.DrawLine(pos + movAmount, pos + (movAmount.normalized * vel.magnitude), Color.blue);
    }

    public void Jump(float desiredHeight)
    {
        throw new NotImplementedException();
    }
}
