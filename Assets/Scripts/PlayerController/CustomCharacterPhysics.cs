using UnityEngine;

public class CustomCharacterPhysics : MonoBehaviour
{
    [Header("Collision")]
    [Min(0.001f)]
    public float skinWidth = 0.05f;

    [Min(1)]
    public int maxBounces = 5;

    [Range(0f, 89f)]
    public float maxSlopeAngle = 45f;

    [Tooltip("Child object containing the CapsuleCollider.")]
    public GameObject meshObject;

    [Header("Ground")]
    [Min(0.001f)]
    public float groundCheckDistance = 0.1f;

    [Header("State")]
    public bool onGround = false;
    public bool debugView = true;

    [Header("Movement")]
    public Vector3 playerVelocity;

    private CapsuleCollider coll;
    private LayerMask environmentMask;

    private readonly CharacterCapsule capsule = new CharacterCapsule();
    private readonly CharacterGroundChecker groundChecker = new CharacterGroundChecker();
    private CharacterCollisionSolver solver;

    // ================================================================
    // INITIALIZATION
    // ================================================================

    private void Start()
    {
        if (meshObject == null)
        {
            Debug.LogError("[CCP] meshObject has not been assigned.", this);
            enabled = false;
            return;
        }

        coll = meshObject.GetComponent<CapsuleCollider>();

        if (coll == null)
        {
            Debug.LogError("[CCP] meshObject does not have a CapsuleCollider.", meshObject);
            enabled = false;
            return;
        }

        if (coll.direction != 1)
        {
            Debug.LogError("[CCP] Body CapsuleCollider must have Direction = Y.");
            enabled = false;
            return;
        }

        int environmentLayer = LayerMask.NameToLayer("Environment");

        if (environmentLayer == -1)
        {
            Debug.LogError(
                "[CCP] The 'Environment' layer does not exist. "
                    + "Create it in Project Settings > Tags and Layers."
            );
            enabled = false;
            return;
        }

        environmentMask = 1 << environmentLayer;

        solver = new CharacterCollisionSolver(
            transform,
            coll,
            environmentMask,
            maxBounces,
            maxSlopeAngle,
            skinWidth
        );

        capsule.Refresh(coll, skinWidth);
        RefreshGroundState();
    }

    // ================================================================
    // GROUND CHECK
    // ================================================================

    private void RefreshGroundState()
    {
        CharacterGroundChecker.Result result = groundChecker.Check(
            capsule,
            environmentMask,
            groundCheckDistance,
            skinWidth,
            maxSlopeAngle
        );

        onGround = result.grounded;

        if (onGround && playerVelocity.y < 0f)
            playerVelocity.y = 0f;

        if (debugView)
        {
            Debug.DrawRay(
                result.rayStart,
                Vector3.down * result.rayDistance,
                onGround ? Color.green : Color.red
            );

            if (result.hit.collider != null)
            {
                Debug.DrawRay(result.hit.point, result.hit.normal * 0.5f, Color.green);
            }
        }
    }

    // ================================================================
    // PUBLIC MOVEMENT API
    // ================================================================

    /// <summary>
    /// Advances the character by one frame of horizontal movement.
    /// IMPORTANT: call this every frame — even with Vector2.zero — since
    /// gravity and ground detection are stepped here too. Only calling it
    /// while there's movement input (as the previous version's driver did)
    /// means the character stops falling whenever no key is held.
    /// </summary>
    public void Move(Vector2 velocity)
    {
        Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.y);

