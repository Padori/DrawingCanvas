using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawColorPick : MonoBehaviour
{
    public Color resultColor;

    private RectTransform Rect;
    private Texture2D colorTexture;

    // Start is called before the first frame update
    void Start()
    {
        Rect = GetComponent<RectTransform>();
        colorTexture = GetComponent<Image>().mainTexture as Texture2D;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (RectTransformUtility.RectangleContainsScreenPoint(Rect, mousePos))
        {
            Vector2 delta;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(Rect, mousePos, null, out delta);

            float width = Rect.rect.width;
            float height = Rect.rect.height;
            delta += new Vector2(width * 0.5f, height * 0.5f);

            float x = Mathf.Clamp(delta.x / width, 0f, 1f);
            float y = Mathf.Clamp(delta.y / height, 0f, 1f);

            int texX = Mathf.RoundToInt(x * colorTexture.width);
            int texY = Mathf.RoundToInt(y * colorTexture.height);

            if (Input.GetMouseButtonDown(0))
            {
                resultColor = colorTexture.GetPixel(texX, texY);
            }
        }
    }
}
