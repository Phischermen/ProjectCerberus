using UnityEngine;

public static class AnimationUtility
{
    public static float superPushAnimationSpeed = 16f;
    public static float basicMoveAndPushSpeed = 8f;
    public static float jumpSpeed = 8f;

    public static float initialFireballSpeed = 8f;
    public static float fireBallAcceleration = 32f;

    //The De Casteljau's Algorithm
    public static Vector3 DeCasteljausAlgorithm(Vector3 A, Vector3 B, Vector3 C, Vector3 D, float t)
    {
        //Linear interpolation = lerp = (1 - t) * A + t * B
        //Could use Vector3.Lerp(A, B, t)

        //To make it faster
        float oneMinusT = 1f - t;

        //Layer 1
        Vector3 Q = oneMinusT * A + t * B;
        Vector3 R = oneMinusT * B + t * C;
        Vector3 S = oneMinusT * C + t * D;

        //Layer 2
        Vector3 P = oneMinusT * Q + t * R;
        Vector3 T = oneMinusT * R + t * S;

        //Final interpolated position
        Vector3 U = oneMinusT * P + t * T;

        return U;
    }
}