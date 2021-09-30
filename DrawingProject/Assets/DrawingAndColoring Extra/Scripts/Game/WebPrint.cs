using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
#if (UNITY_WEBPLAYER || UNITY_WEBGL)
using System.Runtime.InteropServices;
#endif

///Developed by Indie Studio
///https://assetstore.unity.com/publishers/9268
///www.indiestd.com
///info@indiestd.com

namespace IndieStudio.DrawingAndColoring.Logic
{
	public class WebPrint : MonoBehaviour
	{
		/// <summary>
		/// Whether process is running or not.
		/// </summary>
		public static bool isRunning;

        /// <summary>
        /// App Name in the Print Window
        /// </summary>
        public string appName = "Drawing And Coloring Extra";

        /// <summary>
        /// Exported image file prefix
        /// </summary>
        public string imageNamePrefix = "DrawingAndColoring";

		/// <summary>
		/// The flash sound effect.
		/// </summary>
		public AudioClip flashSFX;

        /// <summary>
        /// The flash effect fade.
        /// </summary>
        public Animator flashEffect;

        /// <summary>
        /// The objects bet hide/show on screen capturing.
        /// </summary>
        public Transform[] objects;

		/// <summary>
		/// The logo on the bottom of the page.
		/// </summary>
		public Transform bottomLogo;


        #if (UNITY_WEBPLAYER || UNITY_WEBGL)

            [DllImport("__Internal")]
            private static extern void ExportImage(string appName,string base64, string imageName);
        #endif


        void Start(){
			isRunning = false;
		}

		/// <summary>
		/// Print the screen.
		/// </summary>
		public void PrintScreen ()
		{
            #if !(UNITY_WEBPLAYER || UNITY_WEBGL) || UNITY_EDITOR
                Debug.LogError("Print feature works only in the online web application, \nyou need to export your web application and deploy it online to your server and test");
            #endif

            #if (UNITY_WEBPLAYER || UNITY_WEBGL || UNITY_EDITOR)
                StartCoroutine("PrintScreenCoroutine");
		    #endif
		}

		public IEnumerator PrintScreenCoroutine ()
		{
			isRunning = true;

			HideObjects ();
			if(bottomLogo!=null)
				bottomLogo.gameObject.SetActive (true);

            string imageName = string.Format("{0}-{1}", imageNamePrefix, System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
                
			//Capture screen shot
			yield return new WaitForEndOfFrame();
			Texture2D texture = new Texture2D(Screen.width, Screen.height);
			texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
			texture.Apply();

			flashEffect.SetTrigger ("Run");
			if(flashSFX !=null && AudioSources.instance!=null)
				AudioSources.instance.SFXAudioSource().PlayOneShot (flashSFX);// play flash sfx
			yield return new WaitForSeconds (1);
			ShowObjects ();
			if(bottomLogo!=null)
				bottomLogo.gameObject.SetActive (false);

            string strBase64 = System.Convert.ToBase64String(texture.EncodeToPNG());

            try
            {
                #if UNITY_WEBPLAYER || UNITY_WEBGL
                    ExportImage(appName, strBase64, imageName);
                #endif
            }
            catch(System.Exception ex)
            {
                Debug.LogError(ex.ToString());
            }
			isRunning = false;

		}

		/// <summary>
		/// Hide the objects.
		/// </summary>
		private void HideObjects ()
		{
			if (objects == null) {
				return;
			}

			foreach (Transform obj in objects) {
				if(obj!=null)
					obj.gameObject.SetActive (false);
			}
		}

		/// <summary>
		/// Show the objects.
		/// </summary>
		private void ShowObjects ()
		{
			if (objects == null) {
				return;
			}
			
			foreach (Transform obj in objects) {
                if (obj != null)
                    obj.gameObject.SetActive (true);
			}
		}
	}
}