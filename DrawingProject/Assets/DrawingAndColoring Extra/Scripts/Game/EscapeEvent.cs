using UnityEngine;
using System.Collections;

///Developed by Indie Studio
///https://assetstore.unity.com/publishers/9268
///www.indiestd.com
///info@indiestd.com

namespace IndieStudio.DrawingAndColoring.Logic
{
	using UnityEngine;
	using System.Collections;
	using UnityEngine.Events;

	/// <summary>
	/// Escape or Back event
	/// </summary>
	[DisallowMultipleComponent]
	public class EscapeEvent : MonoBehaviour
	{
		/// <summary>
		/// On escape/back event
		/// </summary>
		public UnityEvent escapeEvent;

		void Update ()
		{
			if (Input.GetKeyDown (KeyCode.Escape)) {
				OnEscapeClick ();
			}
		}

		/// <summary>
		/// On Escape click event.
		/// </summary>
		public void OnEscapeClick ()
		{
			escapeEvent.Invoke ();
		}
	}
}