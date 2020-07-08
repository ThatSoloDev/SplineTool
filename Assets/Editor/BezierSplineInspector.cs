using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

//ToDo: Refactor

[CustomEditor(typeof(BezierSpline))]
[CanEditMultipleObjects]
public class BezierSplineInspector : Editor //Refactor
{
    private int selectedIndex = -1;

    private BezierSpline _bezierSpline;
    private Transform _handleTransform;
    private Quaternion _handleRotation;

    private Vector3[] _points;

    private SerializedProperty data;

    private SerializedProperty _lineSteps;
    private SerializedProperty _directionSizeScale;
    private SerializedProperty _directionNoiseScale;
    private SerializedProperty _handleSize;
    private SerializedProperty _pickSize;
    private SerializedProperty _showVelocity;
    private SerializedProperty _showPrimitive;


    private void OnEnable()
    {
        _bezierSpline = target as BezierSpline;

        data = serializedObject.FindProperty("splineVisualData");

        _lineSteps = data.FindPropertyRelative("lineSteps");
        _directionSizeScale = data.FindPropertyRelative("directionSizeScale");
        _directionNoiseScale = data.FindPropertyRelative("directionNoiseScale");
        _handleSize = data.FindPropertyRelative("handleSize");
        _pickSize = data.FindPropertyRelative("pickSize");
        _showVelocity = data.FindPropertyRelative("showVelocity");
        _showPrimitive = data.FindPropertyRelative("showPrimitive");

    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        Undo.RecordObject(_bezierSpline, "Modify Inspector");
        EditorUtility.SetDirty(_bezierSpline);

        EditorGUILayout.PropertyField(data);

        EditorGUILayout.LabelField("Curves", new GUIStyle() { alignment = TextAnchor.MiddleCenter });
        for (int i = 0; i < _bezierSpline.points.Count; i++)
        {
            if (i == 0)
            {
                _bezierSpline.points[i] = EditorGUILayout.Vector3Field("Start Point ", _bezierSpline.points[i]);
                continue;
            }

            if ((i - 1) % 3 == 0)
            {
                EditorGUILayout.LabelField("Curve " + (i / 4 + 1));
            }

            _bezierSpline.points[i] = EditorGUILayout.Vector3Field("Point " + (i), _bezierSpline.points[i]);

        }

        if (GUILayout.Button("Add Curve"))
        {
            _bezierSpline.AddCurve();
        }

        if(GUILayout.Button("Remove Curve"))
        {
            _bezierSpline.RemoveCurve();
        }

        SceneView.RepaintAll();
        serializedObject.ApplyModifiedProperties();
    }

    public void OnSceneGUI()
    {
        Undo.RecordObject(_bezierSpline, "Modify Spline");
        EditorUtility.SetDirty(_bezierSpline);

        _handleTransform = _bezierSpline.transform;
        _handleRotation = Tools.pivotRotation == PivotRotation.Local ? _handleTransform.rotation : Quaternion.identity;

        _points = new Vector3[_bezierSpline.points.Count];
        for(int i = 0; i < _points.Length; i++)
        {
            _points[i] = ShowPoint(i);
        }

        DrawPrimitive();
        DrawBezierCurve(_lineSteps.floatValue * _bezierSpline.CurveCount);
        DrawVelocity(_lineSteps.floatValue * _bezierSpline.CurveCount * _directionNoiseScale.floatValue);
    }

    private void DrawVelocity(float steps)
    {
        if (_showVelocity.boolValue == false)
            return;

        if (_directionNoiseScale.floatValue == 0)
            return;

        for (int i = 0; i < steps; i++)
        {
            Vector3 lineEnd = _bezierSpline.GetPoint(i / steps);
            Handles.color = Color.green;
            Handles.DrawLine(lineEnd, lineEnd + _bezierSpline.GetDirection(i / steps) * _directionSizeScale.floatValue);
        }
    }

    private void DrawBezierCurve(float steps)
    {
        Vector3 lineStart = _bezierSpline.GetPoint(0f);

        for (int y = 0; y < steps; y++)
        {
            Vector3 lineEnd = _bezierSpline.GetPoint(y / steps);
            Handles.color = Color.white;
            Handles.DrawLine(lineStart, lineEnd);
            lineStart = lineEnd;
        }
    }

    private void DrawPrimitive()
    {
        if (_showPrimitive.boolValue == false)
            return;

        for (int i = 0; i < _points.Length - 1; i++)
        {
            Handles.color = Color.gray;
            Handles.DrawLine(_points[i], _points[i + 1]);
        }
    }

    private Vector3 ShowPoint(int index)
    {
        Vector3 point = _handleTransform.TransformPoint(_bezierSpline.GetControlPoint(index));

        float size = HandleUtility.GetHandleSize(point);
        Handles.color = Color.white;

        if(Handles.Button(point, _handleRotation, size * _handleSize.floatValue, size * _pickSize.floatValue, Handles.DotHandleCap))
        {
            selectedIndex = index;
        }
        if(selectedIndex == index)
        {
            EditorGUI.BeginChangeCheck();
            point = Handles.DoPositionHandle(point, _handleRotation);
            if (EditorGUI.EndChangeCheck())
            {
                _bezierSpline.SetControlPoint(index, _handleTransform.InverseTransformPoint(point));
            }
        }

        return point;
    }
}
