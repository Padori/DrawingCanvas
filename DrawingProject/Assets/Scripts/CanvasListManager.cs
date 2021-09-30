using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CanvasListManager : MonoBehaviour
{
    public GameObject holder;
    public GameObject content;

    public Image thumbnailImage;

    public List<RecordFile> backupFiles = new List<RecordFile>();

    private void Start()
    {
        string path = Application.dataPath + "/FileList/";
        int count = 0;

        if (Directory.Exists(path))
        {
            DirectoryInfo di = new DirectoryInfo(path);

            foreach (var item in di.GetFiles())
            {
                if (Path.GetExtension(item.Name) == ".json")
                {
                    string str = File.ReadAllText(item.FullName);
                    var saveFile = JsonUtility.FromJson<RecordFile>(str);
                    if (saveFile != null)
                    {
                        RecordFile recordFile = new RecordFile();
                        recordFile.name = saveFile.name;
                        recordFile.day = saveFile.day;
                        recordFile.length = saveFile.length;

                        for (int i = 0; i < saveFile.recordDic.Count; i++)
                        {
                            if (saveFile.recordDic[i].count == i + 1)
                            {
                                RecordDic replayRecord = new RecordDic();
                                replayRecord.count = i + 1;
                                for (int j = 0; j < saveFile.recordDic[i].recordList.replayRecords.Count; j++)
                                {
                                    if (saveFile.recordDic[i].recordList.replayRecords[j].num == j)
                                    {
                                        ReplayRecord rRecord = new ReplayRecord(j, saveFile.recordDic[i].recordList.replayRecords[j].x, saveFile.recordDic[i].recordList.replayRecords[j].y);
                                        replayRecord.recordList.replayRecords.Add(rRecord);
                                    }
                                }
                                replayRecord.recordList.thickness = saveFile.recordDic[i].recordList.thickness;
                                replayRecord.recordList.isPaint = saveFile.recordDic[i].recordList.isPaint;
                                replayRecord.recordList.mX = saveFile.recordDic[i].recordList.mX;
                                replayRecord.recordList.mX = saveFile.recordDic[i].recordList.mY;
                                replayRecord.recordList.color = saveFile.recordDic[i].recordList.color;

                                recordFile.recordDic.Add(replayRecord);
                            }
                        }

                        backupFiles.Add(recordFile);

                        GameObject fileContent = Instantiate(content, Vector3.zero, Quaternion.identity);
                        fileContent.name = saveFile.name;
                        fileContent.transform.SetParent(holder.transform);
                        fileContent.transform.localScale = Vector3.one;
                        fileContent.GetComponentInChildren<Text>().text = saveFile.name + "     " + saveFile.day + "     " + GetFileSize(saveFile.length);
                        fileContent.GetComponent<Button>().onClick.AddListener(() => ContentButton(fileContent.GetComponent<Button>()));
                        count++;
                    }
                }
            }
        }
    }

    private string GetFileSize(double byteCount)
    {
        string size = "0 Bytes";
        if (byteCount >= 1048576)
            size = string.Format("{0:##.##}", byteCount / 1048576.0) + "MB";
        else if (byteCount >= 1024)
            size = string.Format("{0:##.##}", byteCount / 1024.0) + "KB";
        else if (byteCount > 0 && byteCount < 1024)
            size = byteCount.ToString() + "Bytes";

        return size;
    }
    public void ContentButton(Button button)
    {
        string path = Application.dataPath + "/FileList/";
        foreach (RecordFile file in backupFiles)
        {
            if (file.name == button.name)
            {
                if (Directory.Exists(path))
                {
                    DirectoryInfo di = new DirectoryInfo(path);

                    foreach (var item in di.GetFiles())
                    {
                        if (Path.GetFileNameWithoutExtension(item.Name) == file.name)
                        {
                            //JSON 리플레이
                            if (Path.GetExtension(item.Name) == ".json")
                                ImageAndJson.jsonPath = item.FullName;
                            else if (Path.GetExtension(item.Name) == ".png")
                                ImageAndJson.pngPath = item.FullName;
                        }
                    }
                }
            }
        }
        if (ImageAndJson.pngPath != "")
            ThumbnailImage(ImageAndJson.pngPath);
    }
    public void StartButton(bool _isLoad)
    {
        if (_isLoad)
        {
            if (ImageAndJson.jsonPath != "" && ImageAndJson.pngPath != "")
                ImageAndJson.isSuccess = true;
            else
                ImageAndJson.isSuccess = false;
        }
        else
            ImageAndJson.isSuccess = false;
        SceneManager.LoadScene("DrawCanvas");
    }
    public void ThumbnailImage(string path)
    {
        Texture2D tex = thumbnailImage.sprite.texture;
        byte[] bytes = File.ReadAllBytes(path);
        tex.LoadImage(bytes);
        tex.Apply();
    }
}