        RestrictMove(horizontalVelocity);
    }

    public void Jump(float desiredHeight)
    {
        if (!onGround)
            return;

        if (desiredHeight <= 0f)
            return;

        float gravity = Mathf.Abs(Physics.gravity.y);

        if (gravity <= Mathf.Epsilon)
            return;

        // v = sqrt(2gh)
        playerVelocity.y = Mathf.Sqrt(2f * gravity * desiredHeight);

        onGround = false;
    }

    // ================================================================
    // DEPENETRATION
    // ================================================================

    /// <summary>
    /// Pushes the capsule out of any environment collider it currently
    /// overlaps. This is a discrete safety net on top of the swept
    /// collide-and-slide above — sweeps only ever ask "what's in the way
    /// between here and there", they don't notice (or fix) an overlap that
    /// already exists at the start. A tiny overlap left behind by rounding
    /// tends to make every subsequent CapsuleCastAll — in any direction —
    /// report an immediate hit, which is what a total movement freeze
    /// actually looks like from here.
    /// </summary>
    private void Depenetrate()
    {
        const int maxIterations = 4;
        const float pushExtra = 0.001f;

        for (int i = 0; i < maxIterations; i++)
        {
            capsule.Refresh(coll, skinWidth);

            Collider[] overlaps = Physics.OverlapCapsule(
                capsule.Center - capsule.CastOffset,
                capsule.Center + capsule.CastOffset,
                capsule.CastRadius,
                environmentMask,
                QueryTriggerInteraction.Ignore
            );

            bool resolvedAny = false;

            foreach (Collider other in overlaps)
            {
                if (other == coll)
                    continue;

                if (other.transform == transform || other.transform.IsChildOf(transform))
                    continue;

                bool overlapping = Physics.ComputePenetration(
                    coll,
                    coll.transform.position,
                    coll.transform.rotation,
                    other,
                    other.transform.position,
                    other.transform.rotation,
                    out Vector3 pushDirection,
                    out float pushDistance
                );

                if (overlapping && pushDistance > 0f)
                {
                    transform.position += pushDirection * (pushDistance + pushExtra);
                    resolvedAny = true;
                }
            }

            if (!resolvedAny)
                break;
        }
    }

    // ================================================================
    // MOVEMENT
    // ================================================================

    private void RestrictMove(Vector3 horizontalVelocity)
    {
        float dt = Time.deltaTime;

        capsule.Refresh(coll, skinWidth);

        // Ground + gravity are stepped here — inside the same call that
        // moves the character — instead of a separate FixedUpdate(). The
        // previous version integrated gravity in FixedUpdate but only ever
        // turned it into an actual position change inside Move(), which was
        // driven by Update(). That mismatch (and Move() only being called
        // conditionally) is what caused inconsistent falling.
        RefreshGroundState();

        if (onGround)
        {
            if (playerVelocity.y < 0f)
                playerVelocity.y = 0f;
        }
        else
        {
            playerVelocity += Physics.gravity * dt;
        }

        Vector3 startPosition = capsule.Center;

        // ------------------------------------------------------------
        // Horizontal movement
        // ------------------------------------------------------------

        Vector3 horizontalMovement = horizontalVelocity * dt;

        CharacterCollisionSolver.Result horizontalResult = solver.Slide(
            horizontalMovement,
            startPosition,
            capsule,
            isVertical: false,
            debugView
        );

        // ------------------------------------------------------------
        // Vertical movement
        // ------------------------------------------------------------

        Vector3 verticalMovement = playerVelocity * dt;

        CharacterCollisionSolver.Result verticalResult = solver.Slide(
            verticalMovement,
            startPosition + horizontalResult.movement,
            capsule,
            isVertical: true,
            debugView
        );

        if (verticalResult.landedOnGround)
        {
            onGround = true;

            if (playerVelocity.y < 0f)
                playerVelocity.y = 0f;
        }

        if (verticalResult.hitCeiling && playerVelocity.y > 0f)
        {
            playerVelocity.y = 0f;
        }

        // ------------------------------------------------------------
        // Total movement
        // ------------------------------------------------------------

        Vector3 totalMovement = horizontalResult.movement + verticalResult.movement;

        // Body is a CHILD of Player — moving Player moves Body automatically.
        transform.position += totalMovement;

        // Safety net: if rounding from the sweeps above (or a corner, or a
        // fast-moving platform) left the capsule slightly embedded in a
        // collider, push it back out now. Left unresolved, this tends to
        // make CapsuleCastAll report an immediate hit in EVERY direction
        // next frame — not just into the wall — which looks exactly like
        // "touched a wall and now nothing works".
        Depenetrate();

        capsule.Refresh(coll, skinWidth);
        RefreshGroundState();

        if (debugView)
        {
            Debug.DrawLine(startPosition, startPosition + horizontalResult.movement, Color.blue);
            Debug.DrawLine(
                startPosition + horizontalResult.movement,
                startPosition + totalMovement,
                Color.yellow
            );
        }
    }
}
