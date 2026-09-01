using UnityEngine;

/// <summary>
/// Resolves a desired movement vector against the environment via recursive
/// "collide and slide" capsule casts.
/// </summary>
public class CharacterCollisionSolver
{
    public struct Result
    {
        public Vector3 movement;
        public bool landedOnGround;
        public bool hitCeiling;
    }

    private readonly Transform ignoreRoot;
    private readonly Collider ignoreCollider;
    private readonly LayerMask environmentMask;
    private readonly int maxBounces;
    private readonly float maxSlopeAngle;
    private readonly float skinWidth;

    public CharacterCollisionSolver(
        Transform ignoreRoot,
        Collider ignoreCollider,
        LayerMask environmentMask,
        int maxBounces,
        float maxSlopeAngle,
        float skinWidth
    )
    {
        this.ignoreRoot = ignoreRoot;
        this.ignoreCollider = ignoreCollider;
        this.environmentMask = environmentMask;
        this.maxBounces = maxBounces;
        this.maxSlopeAngle = maxSlopeAngle;
        this.skinWidth = skinWidth;
    }

    /// <param name="isVertical">
    /// True only for the gravity/jump pass. Only that pass is allowed to
    /// "land" on a walkable floor and stop dead on contact — if the
    /// horizontal pass did the same thing, the character would freeze at
    /// the base of every ramp instead of walking up it (this was the main
    /// bug in the previous version).
    /// </param>
    public Result Slide(
        Vector3 movement,
        Vector3 position,
        CharacterCapsule capsule,
        bool isVertical,
        bool debugView
    )
    {
        return SlideRecursive(movement, position, capsule, isVertical, debugView, 0);
    }

    private Result SlideRecursive(
        Vector3 movement,
        Vector3 position,
        CharacterCapsule capsule,
        bool isVertical,
        bool debugView,
        int depth
    )
    {
        if (movement.sqrMagnitude <= Mathf.Epsilon || depth >= maxBounces)
            return new Result { movement = Vector3.zero };

        Vector3 direction = movement.normalized;
        float castDistance = movement.magnitude + skinWidth;

        if (!GetClosestHit(position, capsule, direction, castDistance, out RaycastHit hit))
            return new Result { movement = movement };

        // Always stop a little short of the actual surface (skinWidth) so a
        // real gap remains afterwards. Without this, the capsule ends each
        // step touching the surface at exactly 0 distance, and the next
        // recursive cast — even one that's meant to slide tangentially
        // along that same surface — immediately re-detects it at ~0
        // distance too, so the recursion burns through maxBounces without
        // ever making progress (frozen against flat ground or a ramp).
        float moveDistance = Mathf.Max(0f, hit.distance - skinWidth);
        Vector3 movementToHit = direction * moveDistance;
        Vector3 remainingMovement = movement - movementToHit;

        float surfaceAngle = Vector3.Angle(Vector3.up, hit.normal);
        bool walkable = surfaceAngle <= maxSlopeAngle;

        if (debugView)
        {
            Debug.DrawLine(position, position + movementToHit, Color.red);
            Debug.DrawRay(hit.point, hit.normal * 0.5f, Color.green);
        }

        // Only the vertical/falling pass stops on a walkable floor.
        if (isVertical && walkable && movement.y <= 0f && Vector3.Dot(movement, hit.normal) < 0f)
        {
            return new Result { movement = movementToHit, landedOnGround = true };
        }

        bool ceiling = Vector3.Dot(hit.normal, Vector3.up) < -0.5f;

        Vector3 slideMovement = Vector3.ProjectOnPlane(remainingMovement, hit.normal);

        if (slideMovement.sqrMagnitude <= Mathf.Epsilon)
            return new Result { movement = movementToHit, hitCeiling = ceiling };

        Result next = SlideRecursive(
            slideMovement,
            position + movementToHit,
            capsule,
            isVertical,
            debugView,
            depth + 1
        );

        return new Result
        {
            movement = movementToHit + next.movement,
            landedOnGround = next.landedOnGround,
            hitCeiling = ceiling || next.hitCeiling,
        };
    }

    private bool GetClosestHit(
        Vector3 position,
        CharacterCapsule capsule,
        Vector3 direction,
        float distance,
        out RaycastHit closestHit
    )
    {
        closestHit = default;

        RaycastHit[] hits = Physics.CapsuleCastAll(
            position - capsule.CastOffset,
            position + capsule.CastOffset,
            capsule.CastRadius,
            direction,
            distance,
            environmentMask,
            QueryTriggerInteraction.Ignore
        );

        bool found = false;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];

            // Environment is the only layer queried, so the body itself
            // shouldn't normally appear here — kept as extra protection.
            if (hit.collider == ignoreCollider)
                continue;

            if (
                hit.collider.transform == ignoreRoot
                || hit.collider.transform.IsChildOf(ignoreRoot)
            )
                continue;

            if (hit.distance < closestDistance)
            {
                closestDistance = hit.distance;
                closestHit = hit;
                found = true;
            }
        }

        return found;
    }
}
