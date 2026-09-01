using UnityEngine;

/// <summary>
/// Raycast-based ground detection for a character capsule.
/// </summary>
public class CharacterGroundChecker
{
    public struct Result
    {
        public bool grounded;
        public RaycastHit hit;
        public float angle;
        public Vector3 rayStart;
        public float rayDistance;
    }

    public Result Check(
        CharacterCapsule capsule,
        LayerMask environmentMask,
        float groundCheckDistance,
        float skinWidth,
        float maxSlopeAngle
    )
    {
        // Start slightly ABOVE the true bottom of the capsule so the ray
        // isn't embedded in the environment while standing on a surface.
        Vector3 rayStart = capsule.Center - Vector3.up * capsule.BottomDistance + Vector3.up * skinWidth;
        float rayDistance = groundCheckDistance + skinWidth;

        var result = new Result { rayStart = rayStart, rayDistance = rayDistance };

        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, rayDistance, environmentMask, QueryTriggerInteraction.Ignore))
        {
            result.hit = hit;
            result.angle = Vector3.Angle(Vector3.up, hit.normal);
            result.grounded = result.angle <= maxSlopeAngle;
        }

        return result;
    }
}
