using UnityEngine;

public class MatrixOperation {

    public static float MatrixMul(Vector3 v1, Vector3 v2) {
        return (v1.x * v2.x) + (v1.y * v2.y) + (v1.z * v2.z);
    }
    public static Vector2 Matrix_Rotation(float x, float y, float angle)
    {
        Vector2 result = new Vector2();
        result.x = (x * Mathf.Cos(angle)) - (y * Mathf.Sin(angle));
        result.y = (x * Mathf.Sin(angle)) + (y * Mathf.Cos(angle));
        return result;
    }
}
