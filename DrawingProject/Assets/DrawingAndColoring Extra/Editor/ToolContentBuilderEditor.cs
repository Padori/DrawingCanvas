using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine.SceneManagement;
using IndieStudio.DrawingAndColoring.Utility;
using IndieStudio.DrawingAndColoring.Logic;

///Developed by Indie Studio
///https://assetstore.unity.com/publishers/9268
///www.indiestd.com
///info@indiestd.com

namespace IndieStudio.DrawingAndColoring.DCEditor
{
	public class ToolContentBuilderEditor : EditorWindow
	{
		private static Color color = Color.white;
		private static List<Sprite> sprites;
		private static List<string> paths;
		private static List<Transform> prefabs;
		private static List<Logic.Tool> tools;
		private static bool showInstructions = true;

		private static string[] staticPaths = new string[] {
			"Assets/DrawingAndColoring Extra/Prefabs/Crayons/",
			"Assets/DrawingAndColoring Extra/Prefabs/Pencils/",
			"Assets/DrawingAndColoring Extra/Prefabs/PaintRoller/",
			"Assets/DrawingAndColoring Extra/Prefabs/Sparkles/",
			"Assets/DrawingAndColoring Extra/Prefabs/Paints/",
			"Assets/DrawingAndColoring Extra/Prefabs/WaterBrushes/",
		};
		private Vector2 scrollPos;
		private Vector2 scale;
		private Vector2 scrollView = new Vector2(550, 430);
		private static ToolContentBuilderEditor window;

        //TODO
		//[MenuItem("Tools/Drawing And Coloring Extra/Tool Contents Factory #f", false, 0)]
		//static void ToolContentGenerator()
		//{
		//	Init();
		//}

		//[MenuItem("Tools/Drawing And Coloring Extra/Tool Contents Factory #f", true, 0)]
		//static bool ToolContentGeneratorValidate()
		//{
		//	return !Application.isPlaying && SceneManager.GetActiveScene().name == "Game";
		//}


		private static void Init()
		{
			if (sprites == null)
				sprites = new List<Sprite>();

			if (paths == null)
				paths = new List<string>();

			if (prefabs == null)
				prefabs = new List<Transform>();

			if (tools == null)
				tools = new List<IndieStudio.DrawingAndColoring.Logic.Tool>();

			window = (ToolContentBuilderEditor)EditorWindow.GetWindow(typeof(ToolContentBuilderEditor));
			float windowSize = Screen.currentResolution.height * 0.75f;
			window.position = new Rect(50, 100, windowSize, windowSize);
			window.maximized = false;
			window.titleContent.text = "Tool Content Factory";
			window.Show();
		}

		void OnGUI()
		{
			if (window == null || Application.isPlaying)
			{
				return;
			}

			window.Repaint();
			scrollView = new Vector2(position.width, position.height - 40);
			scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(scrollView.x), GUILayout.Height(scrollView.y));
			ShowInstructions();
			ShowGeneralSection();
			EditorGUILayout.EndScrollView();

		   ShowGenerateSection();
		}

		private void ShowInstructions()
		{
			EditorGUILayout.Separator();
			EditorGUILayout.Separator();

			showInstructions = EditorGUILayout.Foldout(showInstructions, "Instructions");
			EditorGUILayout.Separator();
			if (showInstructions)
			{
				EditorGUILayout.HelpBox("Set the color of the content.", MessageType.Info);
				EditorGUILayout.HelpBox("Click On 'Plus/Minus' button to define/delete a Sprite/Prefab/Path/Tool.", MessageType.Info);
				EditorGUILayout.HelpBox("Set the 'Sprite/Prefab/Path/Tool' of the content.", MessageType.Info);
				EditorGUILayout.HelpBox("Click On 'Generate' button to create the contents for the tools.", MessageType.Info);
			}
		}

		private void ShowGenerateSection()
		{
			GUI.backgroundColor = Colors.greenColor;
			if (sprites.Count > 0)
			{
				if (GUILayout.Button("Generate", GUILayout.ExpandWidth(true), GUILayout.Height(30)))
				{
					Generate();
				}
			}
			GUI.backgroundColor = Colors.whiteColor;
		}

