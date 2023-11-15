using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace LFG.LevelEditor
{
    public static class LevelIO
    {
        public static void SaveLevel(LevelData levelData, Type type)
        {
            #if UNITY_EDITOR
                if(type == Type.EditorObject)
                    SaveLevel_Editor(levelData);
                else if(type == Type.LocalJson)
                    SaveLevel_LocalJson(levelData);
            #else
                SaveLevel_LocalJson(levelData);
            #endif
            
        }

        public static void SaveLevel_LocalJson(LevelData levelData)
        {
            string basePath = Controller.Settings.GetRuntimeSavePath();
            if (!Directory.Exists(basePath))
                Directory.CreateDirectory(basePath);
            string json = JsonUtility.ToJson(levelData);
            File.WriteAllText($"{basePath}/{levelData.Guid}.json", json);
        }

        public static LevelData[] LoadLevels(LevelIO.Type type)
        {
            #if UNITY_EDITOR
                if (type == Type.EditorObject)
                    return LoadLevels_Editor();
                else
                    return LoadLevels_LocalJson();
            #else
                return LoadLevels_LocalJson();
            #endif
        }
        
        public static LevelData[] LoadLevels_LocalJson()
        {
            List<LevelData> levels = new List<LevelData>();
            string basePath = Controller.Settings.GetRuntimeSavePath();
            if (!Directory.Exists(basePath))
                Directory.CreateDirectory(basePath);
            foreach (var file in Directory.GetFiles(basePath))
            {
                string json = File.ReadAllText(file);

                LevelData data = ScriptableObject.CreateInstance<LevelData>();
                JsonUtility.FromJsonOverwrite(json, data);
                levels.Add(data);
            }
            return levels.ToArray();
        }
        
        
        public static void DeleteLevel(LevelData levelData, Type type)
        {
            #if UNITY_EDITOR
                if(type == Type.EditorObject)
                    DeleteLevel_Editor(levelData);
                else
                    DeleteLevel_LocalJson(levelData);
            #else
                DeleteLevel_LocalJson(levelData);
            #endif
        }

        public static void DeleteLevel_LocalJson(LevelData levelData)
        {
            string fullPath = $"{Controller.Settings.GetRuntimeSavePath()}/{levelData.Guid}.json";
            if(File.Exists(fullPath))
                File.Delete(fullPath);
        }
        
        #region Editor Actions
        #if UNITY_EDITOR
        public static void SaveLevel_Editor(LevelData data)
        {
            string basePath = Controller.Settings.editorSavePath;

            string oldPath = AssetDatabase.GetAssetPath(data);

            string fullPath = $"{basePath}/{GetLevelAssetName(data)}";
            if (string.IsNullOrEmpty(oldPath))
            {
                Debug.Log($"[LevelEditor] Creating new level asset at {fullPath}");
                AssetDatabase.CreateAsset(data,fullPath);
            }
            else
            {
                EditorUtility.SetDirty(data);
                AssetDatabase.SaveAssetIfDirty(data);
                AssetDatabase.RenameAsset(oldPath, fullPath);
            }
        }
        
        public static LevelData[] LoadLevels_Editor()
        {
            List<LevelData> allLevelData = new List<LevelData>();
            string basePath = Controller.Settings.editorSavePath;
            var guids = AssetDatabase.FindAssets("t:LevelData", new[]{basePath});
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                LevelData data = AssetDatabase.LoadAssetAtPath<LevelData>(path);
                allLevelData.Add(data);
            }

            return allLevelData.ToArray();
        }
        

        public static void DeleteLevel_Editor(LevelData data)
        {
            
        }
        #endif
        #endregion

        public static void ValidateDirectories()
        {
            
        }
        
        public static string GetLevelAssetName(LevelData levelData)
        {
            return $"{levelData.DisplayName}.asset";
        }
        
        public enum Type
        {
            LocalJson,
            EditorObject,
        }
    }
}
