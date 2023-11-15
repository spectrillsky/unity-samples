using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LFG.LevelEditor.Editor
{
    public class LevelEditorWindow : EditorWindow
    {
        [MenuItem("LifeForce/Level Editor/Main Window")]
        static void Init()
        {
            LevelEditorWindow window = (LevelEditorWindow)EditorWindow.GetWindow(typeof(LevelEditorWindow));
        }
        
        private void OnGUI()
        {
            GUILayout.Label("Level Editor", EditorStyles.boldLabel);
            
            ActionsGUI();            
        }

        void ActionsGUI()
        {
            if (GUILayout.Button("Load Levels"))
            {
                LevelIO.LoadLevels_LocalJson();
                LevelIO.LoadLevels_Editor();
            }

            GUI.enabled = (Controller.CurrentLevel);

            GUI.enabled = true;

        }
    }
}
