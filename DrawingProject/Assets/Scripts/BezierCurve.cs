using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BezierCurve : MonoBehaviour
{
    public GameObject target;

    [Range(0, 1)]
    public float time;

    //public List<Vector3> posList;
    public Vector3 p1, p2, p3, p4;

    private void Update()
    {
        target.transform.position = BezierTest(p1, p2, p3, p4, time);
    }
    /*public Vector3 GetPos_Multi(List<Vector3> _originList, float _time)
    {
        List<Vector3> ListInput = new List<Vector3>();
        for (int i = 0; i < _originList.Count; i++)
            ListInput.Add(_originList[i]);

        while (ListInput.Count >= 2)
            ListInput = GetLowerBezier(ListInput, _time);

        return ListInput[0];
    }
    private List<Vector3> GetLowerBezier(List<Vector3> _originList, float _time)
    {
        List<Vector3> FinalList = new List<Vector3>();

        if(_originList.Count >= 2)
        {
            for (int i = 0; i < _originList.Count - 1; i++)
            {
                Vector3 addValue = Vector3.Lerp(_originList[i], _originList[i + 1], _time);
                FinalList.Add(addValue);
            }
        }
        return FinalList;
    }*/
    public Vector3 BezierTest(Vector3 _p1, Vector3 _p2, Vector3 _p3, Vector3 _p4, float _time)
    {
        Vector3 A = Vector3.Lerp(_p1, _p2, _time);
        Vector3 B = Vector3.Lerp(_p2, _p3, _time);
        Vector3 C = Vector3.Lerp(_p3, _p4, _time);

        Vector3 D = Vector3.Lerp(A, B, _time);
        Vector3 E = Vector3.Lerp(B, C, _time);
        Vector3 F = Vector3.Lerp(D, E, _time);
        return F;
    }
}

[CanEditMultipleObjects]
[CustomEditor(typeof(BezierCurve))]
public class BezierEditor : Editor
{
    private void OnSceneGUI()
    {
        BezierCurve generator = (BezierCurve)target;

        generator.p1 = Handles.PositionHandle(generator.p1, Quaternion.identity);
        generator.p2 = Handles.PositionHandle(generator.p2, Quaternion.identity);
        generator.p3 = Handles.PositionHandle(generator.p3, Quaternion.identity);
        generator.p4 = Handles.PositionHandle(generator.p4, Quaternion.identity);

        Handles.DrawLine(generator.p1, generator.p2);
        Handles.DrawLine(generator.p3, generator.p4);

        int count = 50;
        for (float i = 0; i < count; i++)
        {
            float value_before = i / count;
            Vector3 before = generator.BezierTest(generator.p1, generator.p2, generator.p3, generator.p4, value_before);

            float value_after = (i + 1) / count;
            Vector3 after = generator.BezierTest(generator.p1, generator.p2, generator.p3, generator.p4, value_after);

            Handles.DrawLine(before, after);
        }
    }
}