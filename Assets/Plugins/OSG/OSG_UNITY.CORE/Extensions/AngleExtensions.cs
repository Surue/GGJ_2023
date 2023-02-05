
using UnityEngine;

public static class AngleExtensions
{
    public static float NormalizeAngleInDegrees(this float angle, float min = -180f)
    {
        float max = min + 360;
        while (angle>max)
        {
            angle -= 360;
        }
        while (angle < min)
        {
            angle += 360;
        }
        return angle;
    }

    public static float _2PI = Mathf.PI*2;

    public static float NormalizeAngleInRadians(this float angle, float min = -Mathf.PI)
    {
        float max = min+_2PI;
        while (angle>max)
        {
            angle -= _2PI;
        }
        while (angle < min)
        {
            angle += _2PI;
        }
        return angle;
    }

    public static Vector3 NormalizeEulerAngles(this Vector3 e)
    {
        float x = e.x.NormalizeAngleInDegrees();
        float y = e.y.NormalizeAngleInDegrees();
        float z = e.z.NormalizeAngleInDegrees();
        return new Vector3(x,y,z);
    }


}
