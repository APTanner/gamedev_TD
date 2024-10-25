using Unity.Mathematics;
using UnityEngine;

public static class MathFunctions
{
    public static float2 ClampMagnitude(float2 v, float magnitude)
    {
        return math.select(
            v,
            math.normalizesafe(v) * magnitude,
            math.lengthsq(v) > math.square(magnitude));
    }

    public static float3 ClampMagnitude(float3 v, float magnitude)
    {
        return math.select(
            v,
            math.normalizesafe(v) * magnitude,
            math.lengthsq(v) > math.square(magnitude));
    }

    public static float2 FlattenFloat3(float3 v)
    {
        return new float2(v.x, v.z);
    }

    public static float2 AccelerateTowards(float2 direction,
                                           float2 currentVelocity,
                                           float maxAcceleration,
                                           float maxSpeed = 1f)
    {
        return ClampMagnitude(direction * maxSpeed - currentVelocity, maxAcceleration);
    }

    public static Vector3 ToVector3(this float2 v)
    {
        return new Vector3(v.x, 0, v.y);
    }


    // TEMPORARY
    public static Vector2 xz(this Vector3 v)
    {
        return new Vector2(v.x, v.z);
    }

    public static Vector3 UnflattenVector2(Vector2 v, float y = 0)
    {
        return new Vector3(v.x, y, v.y);
    }

    public static float Cross(Vector2 a, Vector2 b)
    {
        return a.x * b.y - a.y * b.x;
    }

    public static float Square(float a)
    {
        return a * a;
    }
}

