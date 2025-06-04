using UnityEngine;

namespace Assets.Scripts.Math
{
    public class VectorOperation
    {
        public static float GetAngle(Vector3 v1, Vector3 v2) {
            return Mathf.Acos(MatrixOperation.MatrixMul(v1, v2) / (v1.magnitude * v2.magnitude)) * Mathf.Rad2Deg;
        }

        public static float DotProduct(Vector3 v1, Vector3 v2) {
            return v1.magnitude * v2.magnitude * Mathf.Cos(GetAngle(v1, v2) * Mathf.Deg2Rad);
        }

        public static Vector3 ProjectOnSurface(Vector3 v1, Vector3 normal) {
            // do it yourself?
            // https://www.youtube.com/watch?v=qz3Q3v84k9Y
            Vector3 result = Vector3.ProjectOnPlane(v1, normal);
            return result;
        }
    }
}
