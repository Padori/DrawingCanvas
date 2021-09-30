using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Events;

[Serializable]
public class ColorEvent : UnityEvent<Color> { }
public class DrawColor : MonoBehaviour
{
    public TextMeshProUGUI DebugText;
    public ColorEvent OnColorPreview;
    public ColorEvent OnColorSelect;

    RectTransform Rect;
    Texture2D ColorTexture;

    private void Start()
    {
        Rect = GetComponent<RectTransform>();

        ColorTexture = GetComponent<Image>().mainTexture as Texture2D;
    }
    private void Update()
    {
        if (RectTransformUtility.RectangleContainsScreenPoint(Rect, Input.mousePosition))
        {
            Vector2 delta;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(Rect, Input.mousePosition, null, out delta);

            //마우스 위치
            string debug = "mousePosition = " + Input.mousePosition;
            //이미지 중앙 기준
            debug += "<br>delta = " + delta;

            float width = Rect.rect.width;
            float height = Rect.rect.height;
            delta += new Vector2(width * 0.5f, height * 0.5f);

            //텍스쳐 왼쪽 아래 기준
            debug += "<br>offset delta = " + delta;

            float x = Mathf.Clamp(delta.x / width, 0f, 1f);
            float y = Mathf.Clamp(delta.y / height, 0f, 1f);
            //0~1 사이 기준
            debug += "<br>x = " + x + " y = " + y;

            int texX = Mathf.RoundToInt(x * ColorTexture.width);
            int texY = Mathf.RoundToInt(y * ColorTexture.height);
            debug += "<br>texX = " + texX + " texY = " + texY;

            Color color = ColorTexture.GetPixel(texX, texY);

            DebugText.color = color;
            DebugText.text = debug;

            OnColorPreview?.Invoke(color);

            if (Input.GetMouseButtonDown(0))
            {
                OnColorSelect?.Invoke(color);
            }
        }
    }
}
