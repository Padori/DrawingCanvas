using SimpleFileBrowser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public static class ImageAndJson
{
    public static string pngPath = "";
    public static string jsonPath = "";

    public static bool isSuccess = false;
}
[Serializable]
public class RecordFile
{
    public string name; //이름
    public string day; //날짜
    public long length; //파일 크기
    //리플레이 집합체
    public List<RecordDic> recordDic = new List<RecordDic>();
}
[Serializable]
public class RecordDic
{
    public int count; //번호
    public RecordList recordList = new RecordList();
}
[Serializable]
public class RecordList
{
    //포인트 좌표
    public List<ReplayRecord> replayRecords = new List<ReplayRecord>();
    public float thickness;
    public bool isPaint;
    public int mX;
    public int mY;
    public Color color;
}
[Serializable]
public class ReplayRecord
{
    public int num;
    public float x;
    public float y;

    public ReplayRecord(int _num, float _x, float _y)
    {
        num = _num; x = _x; y = _y;
    }
}

public class ReplayRecordList
{
    public Dictionary<int, Vector2> replayRecords = new Dictionary<int, Vector2>();
    public float thickness_r;
    public bool isPaint;
    public Vector2Int mousePos;
    public Color color;
}
public class CanvasManager : MonoBehaviour
{
    public GameObject eventSystem;
    //라인 프리팹
    public GameObject linePrefab;
    //캔버스
    public GameObject drawable_go;

    //카메라
    public Camera mainCam;
    public Camera toolCam;

    //색칠할 영역
    public RectTransform drawArea;
    public RectTransform colorPickArea;

    //컬러 피커
    public Image colorPickerImage;
    public Image colorPickShowImage;

    public Toggle toggle_replay;

    //선택한 색상
    private Color setColor;
    private Texture2D colorTexture;
    //색칠 영역 체크
    private bool isCanvasClick = false;
    private bool canClick = true;
    //선굵기
    private float thickness = 0.1f;

    //라인 오브젝트
    private GameObject line_go;
    private LineRenderer lr;
    private List<Vector3> points = new List<Vector3>();

    //Undo, Redo 기록용
    public Dictionary<int, Color32[]> textureList = new Dictionary<int, Color32[]>();
    public int textureCount = 0;
    public int replayCount = 0;

    //캔버스 컴포넌트
    Texture2D drawing_texture;

    //리플레이
    public bool isReplayMode;

    public Dictionary<int, ReplayRecordList> replayRecordDic = new Dictionary<int, ReplayRecordList>();
    public static ReplayRecordList replayRecordList = new ReplayRecordList();

    bool isSave = false;
    bool isFirst = false;

    public enum DrawMode
    {
        Hand, Brush, Eraser, Paint, Spoid
    }
    public DrawMode drawMode;

