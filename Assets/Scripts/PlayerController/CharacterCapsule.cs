using UnityEngine;

/// <summary>
/// Computes the world-space capsule geometry of a CapsuleCollider (and a
/// slightly shrunk "cast" capsule used for collision sweeps) each time it's
/// refreshed. Plain data holder — no MonoBehaviour, no Unity lifecycle, so
/// it's trivial to unit test or reuse on NPCs later.
/// </summary>
public class CharacterCapsule
{
    /// <summary>World-space center of the capsule.</summary>
    public Vector3 Center { get; private set; }

    /// <summary>Offset from Center to the top hemisphere center (also used for the bottom, mirrored).</summary>
    public Vector3 Offset { get; private set; }

    public float Radius { get; private set; }

    /// <summary>Same offset, kept separate in case cast geometry ever needs to diverge from visual geometry.</summary>
    public Vector3 CastOffset { get; private set; }

    /// <summary>
    /// Radius used for collision sweeps. Deliberately the SAME as the real
    /// radius (not shrunk) — the skin margin is enforced by stopping every
    /// move a little short of the hit distance instead (see
    /// CharacterCollisionSolver). Shrinking the radius here as well used to
    /// double up with that, and after any landed/slide step the capsule was
    /// left touching the surface at exactly 0 distance, so the very next
    /// cast (even a tangential slide) re-hit the same surface at ~0 again —
    /// the character got stuck unable to move along the ground or up ramps.
    /// </summary>
    public float CastRadius { get; private set; }

    public void Refresh(CapsuleCollider collider, float skinWidth)
    {
        Bounds bounds = collider.bounds;

        Center = bounds.center;

        Radius = Mathf.Min(bounds.extents.x, bounds.extents.z);

        float halfHeight = Mathf.Max(0f, bounds.extents.y - Radius);

        Offset = Vector3.up * halfHeight;

        CastRadius = Radius;
        CastOffset = Offset;
    }

    /// <summary>Distance from Center straight down to the very bottom point of the capsule.</summary>
    public float BottomDistance => Offset.magnitude + Radius;
}
