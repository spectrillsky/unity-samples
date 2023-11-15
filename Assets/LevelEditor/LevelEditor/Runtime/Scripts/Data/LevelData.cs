using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace LFG.LevelEditor
{
    [Serializable]
    [CreateAssetMenu(fileName = "LevelData", menuName = "LifeForce/LevelData")]
    public class LevelData : ScriptableObject
    {
        public string DisplayName = "LevelData";
        public string Guid;

        public string Layout;
        
        public Vector3 Size;

        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale = Vector3.one;
        
        public List<LevelObjectData> LevelObjects = new List<LevelObjectData>();

        public virtual void Capture(Level level)
        {
            if (string.IsNullOrEmpty(Guid))
                Guid = System.Guid.NewGuid().ToString();

            if (string.IsNullOrEmpty(DisplayName))
                DisplayName = GetType().ToString();

            Layout = level.Layout;
            
            Size = level.Size;
            Position = level.transform.position;
            Rotation = level.transform.eulerAngles;
            Scale = level.transform.localScale;
            
            LevelObjects.Clear();
            foreach(var lvlObj in level.GetComponentsInChildren<LevelObject>())
                LevelObjects.Add(new LevelObjectData(lvlObj));
        }
        
        public LevelData()
        {
            Guid = System.Guid.NewGuid().ToString();
        }

        public LevelData(Vector3 _size)
        {
            Guid = System.Guid.NewGuid().ToString();
            Size = _size;
        }

        public LevelData(Level level)
        {
            Capture(level);
        }

        public LevelData(LevelData data)
        {
            DisplayName = $"{data.DisplayName} - Copy";
            Guid = System.Guid.NewGuid().ToString();
            Size = data.Size;
            Position = data.Position;
            Rotation = data.Rotation;
            Scale = data.Scale;
            LevelObjects = data.LevelObjects;
        }
    }

    [Serializable]
    public class LevelObjectData
    {
        public string ID;
        public string ResourceID;

        public string ParentID;
        
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale;

        public Dictionary<string, string> Attributes;
        public Dictionary<string, string> Data;
        
        public LevelObjectData(){}

        public LevelObjectData(LevelObject lvlObj)
        {
            if (lvlObj.Data == null || string.IsNullOrEmpty(lvlObj.Data.ID))
                ID = System.Guid.NewGuid().ToString();
            else
                ID = lvlObj.Data.ID;
            
            ResourceID = lvlObj.Profile.ID;

            LevelObject parent = lvlObj.GetComponentInParent<LevelObject>();

            if (parent != lvlObj)
                ParentID = parent.Data.ID;
            
            Position = lvlObj.transform.localPosition;
            Rotation = lvlObj.transform.localEulerAngles;
            Scale = lvlObj.transform.lossyScale;
        }
    }
    
}
