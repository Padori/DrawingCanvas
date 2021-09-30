using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Texture2DTest : MonoBehaviour
{
    public Texture2D tex;

    private void Start()
    {
        Texture2D texCopy = new Texture2D(tex.width, tex.height, tex.format, tex.mipmapCount > 1);
        texCopy.LoadRawTextureData(tex.GetRawTextureData());
        texCopy.Apply();
        GetComponent<Renderer>().material.mainTexture = texCopy;
    }
}
