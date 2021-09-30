using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///Developed by Indie Studio
///https://assetstore.unity.com/publishers/9268
///www.indiestd.com
///info@indiestd.com

namespace IndieStudio.DrawingAndColoring.Logic
{
	public class WorldSpaceRender : MonoBehaviour {

		// Use this for initialization
		void Start () {
			RectTransform rectTransform = GetComponent<RectTransform> ();
			//Copy ScreenSpaceOverlay values
			Vector3 anchoredPos = rectTransform.anchoredPosition3D;
			Vector3 size = rectTransform.sizeDelta;
			Vector3 max = rectTransform.anchorMax;
			Vector3 min = rectTransform.anchorMin;
			Vector3 piviot = rectTransform.pivot;
			Vector3 scale = rectTransform.localScale;

			//Change Render Mode to WorldSpace
			GetComponent<Canvas> ().renderMode = RenderMode.WorldSpace;	

			//Paste ScreenSpaceOverlay old values
			rectTransform.anchoredPosition3D = anchoredPos;
			rectTransform.sizeDelta = size;
			rectTransform.anchorMin = min;
			rectTransform.anchorMax = max;
			rectTransform.pivot = piviot;
			rectTransform.localScale = scale;
		}
	}
}