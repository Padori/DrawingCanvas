using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using IndieStudio.DrawingAndColoring;

public enum DrawMode
{
    Pen, Line, Hand
}
public class DrawManager : MonoBehaviour
{
    public DrawMode drawMode;

    public GameObject linePrefab;
    public RectTransform Rect;
    public Transform LineHolder;
    public Text mouseText;

    private bool isClick = false;

    private LineRenderer lr;
    private List<Vector3> points = new List<Vector3>();

    public Dictionary<int, GameObject> LineList = new Dictionary<int, GameObject>();
    public int LineCount = 0;

    public DrawColorPick theDCP;

    GameObject go;

    public float thickness = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        drawMode = DrawMode.Hand;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //이미지 중앙 기준
        mouseText.text = mousePos.ToString();

        if (RectTransformUtility.RectangleContainsScreenPoint(Rect, mousePos))
        {
            if (Input.GetKeyDown(KeyCode.Mouse0) && drawMode != DrawMode.Hand)
            {
                isClick = true;
                go = Instantiate(linePrefab);
                lr = go.GetComponent<LineRenderer>();
                lr.startWidth = thickness;
                lr.endWidth = thickness;
                lr.startColor = theDCP.resultColor;
                lr.endColor = theDCP.resultColor;
                //lr.startColor = new Color(mr, mg, mb, ma);
                //lr.endColor = new Color(mr, mg, mb, ma);
                points.Add(mousePos);
                lr.positionCount = 1;
                lr.SetPosition(0, points[0]);
            }
            else if (Input.GetKey(KeyCode.Mouse0) && isClick)
            {
                Vector2 pos = mousePos;
                if (drawMode == DrawMode.Pen)
                {
                    if (Vector2.Distance(points[points.Count - 1], pos) > Mathf.Epsilon)
                    {
                        points.Add(pos);
                        lr.positionCount++;
                        lr.SetPosition(lr.positionCount - 1, pos);
                    }
                }
                else if (drawMode == DrawMode.Line)
                {
                    lr.positionCount = 2;
                    lr.SetPosition(1, pos);
                }
            }
            else if (Input.GetKeyUp(KeyCode.Mouse0) && isClick)
            {
                isClick = false;
                GenerateLine();
            }
        }
        else
        {
            if (isClick)
            {
                isClick = false;
                GenerateLine();
            }
        }
        if (Input.GetKeyDown(KeyCode.J))
            UndoButton();
        if (Input.GetKeyDown(KeyCode.K))
            RedoButton();
        if (Input.GetKeyDown(KeyCode.S))
            StartCoroutine(SaveScreen());
    }
    public void GenerateLine()
    {
        GameObject line = Instantiate(linePrefab);
        line.name = "lines";
        LineRenderer liner = line.GetComponent<LineRenderer>();

        BezierPath bezierPath = new BezierPath();
        bezierPath.Interpolate(points, 0.3f);
        List<Vector3> smoothedPoints = bezierPath.GetDrawingPoints2();

        liner.positionCount = smoothedPoints.Count;
        liner.SetPositions(smoothedPoints.ToArray());
        liner.startWidth = thickness;
        liner.endWidth = thickness;
        liner.startColor = theDCP.resultColor;
        liner.endColor = theDCP.resultColor;
        Destroy(go);
        points.Clear();

        if (LineList.ContainsKey(LineCount))
        {
            Destroy(LineList[LineCount]);
            LineList.Remove(LineCount);
        }

        LineList.Add(LineCount, line);
        LineCount++;
    }
    public void ClickMode(string _mode)
    {
        switch (_mode)
        {
            case "pen":
                drawMode = DrawMode.Pen;
                break;
            case "line":
                drawMode = DrawMode.Line;
                break;
            case "hand":
                drawMode = DrawMode.Hand;
                break;
            default:
                drawMode = DrawMode.Hand;
                break;
        }
    }
    public void UndoButton()
    {
        if (LineList.Count > 0 && LineCount > 0)
        {
            LineList[LineCount - 1].SetActive(false);
            LineCount -= 1;
        }
    }
    public void RedoButton()
    {
        if (LineList.Count > 0 && LineCount < LineList.Count)
        {
            LineList[LineCount].SetActive(true);
            LineCount += 1;
        }
    }
    public void SetWidth(Slider slider)
    {
        thickness = slider.value;
    }
    IEnumerator SaveScreen()
    {
        yield return new WaitForEndOfFrame();
        int width = Screen.width;
        int height = Screen.height - 250;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB565, false);
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();

        //바이트로 변환
        byte[] bytes = tex.EncodeToPNG();
        Object.Destroy(tex);
        File.WriteAllBytes(Application.dataPath + "/Files/test.png", bytes);
    }
    public Vector3[] SmoothLine(List<Vector2> inputPoints, float segmentSize)
    {
        //create curves
        AnimationCurve curveX = new AnimationCurve();
        AnimationCurve curveY = new AnimationCurve();

        //create keyframe sets
        Keyframe[] keysX = new Keyframe[inputPoints.Count];
        Keyframe[] keysY = new Keyframe[inputPoints.Count];

        //set keyframes
        for (int i = 0; i < inputPoints.Count; i++)
        {
            keysX[i] = new Keyframe(i, inputPoints[i].x);
            keysY[i] = new Keyframe(i, inputPoints[i].y);
        }

        //apply keyframes to curves
        curveX.keys = keysX;
        curveY.keys = keysY;

        //smooth curve tangents
        for (int i = 0; i < inputPoints.Count; i++)
        {
            curveX.SmoothTangents(i, 0);
            curveY.SmoothTangents(i, 0);
        }

        //list to write smoothed values to
        List<Vector3> lineSegments = new List<Vector3>();

        //find segments in each section
        for (int i = 0; i < inputPoints.Count; i++)
        {
            //add first point
            lineSegments.Add(inputPoints[i]);

            //make sure within range of array
            if (i + 1 < inputPoints.Count)
            {
                //find distance to next point
                float distanceToNext = Vector2.Distance(inputPoints[i], inputPoints[i + 1]);

                //number of segments
                int segments = (int)(distanceToNext / segmentSize);

                //add segments
                for (int s = 1; s < segments; s++)
                {
                    //interpolated time on curve
                    float time = ((float)s / (float)segments) + (float)i;

                    //sample curves to find smoothed position
                    Vector3 newSegment = new Vector3(curveX.Evaluate(time), curveY.Evaluate(time), 0f);

                    //add to list
                    lineSegments.Add(newSegment);
                }
            }
        }

        return lineSegments.ToArray();
    }
}