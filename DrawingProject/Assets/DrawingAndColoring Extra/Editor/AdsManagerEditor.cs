using UnityEngine;
using System.Collections;
using UnityEditor;
using IndieStudio.DrawingAndColoring.Utility;

///Developed by Indie Studio
///https://assetstore.unity.com/publishers/9268
///www.indiestd.com
///info@indiestd.com

namespace IndieStudio.DrawingAndColoring.DCEditor
{
	[CustomEditor (typeof(AdsManager))]
	public class AdsManagerEditor :  Editor
	{
		public int selectedPackage;
		private static bool showInstructions = true;
		private static string[] Packages = null;
		private static string[] downloadURlS = new string[] {
			"https://github.com/googleads/googleads-mobile-unity/releases",
			"http://www.chartboo.st/sdk/unity",
			"https://assetstore.unity.com/packages/add-ons/services/unity-ads-66123"
		};
		private static string[] moreDetails = new string[] {
			"https://firebase.google.com/docs/admob/unity/start",
			"https://answers.chartboost.com/en-us/child_article/unity",
			"https://unity3d.com/services/ads/quick-start-guide"
		};

		private static string[] labels = new string[]{
			"GOOGLE MOBILE ADS",
			"CHARTBOOST",
			"UNITY ADS"
		};

		private Undefined undefined;
		public enum Undefined
		{
			UNDEFINED
		}

		public override void OnInspectorGUI ()
		{
			if (Application.isPlaying) {
				return;
			}

			#if !(UNITY_ANDROID || UNITY_IPHONE)
				EditorGUILayout.Separator ();
				EditorGUILayout.HelpBox ("You need to switch to mobile platform (Android or IOS) to setup Ads", MessageType.Warning);
				return;
			#endif

			AdsManager attrib = (AdsManager)target;//get the target
			if (Packages == null) {
				selectedPackage = 0;
				System.Array packagesEnum = System.Enum.GetValues (typeof(AdPackage.Package));
				if (packagesEnum.Length == 0) {
					return;
				}

				Packages = new string[packagesEnum.Length];
				for (int i = 0; i < packagesEnum.Length; i++) {
					Packages [i] = packagesEnum.GetValue (i).ToString ();
				}
			}

			EditorGUILayout.Separator();
			#if !(UNITY_5 || UNITY_2017 || UNITY_2018_0 || UNITY_2018_1 || UNITY_2018_2)
				//Unity 2018.3 or higher
				EditorGUILayout.BeginHorizontal();
				GUI.backgroundColor = Colors.cyanColor;
				EditorGUILayout.Separator();
				if(PrefabUtility.GetPrefabParent(attrib.gameObject)!=null)
				if (GUILayout.Button("Apply", GUILayout.Width(70), GUILayout.Height(30), GUILayout.ExpandWidth(false)))
				{
					PrefabUtility.ApplyPrefabInstance(attrib.gameObject, InteractionMode.AutomatedAction);
				}
				GUI.backgroundColor = Colors.whiteColor;
				EditorGUILayout.EndHorizontal();
			#endif
			EditorGUILayout.Separator ();
			selectedPackage = GUILayout.Toolbar (selectedPackage, Packages);
			EditorGUILayout.Separator ();

			for (int i = 0; i < attrib.adPackages.Count; i++) {
				if (selectedPackage != i) {
					continue;
				}
									
				ShowInstruction (attrib.adPackages [i].package);

				EditorGUILayout.Separator ();
				EditorGUILayout.BeginHorizontal ();

				GUI.backgroundColor = Colors.greenColor;
				if (GUILayout.Button ("Download " + labels [selectedPackage], GUILayout.Width (200), GUILayout.Height (20))) {
					Application.OpenURL (downloadURlS [i]);
				}
				GUI.backgroundColor = Colors.whiteColor;

				GUI.backgroundColor = Colors.cyanColor;
				if (GUILayout.Button ("More Details About " + Packages [selectedPackage], GUILayout.Width (220), GUILayout.Height (20))) {
					Application.OpenURL (moreDetails [i]);
				}
				GUI.backgroundColor = Colors.whiteColor;
				EditorGUILayout.EndHorizontal ();

				EditorGUILayout.Separator ();
				attrib.adPackages [i].isEnabled = EditorGUILayout.Toggle ("Enable " + Packages [selectedPackage] + " ADS", attrib.adPackages [i].isEnabled);
				EditorGUILayout.Separator ();

				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("Event");
				GUILayout.Space (40);
				GUILayout.Label ("Ad Type");
				GUILayout.Space (20);
				GUILayout.Label ("Ad Position");
				GUILayout.Label ("Active");
					
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.Separator ();
				foreach (AdPackage.AdEvent adEvent in attrib.adPackages[i].adEvents) {
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.EnumPopup (adEvent.evt);
					EditorGUI.BeginDisabledGroup (attrib.adPackages [i].package == AdPackage.Package.UNITY);
					adEvent.type = (AdPackage.AdEvent.Type)EditorGUILayout.EnumPopup (adEvent.type);
					if (adEvent.type == AdPackage.AdEvent.Type.BANNER && attrib.adPackages [i].package == AdPackage.Package.CHARTBOOST) {
						adEvent.type = AdPackage.AdEvent.Type.INTERSTITIAL;
					}
					EditorGUI.EndDisabledGroup ();

					EditorGUI.BeginDisabledGroup (adEvent.type != AdPackage.AdEvent.Type.BANNER || attrib.adPackages [i].package == AdPackage.Package.CHARTBOOST || attrib.adPackages [i].package == AdPackage.Package.UNITY);
					#if GOOGLE_MOBILE_ADS
					adEvent.adPostion = (GoogleMobileAds.Api.AdPosition)EditorGUILayout.EnumPopup (adEvent.adPostion);
					#else
					EditorGUILayout.EnumPopup(undefined);
					#endif
					EditorGUI.EndDisabledGroup ();
						
					adEvent.isEnabled = EditorGUILayout.Toggle (adEvent.isEnabled);
					EditorGUILayout.EndHorizontal ();
				}
			}
			EditorGUILayout.Separator ();
		

			if (GUI.changed)
			{
				DirtyUtil.MarkSceneDirty();
			}
		}

				
		private void ShowInstruction (AdPackage.Package package)
		{
			EditorGUILayout.Separator ();
			EditorGUILayout.HelpBox ("Follow the instructions below on how to enable the " + Packages [selectedPackage] + " Advertisements", MessageType.Info);
			EditorGUILayout.Separator ();
			showInstructions = EditorGUILayout.Foldout (showInstructions, "Instructions");
			EditorGUILayout.Separator ();

			if (!showInstructions) {
				return;
			}

			EditorGUILayout.BeginHorizontal ();
			ShowReadManual ();
			ShowContactUS ();
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.Separator ();

			if (package == AdPackage.Package.ADMOB) {
				ShowAdmobInstructions ();
			} else if (package == AdPackage.Package.CHARTBOOST) {
				ShowChartBoostInstructions ();
			} else if (package == AdPackage.Package.UNITY) {
				ShowUnityAdsInstructions ();
			}
			ShowCommonInstructions ();
						
			EditorGUILayout.Separator ();
		}

