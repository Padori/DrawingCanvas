using UnityEngine;
using UnityEditor;
using System.Collections;
using IndieStudio.DrawingAndColoring.Utility;

///Developed by Indie Studio
///https://assetstore.unity.com/publishers/9268
///www.indiestd.com
///info@indiestd.com

namespace IndieStudio.DrawingAndColoring.DCEditor
{
	[CustomEditor(typeof(IndieStudio.DrawingAndColoring.Logic.Tool))]
	public class ToolEditor : Editor
	{
		public override void OnInspectorGUI ()
		{
			if (Application.isPlaying) {
				return;
			}

			IndieStudio.DrawingAndColoring.Logic.Tool tool = (IndieStudio.DrawingAndColoring.Logic.Tool)target;//get the target

			EditorGUILayout.Separator ();
			#if !(UNITY_5 || UNITY_2017 || UNITY_2018_0 || UNITY_2018_1 || UNITY_2018_2)
				//Unity 2018.3 or higher
				EditorGUILayout.BeginHorizontal();
				GUI.backgroundColor = Colors.cyanColor;
				EditorGUILayout.Separator();
				if(PrefabUtility.GetPrefabParent(tool.gameObject)!=null)
				if (GUILayout.Button("Apply", GUILayout.Width(70), GUILayout.Height(30), GUILayout.ExpandWidth(false)))
				{
					PrefabUtility.ApplyPrefabInstance(tool.gameObject, InteractionMode.AutomatedAction);
				}
				GUI.backgroundColor = Colors.whiteColor;
				EditorGUILayout.EndHorizontal();
			#endif
			EditorGUILayout.Separator ();
			
			EditorGUILayout.HelpBox ("The tool GameObject must be breakable from Prefab instance", MessageType.Info);
			EditorGUILayout.Separator ();

			//tool.selectedContentIndex = EditorGUILayout.IntField ("Selected Content's Index", tool.selectedContentIndex);
			//EditorGUILayout.Separator ();
			tool.feature = (IndieStudio.DrawingAndColoring.Logic.Tool.ToolFeature)EditorGUILayout.EnumPopup ("Feature", tool.feature);

			if (tool.feature == IndieStudio.DrawingAndColoring.Logic.Tool.ToolFeature.Line) {
				if(!tool.repeatedTexture)
					tool.drawMaterial = EditorGUILayout.ObjectField ("Line Material", tool.drawMaterial, typeof(Material), true) as Material;
				tool.lineThicknessFactor = EditorGUILayout.Slider ("Line Thickness Factor", tool.lineThicknessFactor, 0.1f, 10);
				tool.lineTextureMode = (LineTextureMode)EditorGUILayout.EnumPopup ("Line Texture Mode", tool.lineTextureMode);
				tool.createPaintLines = EditorGUILayout.Toggle ("Create Paint Lines", tool.createPaintLines);
				tool.roundedEdges = EditorGUILayout.Toggle ("Rounded Edges", tool.roundedEdges);
			}

			tool.useAsToolContent = EditorGUILayout.Toggle ("Use As Content", tool.useAsToolContent);

			tool.useAsCursor = EditorGUILayout.Toggle ("Use As Cursor", tool.useAsCursor);

			tool.enableContentsShadow = EditorGUILayout.Toggle ("Enable Contents Shadow", tool.enableContentsShadow);

			tool.repeatedTexture = EditorGUILayout.Toggle ("Repeated Texture", tool.repeatedTexture);

			tool.sliderContentsCellSize = EditorGUILayout.Vector2Field ("Slider Contents Cell Size", tool.sliderContentsCellSize);

			tool.sliderContentsSpacing = EditorGUILayout.Vector2Field ("Slider Contents Spacing", tool.sliderContentsSpacing);

			tool.contentRotation = EditorGUILayout.Slider ("Content Rotation", tool.contentRotation, 0, 360);

			tool.cursorRotation = EditorGUILayout.Slider ("Cursor Rotation", tool.cursorRotation, 0, 360);

			if (tool.feature != IndieStudio.DrawingAndColoring.Logic.Tool.ToolFeature.Hand) {
			
				//tool.showContents = EditorGUILayout.Foldout (tool.showContents, "Contents");

				//if (tool.showContents) {
					GUILayout.BeginHorizontal ();
					GUI.backgroundColor = Colors.greenColor;         

					if (GUILayout.Button ("Add New Content", GUILayout.Width (120), GUILayout.Height (20))) {
						tool.contents.Add (null);
					}

                    GUI.backgroundColor = Colors.redColor;

                    if(tool.contents.Count!=0)
                    if (GUILayout.Button("Remove All", GUILayout.Width(120), GUILayout.Height(20)))
                    {
                        bool isOk = EditorUtility.DisplayDialog("Confirm Message", "Are you sure that you want to remove all contents ?", "yes", "no");

                        if (isOk)
                        {
                            tool.contents.Clear();
                        }
                    }

                    GUI.backgroundColor = Colors.whiteColor;

                    GUI.backgroundColor = Colors.yellowColor;

                    if (GUILayout.Button("More Assets", GUILayout.Width(120), GUILayout.Height(20)))
                    {
                        Application.OpenURL(Links.indieStudioStoreURL);
                    }

                    GUI.backgroundColor = Colors.whiteColor;
					GUILayout.EndHorizontal ();

    
                     EditorGUILayout.Separator ();

					for (int i = 0; i <  tool.contents.Count; i++) {
                        EditorGUILayout.Foldout(true, "Content[" + i + "]");

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(10);
                        EditorGUILayout.BeginVertical();

                        EditorGUILayout.Separator();

                        EditorGUILayout.BeginHorizontal();
                        //EditorGUILayout.Separator();

                        GUI.backgroundColor = Colors.redColor;
                        if (GUILayout.Button("Remove", GUILayout.Width(70), GUILayout.Height(20)))
                        {
                            bool isOk = EditorUtility.DisplayDialog("Confirm Message", "Are you sure that you want to remove the selected content ?", "yes", "no");

                            if (isOk)
                            {
                                tool.contents.RemoveAt(i);
                                break;
                            }
                        }
                        GUI.backgroundColor = Colors.whiteColor;
                        EditorGUILayout.EndHorizontal();

                        tool.contents [i] = EditorGUILayout.ObjectField ("Element " + i, tool.contents [i], typeof(Transform), true) as Transform;
                        EditorGUILayout.BeginHorizontal();

                        EditorGUI.BeginDisabledGroup(i == tool.contents.Count - 1);
                        if (GUILayout.Button("▼", GUILayout.Width(22), GUILayout.Height(22)))
                        {
                            MoveDown(i, tool);
                        }
                        EditorGUI.EndDisabledGroup();

                        EditorGUI.BeginDisabledGroup(i - 1 < 0);
                        if (GUILayout.Button("▲", GUILayout.Width(22), GUILayout.Height(22)))
                        {
                            MoveUp(i, tool);
                        }
                        EditorGUI.EndDisabledGroup();

                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.Separator();
                        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(2));

                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();
                    }
				//}
			}

			//Audioclip effect for Fill and Stamp features
			if (tool.feature == IndieStudio.DrawingAndColoring.Logic.Tool.ToolFeature.Fill || tool.feature == IndieStudio.DrawingAndColoring.Logic.Tool.ToolFeature.Stamp) {
				tool.audioClip = EditorGUILayout.ObjectField ("Audio Clip", tool.audioClip, typeof(AudioClip), true) as AudioClip;
			}

			if (GUI.changed) {
				DirtyUtil.MarkSceneDirty ();
			}
		}

        private void MoveUp(int index, IndieStudio.DrawingAndColoring.Logic.Tool tool)
        {
            var content = tool.contents[index];
            tool.contents.RemoveAt(index);
            tool.contents.Insert(index - 1, content);
        }

        private void MoveDown(int index, IndieStudio.DrawingAndColoring.Logic.Tool tool)
        {
            var content = tool.contents[index];
            tool.contents.RemoveAt(index);
            tool.contents.Insert(index + 1, content);
        }
    }
}