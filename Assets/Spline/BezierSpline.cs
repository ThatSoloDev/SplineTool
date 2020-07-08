using System;
using System.Collections.Generic;
using UnityEngine;

public static class Spline
{
    public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) //Gets point on curve that is t steps away from start point
    {
        Vector3 point = (1 - t) * (1 - t) * (1 - t) * p0 +
            3f * (1 - t) * (1 - t) * t * p1 +
            3f * (1 - t) * t * t * p2 +
            t * t * t * p3;

        return point;
    }

    public static Vector3 GetDerivative(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) //Gets derivative of point that is t steps away from start point
    {
        Vector3 derivative = 3f * (1 - t) * (1 - t) * (p1 - p0) +
        6f * (1 - t) * t * (p2 - p1) +
        3f * t * t * (p3 - p2);

        return derivative;
    }
}

[Serializable]
public class BezierSplineData
{
    public bool showVelocity = false;
    public bool showPrimitive = false;
    [Range(0, 100)]
    public float lineSteps = 50;

    [Range(0, 2)]
    public float directionSizeScale = 1, directionNoiseScale = 1;

    [Range(0, 0.3f)]
    public float handleSize = 0.1f, pickSize = 0.06f;
}
public class BezierSpline : MonoBehaviour
{
    public BezierSplineData splineVisualData;

    public int CurveCount => (points.Count) / 3;

    public List<Vector3> points = new List<Vector3>();

    public void Reset()
    {
        points.Add(new Vector3(1, 0, 0));
        AddCurve();
    }

    public Vector3 GetControlPoint (int index)
    {
        return points[index];
    }

    public void SetControlPoint(int index, Vector3 point)
    {
        points[index] = point;
    }

    public void AddCurve()
    {
        for (int i = 0; i < 3; i++)
        {
            Vector3 point = new Vector3(points.Count + 1, 0f, 0f);

            points.Add(point);
        }
    }

    public void RemoveCurve()
    {
        if (CurveCount <= 1)
            return;

        for (int i = 0; i < 3; i++)
        {
            points.Remove(points[points.Count - 1]);
        }
    }

    public Vector3 GetPoint(float t)
    {
        int i;
        if(t >= 1f)
        {
            t = 1f;
            i = points.Count - 4;
        }
        else
        {
            t = Mathf.Clamp01(t) * CurveCount;
            i = (int)t;
            t -= i;
            i *= 3;
        }

        Vector3 point = transform.TransformPoint(Spline.GetPoint(points[i], points[i + 1], points[i + 2], points[i + 3], t));

        return point;
    }

    public Vector3 GetVelocity(float t)
    {
        int i;
        if (t >= 1f)
        {
            t = 1f;
            i = points.Count - 4;
        }
        else
        {
            t = Mathf.Clamp01(t) * CurveCount;
            i = (int)t;
            t -= i;
            i *= 3;
        }

        Vector3 velocity = transform.TransformPoint(Spline.GetDerivative(points[i], points[i + 1], points[i + 2], points[i + 3], t)) - transform.position;

        return velocity;
    }

    public Vector3 GetDirection(float t)
    {
        Vector3 direction = GetVelocity(t).normalized;

        return direction;
    }
}