		private void ShowGeneralSection()
		{
			EditorGUILayout.Separator();
			EditorGUILayout.Separator();

			color = EditorGUILayout.ColorField("Color", color);
			EditorGUILayout.Separator();

			EditorGUILayout.BeginHorizontal();

			GUI.backgroundColor = Colors.greenColor;
			if (GUILayout.Button("+", GUILayout.Width(30), GUILayout.Height(30)))
			{
				sprites.Add(null);
				if(paths.Count < staticPaths.Length)
				{
					paths.Add(staticPaths[paths.Count]);
				}
				else
				{
					paths.Add(string.Empty);
				}
				prefabs.Add(null);
				tools.Add(null);
			}

			GUI.backgroundColor = Colors.whiteColor;

			GUI.backgroundColor = Colors.redColor;
			if (sprites.Count > 0)
			{
				if (GUILayout.Button("-", GUILayout.Width(30), GUILayout.Height(30)))
				{
					sprites.RemoveAt(sprites.Count - 1);
					paths.RemoveAt(paths.Count - 1);
					prefabs.RemoveAt(prefabs.Count - 1);
					tools.RemoveAt(tools.Count - 1);
				}
			}

			GUI.backgroundColor = Colors.whiteColor;

			EditorGUILayout.EndHorizontal();

			/* Sprites Section */

			EditorGUILayout.BeginVertical("Box");
			EditorGUILayout.LabelField("Sprites List", EditorStyles.boldLabel);
			for (int i = 0; i < sprites.Count; i++)
			{
				sprites[i] = EditorGUILayout.ObjectField("Sprite [" + i + "]", sprites[i], typeof(Sprite), true) as Sprite;
			}
			EditorGUILayout.EndVertical();

			/* Prefabs Section */

			EditorGUILayout.BeginVertical("Box");
			EditorGUILayout.LabelField("Prefabs List", EditorStyles.boldLabel);
			for (int i = 0; i < prefabs.Count; i++)
			{
				prefabs[i] = EditorGUILayout.ObjectField("Prefab [" + i + "]", prefabs[i], typeof(Transform), true) as Transform;
			}
			EditorGUILayout.EndVertical();

			/* Paths Section */

			EditorGUILayout.BeginVertical("Box");
			EditorGUILayout.LabelField("Paths List", EditorStyles.boldLabel);
			for (int i = 0; i < paths.Count; i++)
			{
				paths[i] = EditorGUILayout.TextField("Path [" + i + "]", paths[i]);
			}
			EditorGUILayout.EndVertical();

			/* Tools Section */

			EditorGUILayout.BeginVertical("Box");
			EditorGUILayout.LabelField("Tools List", EditorStyles.boldLabel);
			for (int i = 0; i < tools.Count; i++)
			{
				tools[i] = EditorGUILayout.ObjectField("Tool [" + i + "]", tools[i], typeof(Logic.Tool), true) as Logic.Tool;
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.Separator();

		}

		private void Generate()
		{
			GradientAlphaKey []alphaKey;
			GradientColorKey[] colorKey;
			Gradient gradient = null;
			GameObject gob = null;

			for (int i = 0; i < sprites.Count; i++)
			{
				if (prefabs[i] == null || sprites[i] == null || tools[i] == null)
				{
					Debug.LogError("Null reference found at index "+i);
					continue;
				}

				 gob = prefabs[i].gameObject;
				 gradient = new Gradient();

				colorKey = new GradientColorKey[2];
				colorKey[0].color = color;
				colorKey[0].time = 0.0f;
				colorKey[1].color = color;
				colorKey[1].time = 1.0f;

				alphaKey = new GradientAlphaKey[2];
				alphaKey[0].alpha = 1.0f;
				alphaKey[0].time = 0.0f;
				alphaKey[1].alpha = 1.0f;
				alphaKey[1].time = 1.0f;

				gradient.SetKeys(colorKey, alphaKey);

				gob.GetComponent<ToolContent>().gradientColor = gradient;
				gob.GetComponent<Image>().sprite = sprites[i];

				GameObject savedPrefab = CommonUtil.SaveAsPrefab(paths[i] + sprites[i].name + ".prefab", gob, false);

				tools[i].contents.Add(savedPrefab.transform);
			}
			
			 DirtyUtil.MarkSceneDirty();
		}

		void OnInspectorUpdate()
		{
			Repaint();
		}

	}
}