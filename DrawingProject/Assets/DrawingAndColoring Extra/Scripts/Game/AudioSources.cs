using UnityEngine;
using System.Collections;

///Developed by Indie Studio
///https://assetstore.unity.com/publishers/9268
///www.indiestd.com
///info@indiestd.com

namespace IndieStudio.DrawingAndColoring.Logic
{
	[DisallowMultipleComponent]
	public class AudioSources : MonoBehaviour
	{
		/// <summary>
		/// The audio sources references.
		/// First Audio Souce used for the music
		/// Second Audio Souce used for the sound effects
		/// </summary>
		private AudioSource[] audioSources;

		/// <summary>
		/// The loading canvas instance.
		/// </summary>
		public static AudioSources instance;
	
		// Use this for initialization
		void Awake ()
		{
			if (instance == null) {
				instance = this;
				audioSources = GetComponents<AudioSource>();

				DontDestroyOnLoad(gameObject);
			} else {
				Destroy (gameObject);
			}
		}

		/// <summary>
		/// Returns the Audio Source of the Music.
		/// </summary>
		/// <returns>The Audio Source of the Music.</returns>
		public AudioSource MusicAudioSource()
		{
			return audioSources[0];
		}


		/// <summary>
		/// Returns the Audio Source of the Sound Effects.
		/// </summary>
		/// <returns>The Audio Source of the Sound Effects.</returns>
		public AudioSource SFXAudioSource()
		{
			return audioSources[1];
		}
	}
}