		private void ShowReadManual ()
		{
			GUI.backgroundColor = Colors.yellowColor;
			if (GUILayout.Button ("Read the Manual", GUILayout.Width (120), GUILayout.Height (20))) {
				Application.OpenURL (Links.docPath);
			}
			GUI.backgroundColor = Colors.whiteColor;
		}

		private void ShowContactUS ()
		{
			GUI.backgroundColor = Colors.greenColor;
			if (GUILayout.Button ("Contact US", GUILayout.Width (100), GUILayout.Height (20))) {
				Application.OpenURL (Links.indieStudioContactUsURL);
			}
			GUI.backgroundColor = Colors.whiteColor;
		}

		private void ShowCommonInstructions ()
		{
			EditorGUILayout.Separator ();
			EditorGUILayout.HelpBox ("* If you any questions , suggestions or problems you can contact us", MessageType.None);
			EditorGUILayout.Separator ();
		}

		private void ShowAdmobInstructions ()
		{
			EditorGUILayout.HelpBox ("1. Make sure you have JDK, Android SDK installed on your PC and linked in your Unity Editor from (Edit > Preferences > External Tools), as well as install/update the Extras , Google Play Services from the Android SDK Manager in Android Studio", MessageType.None);

			EditorGUILayout.BeginHorizontal();

			if (GUILayout.Button("Download JDK", GUILayout.Width(160), GUILayout.Height(20)))
			{
				Application.OpenURL("https://www.oracle.com/technetwork/java/javase/downloads/jdk8-downloads-2133151.html");
			}

			if (GUILayout.Button("Download Android Studio", GUILayout.Width(160), GUILayout.Height(20)))
			{
				Application.OpenURL("https://developer.android.com/studio");
			}

			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Separator();

			EditorGUILayout.HelpBox ("2. Download Admob package, and then import it to your unity project", MessageType.None);
			EditorGUILayout.HelpBox ("3. Select\n\nAssets > External Dependency Manager > Android Resolver > Resolve/Force Resolve for Android Platform\n" +
				"Assets > External Dependency Manager > iOS Resolver > Install Cocoapods for IOS Platform", MessageType.None);
			EditorGUILayout.HelpBox ("4. Select Assets > Google Mobile Ads > Settings , then Enable AdMob\n\n"+
			"- Leaving AdMob app ID empty results in a crash with the message: 'The Google. Mobile Ads SDK was initialized incorrectly'\n" +
			"- If your app uses Ad Manager instead of AdMob, then enable Google Ad Manager" +
			". You should make this change immediately\n\n" +
			"- Press 'ctrl/cmd + s' to save your changes", MessageType.None);
			EditorGUILayout.HelpBox ("5. Insert your App Id, Admob UnitID in the AdMob component below", MessageType.None);
			EditorGUILayout.HelpBox("6. To test the Ads on your mobile device (Test Mode) : \n\n"+
				"- Enable Test Mode flag in the Admob component\n"+
				"- Get Sample Ad Units from the link below\n" +
				"- Insert your Sample Ad Units in the Admob component, then click on Apply button\n" +
				"- Build and run your application on your mobile device\n" +
				"- Make an Advertisement request (Like set an Ad event as active when load your scene(s) in Admob component)\n" +
				"- Check the logcat output e.g. from 'Android Studio Logcat layout' for a message which shows your device ID\n"+
				"- Insert your Test Device ID in the Admob component, then click on Apply button\n" +
				"- Create new unity build and run your app. If the AD is a Google ad, you'll see a Test Ad label centered at the top of the AD\n\n" +
				"- Note: To see the Test Ad label, you need to be using an SDK version of 11.6.0 or later\n" +
				"- Note: Mediated ads do not render a Test AD label", MessageType.None);

			if (GUILayout.Button("Get Test Ad Units & Find Your Device ID", GUILayout.Width(250), GUILayout.Height(20)))
			{
				Application.OpenURL("https://developers.google.com/admob/android/test-ads");
			}
			EditorGUILayout.Separator();

			EditorGUILayout.HelpBox("7. Modify the attributes below as you wish", MessageType.None);
			EditorGUILayout.HelpBox ("8. Click on Apply button that located on the top to save your changes", MessageType.None);
			EditorGUILayout.HelpBox("9. !important - Once you want to release or publish your app, you need to disable the Test Mode and insert your original app id / ad units for production", MessageType.None);

			#if UNITY_IPHONE || UNITY_IOS
					EditorGUILayout.Separator();
					EditorGUILayout.LabelField("For IOS Platform Only :", EditorStyles.boldLabel);
					EditorGUILayout.Separator();
					EditorGUILayout.HelpBox("You need to download and then add Google Mobile Ads framework to your xCode project", MessageType.Info);
					if (GUILayout.Button("Get Xcode Google Mobile Ads SDK", GUILayout.Width(250), GUILayout.Height(20)))
					{
						Application.OpenURL("https://developers.google.com/ad-manager/mobile-ads-sdk/ios/quick-start#import_the_mobile_ads_sdk");
					}
					EditorGUILayout.Separator();
			#endif

		}

