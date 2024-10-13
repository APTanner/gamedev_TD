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
}