    // Start is called before the first frame update
    void Start()
    {
        colorTexture = colorPickerImage.mainTexture as Texture2D;
        Texture2D first_texture = drawable_go.GetComponent<Image>().sprite.texture;
        ResetCanvas();
        if (ImageAndJson.isSuccess)
        {
            LoadButton(false);
            LoadReplay(ImageAndJson.jsonPath);
            replayCount += replayRecordDic.Count;
        }
        textureList.Add(textureCount, first_texture.GetPixels32());
        textureCount += 1;
        replayCount += 1;
    }
    // Update is called once per frame
    void Update()
    {
        Vector2 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 toolPos = toolCam.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetKeyDown(KeyCode.Mouse0) && canClick)
        {
            //캔버스 안에 있을때
            if (RectTransformUtility.RectangleContainsScreenPoint(drawArea, mousePos))
            {
                isCanvasClick = true;
                if (drawMode == DrawMode.Brush || drawMode == DrawMode.Eraser)
                {
                    //페인트
                    line_go = Instantiate(linePrefab);
                    lr = line_go.GetComponent<LineRenderer>();

                    lr.startWidth = thickness;
                    lr.endWidth = thickness;
                    if (drawMode == DrawMode.Brush)
                    {
                        lr.startColor = setColor;
                        lr.endColor = setColor;
                    }
                    else if (drawMode == DrawMode.Eraser)
                    {
                        //지우개 흰색
                        setColor = Color.white;
                        lr.startColor = setColor;
                        lr.endColor = setColor;
                    }

                    points.Add(mousePos);
                    lr.positionCount = 1;
                    lr.SetPosition(0, points[0]);
                    replayRecordList.replayRecords.Add(0, mousePos);
                    replayRecordList.isPaint = false;
                }
                else if (drawMode == DrawMode.Paint)
                {
                    //페인트
                    drawing_texture = drawable_go.GetComponent<Image>().sprite.texture;
                    Vector2Int mouseAreaPos = GetMousePos();
                    replayRecordList.mousePos = mouseAreaPos;
                    FloodFillArea(drawing_texture, mouseAreaPos.x, mouseAreaPos.y, setColor);
                    drawing_texture.Apply();
                    replayRecordList.replayRecords.Add(0, mousePos);
                    replayRecordList.isPaint = true;
                }
                else if (drawMode == DrawMode.Spoid)
                {
                    //스포이드
                    //캔버스에 마우스위치에 있는 픽셀의 컬러를 가져옴
                    Texture2D drawTex = drawable_go.GetComponent<Image>().mainTexture as Texture2D;
                    Vector2Int mouseAreaPos = GetMousePos();
                    setColor = drawTex.GetPixel(mouseAreaPos.x, mouseAreaPos.y);
                    colorPickShowImage.color = setColor;
                }

                //기록 시작
            }
            //컬러 피커 안에 있을때
            else if (RectTransformUtility.RectangleContainsScreenPoint(colorPickArea, toolPos))
            {
                Vector2 delta;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(colorPickArea, toolPos, null, out delta);

                float width = colorPickArea.rect.width;
                float height = colorPickArea.rect.height;
                delta += new Vector2(width * 0.5f, height * 0.5f);

                float x = Mathf.Clamp(delta.x / width, 0f, 1f);
                float y = Mathf.Clamp(delta.y / height, 0f, 1f);

                int texX = Mathf.RoundToInt(x * colorTexture.width);
                int texY = Mathf.RoundToInt(y * colorTexture.height);

                setColor = colorTexture.GetPixel(texX, texY);
                colorPickShowImage.color = setColor;
            }
        }
        else if (Input.GetKey(KeyCode.Mouse0) && isCanvasClick && canClick)
        {
            if (drawMode == DrawMode.Brush || drawMode == DrawMode.Eraser)
            {
                //브러시 또는 지우개
                if (Vector2.Distance(points[points.Count - 1], mousePos) > Mathf.Epsilon)
                {
                    points.Add(mousePos);
                    lr.positionCount++;
                    lr.SetPosition(lr.positionCount - 1, mousePos);
                    replayRecordList.replayRecords.Add(lr.positionCount - 1, mousePos);
                }
            }
        }
        else if (Input.GetKeyUp(KeyCode.Mouse0) && isCanvasClick && canClick)
        {
            if (drawMode == DrawMode.Brush || drawMode == DrawMode.Eraser)
            {
                BezierPath bezierPath = new BezierPath();
                bezierPath.Interpolate(points, 0.3f);
                List<Vector3> smoothedPoints = bezierPath.GetDrawingPoints2();

                lr.positionCount = smoothedPoints.Count;
                lr.SetPositions(smoothedPoints.ToArray());

                StartCoroutine(ApplyTexture2D(false));
                points.Clear();
            }
            else if (drawMode == DrawMode.Paint)
            {
                StartCoroutine(ApplyTexture2D(false));
            }
            isSave = false;
            isCanvasClick = false;
            //기록 끝
            isFirst = true;
        }
    }

    //손, 브러시, 지우개, 페인트, 스포이드 버튼 눌렀을 때 모드 변경
    public void ClickMode(string _mode)
    {
        switch (_mode)
        {
            case "hand":
                drawMode = DrawMode.Hand;
                break;
            case "brush":
                drawMode = DrawMode.Brush;
                break;
            case "eraser":
                drawMode = DrawMode.Eraser;
                break;
            case "paint":
                drawMode = DrawMode.Paint;
                break;
            case "spoid":
                drawMode = DrawMode.Spoid;
                break;
        }
    }

    IEnumerator ApplyTexture2D(bool isReplay)
    {
        yield return new WaitForEndOfFrame();

        //새 텍스쳐2D를 생성
        int width = (int)drawArea.rect.width;
        int height = (int)drawArea.rect.height;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(360, 0, width, height), 0, 0);
        tex.Apply();

        //Color32로 분할
        Color32[] cur_color = tex.GetPixels32();

        //캔버스에 텍스쳐2D를 대입
        drawing_texture = drawable_go.GetComponent<Image>().sprite.texture;
        drawing_texture.SetPixels32(cur_color);
        drawing_texture.Apply();

        if (!isReplay)
        {
            if (textureList.ContainsKey(textureCount))
            {
                for (int i = textureCount; i < textureList.Count + 1; i++)
                {
                    textureList.Remove(i);
                }
            }
            textureList.Add(textureCount, cur_color);
            replayRecordList.color = setColor;

            if (replayRecordList.replayRecords.Count > 0)
            {
                ReplayRecordList recordList = new ReplayRecordList();
                recordList.thickness_r = thickness;
                recordList.color = setColor;
                recordList.isPaint = replayRecordList.isPaint;
                recordList.mousePos = replayRecordList.mousePos;
                for (int i = 0; i < replayRecordList.replayRecords.Count; i++)
                {
                    recordList.replayRecords.Add(i, replayRecordList.replayRecords[i]);
                }
                replayRecordDic.Add(replayCount, recordList);
            }
        }
        if (!isReplay)
        {
            textureCount += 1;
            replayCount += 1;
        }

        if (line_go != null)
            Destroy(line_go);
        replayRecordList.replayRecords.Clear();
    }
    public Vector2Int GetMousePos()
    {
        Vector2 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 delta;
        Texture2D drawTex = drawable_go.GetComponent<Image>().mainTexture as Texture2D;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(drawArea, mousePos, null, out delta);

        float width = drawArea.rect.width;
        float height = drawArea.rect.height;
        delta += new Vector2(width * 0.5f, height * 0.5f);

        float x = Mathf.Clamp(delta.x / width, 0f, 1f);
        float y = Mathf.Clamp(delta.y / height, 0f, 1f);

        int texX = Mathf.RoundToInt(x * drawTex.width);
        int texY = Mathf.RoundToInt(y * drawTex.height);

        return new Vector2Int(texX, texY);
    }
    //선 굵기 설정
    public void SetThickness(Slider slider)
    {
        thickness = slider.value;
    }
    public void UndoButton()
    {
        //뒤로
        if (textureList.Count > 1 && textureCount > 1)
        {
            drawing_texture = drawable_go.GetComponent<Image>().sprite.texture;
            drawing_texture.SetPixels32(textureList[textureCount - 2]);
            drawing_texture.Apply();
            textureCount -= 1;
        }
    }
    public void RedoButton()
    {
        //앞으로
        if (textureList.Count > textureCount && textureList.Count > 0)
        {
            drawing_texture = drawable_go.GetComponent<Image>().sprite.texture;
            drawing_texture.SetPixels32(textureList[textureCount]);
            drawing_texture.Apply();
            textureCount += 1;
        }
    }
    public void SaveButton(bool isIcon)
    {
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Images", ".png"));
        FileBrowser.SetDefaultFilter(".png");
        FileBrowser.AddQuickLink("Users", "C:\\Users", null);

        StartCoroutine(ShowSaveDialog(isIcon, false));
    }
    IEnumerator ShowSaveDialog(bool isIcon, bool isGoList)
    {
        canClick = false;
        eventSystem.SetActive(false);
        yield return FileBrowser.WaitForSaveDialog(FileBrowser.PickMode.Files, false, null, null, "그림 저장", "저장");

        if (FileBrowser.Success)
        {
            if (!File.Exists(FileBrowser.Result[0]))
            {
                Stream fileStream = File.Create(FileBrowser.Result[0]);

                fileStream.Close();
            }
            drawing_texture = drawable_go.GetComponent<Image>().sprite.texture;
            byte[] bytes = isIcon ? ScaleTexture(drawing_texture, 256, 256).EncodeToPNG() : drawing_texture.EncodeToPNG();
            File.WriteAllBytes(FileBrowser.Result[0], bytes);

            SaveRecord();
            isSave = true;
        }
        else
            isSave = false;
        canClick = true;
        eventSystem.SetActive(true);

        if (isGoList)
            SceneManager.LoadScene("List Scene");
    }
    public void LoadButton(bool _isBroswer)
    {
        if (_isBroswer)
        {
            FileBrowser.SetFilters(true, new FileBrowser.Filter("Images", ".png"));
            FileBrowser.SetDefaultFilter(".png");
            FileBrowser.AddQuickLink("FileList", "D:\\Yechan\\Unity3d\\Drawing\\DrawingProject\\Assets\\FileList", null);
            StartCoroutine(ShowLoadDialog());
        }
        else
        {
            drawing_texture = drawable_go.GetComponent<Image>().sprite.texture;
            byte[] bytes = File.ReadAllBytes(ImageAndJson.pngPath);
            drawing_texture.LoadImage(bytes);
            drawing_texture.Apply();
        }

    }
    IEnumerator ShowLoadDialog()
    {
        canClick = false;
        eventSystem.SetActive(false);
        yield return FileBrowser.WaitForSaveDialog(FileBrowser.PickMode.Files, false, null, null, "그림 불러오기", "불러오기");
        if (FileBrowser.Success)
        {
            //LoadReplay(FileBrowser.Result[0]);
            drawing_texture = drawable_go.GetComponent<Image>().sprite.texture;
            byte[] bytes = File.ReadAllBytes(FileBrowser.Result[0]);
            drawing_texture.LoadImage(bytes);
            drawing_texture.Apply();
        }
        canClick = true;
        eventSystem.SetActive(true);
    }
    public void ReturnListScene()
    {
        if (isSave || !isFirst)
            SceneManager.LoadScene("List Scene");
        else
        {
            FileBrowser.SetFilters(true, new FileBrowser.Filter("Images", ".png"));
            FileBrowser.SetDefaultFilter(".png");
            FileBrowser.AddQuickLink("Users", "C:\\Users", null);
            StartCoroutine(ShowSaveDialog(false, true));
        }
        ImageAndJson.isSuccess = false;
    }
    public struct Point
    {
        public short x;
        public short y;
        public Point(short aX, short aY) { x = aX; y = aY; }
        public Point(int aX, int aY) : this((short)aX, (short)aY) { }
    }

    public void FloodFillArea(Texture2D aTex, int aX, int aY, Color aFillColor)
    {
        int w = aTex.width;
        int h = aTex.height;
        Color[] colors = aTex.GetPixels();
        Color refCol = colors[aX + aY * w];
        Queue<Point> nodes = new Queue<Point>();
        nodes.Enqueue(new Point(aX, aY));
        while (nodes.Count > 0)
        {
            Point current = nodes.Dequeue();
            for (int i = current.x; i < w; i++)
            {
                Color C = colors[i + current.y * w];
                if (C != refCol || C == aFillColor)
                    break;
                colors[i + current.y * w] = aFillColor;
                if (current.y + 1 < h)
                {
                    C = colors[i + current.y * w + w];
                    if (C == refCol && C != aFillColor)
                        nodes.Enqueue(new Point(i, current.y + 1));
                }
                if (current.y - 1 >= 0)
                {
                    C = colors[i + current.y * w - w];
                    if (C == refCol && C != aFillColor)
                        nodes.Enqueue(new Point(i, current.y - 1));
                }
            }
            for (int i = current.x - 1; i >= 0; i--)
            {
                Color C = colors[i + current.y * w];
                if (C != refCol || C == aFillColor)
                    break;
                colors[i + current.y * w] = aFillColor;
                if (current.y + 1 < h)
                {
                    C = colors[i + current.y * w + w];
                    if (C == refCol && C != aFillColor)
                        nodes.Enqueue(new Point(i, current.y + 1));
                }
                if (current.y - 1 >= 0)
                {
                    C = colors[i + current.y * w - w];
                    if (C == refCol && C != aFillColor)
                        nodes.Enqueue(new Point(i, current.y - 1));
                }
            }
        }
        aTex.SetPixels(colors);
    }
    private Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, true);
        Color[] rpixels = result.GetPixels(0);
        float incX = (1.0f / (float)targetWidth);
        float incY = (1.0f / (float)targetHeight);
        for (int px = 0; px < rpixels.Length; px++)
        {
            rpixels[px] = source.GetPixelBilinear(incX * ((float)px % targetWidth), incY * ((float)Mathf.Floor(px / targetWidth)));
        }
        result.SetPixels(rpixels, 0);
        result.Apply();
        return result;
    }
    public void ResetCanvas()
    {
        drawing_texture = drawable_go.GetComponent<Image>().sprite.texture;
        Color[] clean_colours_array = new Color[(int)drawArea.rect.width * (int)drawArea.rect.height];
        for (int x = 0; x < clean_colours_array.Length; x++)
            clean_colours_array[x] = Color.white;

        drawing_texture.SetPixels(clean_colours_array);
        drawing_texture.Apply();
    }
    public void ChangeToggle(Toggle toggle)
    {
        isReplayMode = toggle.isOn;
        if (isReplayMode)
        {
            canClick = false;
            if (replayRecordDic.Count > 0)
                StartCoroutine(ReplayCoroutine());
        }
        else
        {
            StopReplay();
        }
    }
    private void StopReplay()
    {
        StopAllCoroutines();
        Destroy(line_go);
        replayRecordList.replayRecords.Clear();
        points.Clear();
        drawing_texture = drawable_go.GetComponent<Image>().sprite.texture;
        drawing_texture.SetPixels32(textureList[textureCount - 1]);
        drawing_texture.Apply();
        canClick = true;
    }
    IEnumerator ReplayCoroutine()
    {
        ResetCanvas();
        for (int i = 1; i <= replayRecordDic.Count; i++)
        {
            if (replayRecordDic[i].isPaint)
            {
                //페인트
                drawing_texture = drawable_go.GetComponent<Image>().sprite.texture;
                Vector2Int mouseAreaPos = replayRecordDic[i].mousePos;
                FloodFillArea(drawing_texture, mouseAreaPos.x, mouseAreaPos.y, replayRecordDic[i].color);
                drawing_texture.Apply();
                yield return new WaitForSeconds(0.01f);
            }
            else
            {
                line_go = Instantiate(linePrefab);
                lr = line_go.GetComponent<LineRenderer>();

                lr.startWidth = replayRecordDic[i].thickness_r;
                lr.endWidth = replayRecordDic[i].thickness_r;
                lr.startColor = replayRecordDic[i].color;
                lr.endColor = replayRecordDic[i].color;
                points.Add(replayRecordDic[i].replayRecords[0]);
                lr.positionCount = 1;
                lr.SetPosition(0, points[0]);
                for (int j = 1; j < replayRecordDic[i].replayRecords.Count; j++)
                {
                    points.Add(replayRecordDic[i].replayRecords[j]);
                    lr.positionCount++;
                    lr.SetPosition(lr.positionCount - 1, replayRecordDic[i].replayRecords[j]);
                    yield return new WaitForSeconds(0.01f);
                }
                BezierPath bezierPath = new BezierPath();
                bezierPath.Interpolate(points, 0.3f);
                List<Vector3> smoothedPoints = bezierPath.GetDrawingPoints2();

                lr.positionCount = smoothedPoints.Count;
                lr.SetPositions(smoothedPoints.ToArray());
            }

            StartCoroutine(ApplyTexture2D(true));
            points.Clear();
            yield return new WaitForSeconds(0.1f);
        }
        toggle_replay.isOn = false;
        StopReplay();
    }
    public void ResetReplay()
    {
        replayRecordDic.Clear();
    }
    public void LoadReplay(string path)
    {
        ResetReplay();

        string str = File.ReadAllText(path);
        var saveFile = JsonUtility.FromJson<RecordFile>(str);

        if (saveFile != null)
        {
            for (int i = 0; i < saveFile.recordDic.Count; i++)
            {
                if (saveFile.recordDic[i].count == i + 1)
                {
                    ReplayRecordList replayRecord = new ReplayRecordList();
                    for (int j = 0; j < saveFile.recordDic[i].recordList.replayRecords.Count; j++)
                    {
                        if (saveFile.recordDic[i].recordList.replayRecords[j].num == j)
                        {
                            Vector2 point = new Vector2(saveFile.recordDic[i].recordList.replayRecords[j].x, saveFile.recordDic[i].recordList.replayRecords[j].y);
                            replayRecord.replayRecords.Add(j, point);
                        }
                    }
                    replayRecord.thickness_r = saveFile.recordDic[i].recordList.thickness;
                    replayRecord.isPaint = saveFile.recordDic[i].recordList.isPaint;
                    replayRecord.mousePos = new Vector2Int(saveFile.recordDic[i].recordList.mX, saveFile.recordDic[i].recordList.mY);
                    replayRecord.color = saveFile.recordDic[i].recordList.color;

                    replayRecordDic.Add(i + 1, replayRecord);
                }
            }
        }
    }

    public void SaveRecord()
    {
        string path = FileBrowserHelpers.GetDirectoryName(FileBrowser.Result[0]);
        string fileName = path + "/" + Path.GetFileNameWithoutExtension(FileBrowser.Result[0]) + ".json";
        if (!File.Exists(fileName))
        {
            Stream fileStream = File.Create(fileName);
            fileStream.Close();
        }

        RecordFile recordFile = new RecordFile();

        for (int i = 1; i <= replayRecordDic.Count; i++)
        {
            RecordDic rDic = new RecordDic();
            rDic.count = i;
            for (int j = 0; j < replayRecordDic[i].replayRecords.Count; j++)
            {
                ReplayRecord rRecord = new ReplayRecord(j, replayRecordDic[i].replayRecords[j].x, replayRecordDic[i].replayRecords[j].y);
                rDic.recordList.replayRecords.Add(rRecord);
            }
            rDic.recordList.thickness = replayRecordDic[i].thickness_r;
            rDic.recordList.isPaint = replayRecordDic[i].isPaint;
            rDic.recordList.mX = replayRecordDic[i].mousePos.x;
            rDic.recordList.mY = replayRecordDic[i].mousePos.y;
            rDic.recordList.color = replayRecordDic[i].color;
            recordFile.recordDic.Add(rDic);
        }
        recordFile.name = Path.GetFileNameWithoutExtension(FileBrowser.Result[0]);
        recordFile.day = DateTime.Now.ToString();

        FileInfo fileInfo = new FileInfo(FileBrowser.Result[0]);
        recordFile.length = fileInfo.Length;

        File.WriteAllText(fileName, JsonUtility.ToJson(recordFile));
    }
}