		private void ShowChartBoostInstructions ()
		{
			EditorGUILayout.HelpBox ("1. Download ChartBoost package, and then import it to your unity project", MessageType.None);
			EditorGUILayout.HelpBox ("2. Select ChartBoost->Edit Settings, then insert your ChartBoost AppID & App Signiture", MessageType.None);
			EditorGUILayout.HelpBox("3. Select ChartBoost->Edit Settings, then click on Setup Android SDK to make Chartboost configured sucesfully", MessageType.None);
			if (GUILayout.Button("Learn How To Enable Chartboost Test Mode", GUILayout.Width(260), GUILayout.Height(20)))
			{
				Application.OpenURL("https://answers.chartboost.com/en-us/articles/200780549");
			}
			EditorGUILayout.Separator();
			EditorGUILayout.HelpBox ("4. Modify the attributes below as you wish", MessageType.None);
			EditorGUILayout.HelpBox ("5. Click on Apply button that located on the top to save your changes", MessageType.None);
			EditorGUILayout.HelpBox("6. !important - Once you want to release or publish your app, you need to disable the Test Mode", MessageType.None);

		}

		private void ShowUnityAdsInstructions ()
		{
			EditorGUILayout.HelpBox ("1. Download Unity Ads package from the Unity Asset Store, and then import it to your unity project", MessageType.None);
			EditorGUILayout.HelpBox ("2. Insert your Unity Game IDs in the Unity Ad component below", MessageType.None);
			EditorGUILayout.HelpBox ("3. Enable 'Test Mode' in the Unity Ad component  to test the Unity Ads", MessageType.None);
			EditorGUILayout.HelpBox ("4. Modify the attributes below as you wish", MessageType.None);
			EditorGUILayout.HelpBox ("5. Click on Apply button that located on the top to save your changes", MessageType.None);
			EditorGUILayout.HelpBox("6. !important - Once you want to release or publish your app, you need to disable the Test Mode", MessageType.None);

		}		
	
	}
}