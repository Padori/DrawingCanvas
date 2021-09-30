using UnityEngine;
using System.Collections;
using System.Collections.Generic;

///Developed by Indie Studio
///https://assetstore.unity.com/publishers/9268
///www.indiestd.com
///info@indiestd.com

namespace IndieStudio.DrawingAndColoring.Logic
{
	[DisallowMultipleComponent]
	public class Area : MonoBehaviour
	{

		/// <summary>
		/// The shapes drawing contents.
		/// </summary>
		public static List<DrawingContents> shapesDrawingContents = new List<DrawingContents> ();
	}
}
