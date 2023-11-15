using System;
using System.Linq;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace LFG.LevelEditor
{
    [CreateAssetMenu(fileName = "LevelObjectProfile", menuName = "LifeForce/LevelEditor/LevelObjectProfile")]
    public class LevelObjectProfile : ScriptableObject
    {

        [SerializeField, ReadOnly] private string _id;
        
        [field:SerializeField] public string ID { get { return _id; } }

        [field:SerializeField] public string DisplayName { get; private set; }
        
        [field:SerializeField] public bool HasSize { get; private set; }
        [field:SerializeField] public Vector3 Size { get; private set; }

        [field:SerializeField] public LevelObjectType[] Types { get; private set; }

        [field:SerializeField] public bool UseHandle { get; private set; }
        [field:SerializeField] public bool AlwaysShowEditingVisuals { get; private set; }
        [field:SerializeField] public GameObject Prefab { get; private set; }

        [field:SerializeField] public PlacementConstraint[] Constraints { get; private set; }
        
        [field:SerializeField] public bool UseVolume { get; private set; }
        
        [field:SerializeField, Min(0)] public int MinInstances { get; private set; }
        [field:SerializeField, Min(-1)] public int MaxInstances { get; private set; }
        
        private void OnValidate()
        {
            #if UNITY_EDITOR
            if(String.IsNullOrEmpty(_id))
                GenerateID();
            #endif
        }
        
        //Assign a guid to the level object if it is a prefab and doesn't already have one.
        #if UNITY_EDITOR
        [Button, ShowIf(nameof(_id), "")]
        void GenerateID()
        {
            if (!String.IsNullOrEmpty(_id))
            {
                if(!EditorUtility.DisplayDialog(
                       "Level Object already has an ID",
                       "Generate a new ID anyways? \n This could have unintended consequences.",
                       "Yes",
                       "No"))
                    return;
                if (!String.IsNullOrEmpty(_id) && !EditorUtility.DisplayDialog(
                        "Are you 100% sure you want to create a new ID",
                        "",
                        "Generate ID",
                        "Nevermind"))
                    return;
            }

            _id = Guid.NewGuid().ToString();
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
        }
        #endif
        
        public bool HasGridPlacement(GridPlacement placement)
        {
            return Types.Any(t => t.HasGridPlacement(placement));
        }

        public bool HasMaxInstances()
        {
            return MaxInstances >= 0;
        }
    }
}
