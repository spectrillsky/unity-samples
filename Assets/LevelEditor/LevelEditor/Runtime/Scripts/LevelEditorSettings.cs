using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LFG.LevelEditor
{
    [CreateAssetMenu(fileName = "LevelEditorSettings", menuName = "LFG/LevelEditor/LevelEditorSettings")]
    public class LevelEditorSettings : ScriptableObject
    {
        [FoldoutGroup("Level Constraints"), Tooltip("The smallest size a level can be"), OnValueChanged(nameof(OnSizeConstraintsChanged))]
        public Vector3 minimumLevelSize;
        [FoldoutGroup("Level Constraints"), Tooltip("The biggest size a level can be"), OnValueChanged(nameof(OnSizeConstraintsChanged))]
        public Vector3 maximumLevelSize;
        [FoldoutGroup("Level Constraints"), Tooltip("Default level size"), OnValueChanged(nameof(OnSizeConstraintsChanged))]
        public Vector3 defaultLevelSize;
        [FoldoutGroup("Level Constraints"), OnValueChanged(nameof(OnSizeConstraintsChanged))]
        public float maxBuildHeight = 25;
        [FoldoutGroup("Level Constraints"), Min(0.1f), Tooltip("How big a single cell is within the grid in world space")]
        public float cellSize = 0.5f;

        #region Assets
        [Space(10)]
        [FoldoutGroup("Assets")]
        public string levelObjectsPath;
        
        #region Layouts
        [field:SerializeField, FoldoutGroup("Assets")] public Level DefaultLayout { get; private set; }

        [field:SerializeField, FoldoutGroup("Assets")] public List<Level> Layouts { get; private set; } = new List<Level>();

        public Level GetLayout(string layoutName)
        {
            Level _layout = Layouts.Find(layout => layout.name.Equals(layoutName) || layout.Layout.Equals(layoutName));
            
            if (!_layout && string.IsNullOrEmpty(layoutName))
            {
                Debug.LogWarning($"[LevelEditor] No Layout object found for {layoutName}. Check Settings if it should exist.");
                _layout = DefaultLayout;
            }

            return _layout;
        }
        
        #endregion
        #endregion
        
        [field: FoldoutGroup("Handles"), SerializeField] public bool ShowHandles { get; private set; }
        [field: FoldoutGroup("Handles"), SerializeField] public LevelObjectHandle HandlePrefab { get; private set; }
        //Fade not yet implemented
        [field: FoldoutGroup("Handles"), SerializeField] public float HandleFadeInDistance { get; private set; }
        [field: FoldoutGroup("Handles"), SerializeField] public float HandleFadeOutDistance { get; private set; }

        [field: FoldoutGroup("Handles/Colors"), SerializeField] public Color HandleSelectedColor { get; private set; } = Color.green;
        [field: FoldoutGroup("Handles/Colors"), SerializeField] public Color HandleUnselectedColor { get; private set; } = Color.blue;
        [field: FoldoutGroup("Handles/Colors"), SerializeField] public Color HandleInvalidColor { get; private set; } = Color.red;

        
        
        [FoldoutGroup("Grid")]
        public Material latticeMaterial;
        [FoldoutGroup("Grid")]
        public float latticeThickness = 0.03f;

        [FoldoutGroup("Placeholder")] public bool usePlacementMaterials;
        [FoldoutGroup("Placeholder"), ShowIf(nameof(usePlacementMaterials), true)]
        public Material validPlacementMaterial, invalidPlacementMaterial;
        
        [FoldoutGroup("Vertex Snapping")] public bool enableVertexSnapping = false;

        [field: SerializeField, FoldoutGroup("Level Objects"), Tooltip("Path to base level objects folder")]
        public List<LevelObjectProfile> LevelObjects { get; private set; } = new List<LevelObjectProfile>();
        [FoldoutGroup("Level Objects"), Tooltip("Layers to search for levels and objects in raycasts")]
        public LayerMask levelObjectLayers, levelLayers;
        [FoldoutGroup("Level Objects")]
        public List<PlacementConstraint> globalConstraints;
        [FoldoutGroup("Level Objects/Volumes")]
        public bool useVolumes;

        [FoldoutGroup("Editing")] public bool ShowEditingVisuals;
        
        [FoldoutGroup("Level Objects/Volumes"),
         Tooltip("Padding between the true size of the object and the box collider created"),
        ShowIf(nameof(useVolumes), true)]
        public float volumePadding = 0f;
        
        [Space(10)]
        [FoldoutGroup("Cursor Settings")]
        public float heightStep = 0.1f;
        [FoldoutGroup("Cursor Settings")]
        public bool targetObjects = false;
        [FoldoutGroup("Cursor Settings")]
        public float minRayDistance, maxRayDistance, rayDistanceStep;

        [FoldoutGroup("Cursor Settings")] public bool ignoreEmptyRaycasts = false;

        [Space(10)]
        [FoldoutGroup("Constraints")]
        public Controller.PlacementMode defaultPlacementMode = Controller.PlacementMode.Grid;
        [FoldoutGroup("Constraints")]
        public Controller.PlacementMode[] allowedBuildModes;
        [FoldoutGroup("Constraints"), Tooltip("Determines where an object is placed on the grid based on their size attribute")]
        public bool strictGridPlacement;
        [FoldoutGroup("Constraints")]
        public bool stackObjects = false;
        [FoldoutGroup("Constraints")]
        public bool stickObjects = false;

        [FoldoutGroup("Validation")] public bool DisableValidation;
        
        [Space(10)]
        [FoldoutGroup("IO"), Tooltip("Save path for level data")]
        public string editorSavePath;
        [FoldoutGroup("IO"), Tooltip("Save levels as if runtime")]
        public bool simulateRuntimeIO = false;
        [FoldoutGroup("IO"), Tooltip("Additional path for persistent saves")]
        public string runtimeSavePath = "/Level";

        [FoldoutGroup("Events")] public bool useDefaultInputs;
        
        [FoldoutGroup("Debug")] public bool drawVolumeCorners = false;
        //TODO: Start creating callbacks for these settings changes 
        [FoldoutGroup("Debug")] public bool drawGridLattice = false;
        [FoldoutGroup("Debug")] public bool showMarkers = false;

        #region Validation
        #if UNITY_EDITOR
        private void OnValidate()
        {
            ValidateLayouts();
        }

        void ValidateLayouts()
        {
            if (!DefaultLayout)
            {
                Debug.LogWarning($"[LevelEditor] No Default Layout assigned!");
                return;
            }
            if (!Layouts.Contains(DefaultLayout))
            {
                Layouts.Add(DefaultLayout);
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssetIfDirty(this);
            }
        }
        #endif
        #endregion
        
        #region Addressables
        
        [Button]
        void LoadLevelObjects()
        {
            Addressables.LoadAssetsAsync<GameObject>("LevelObject", OnAssetLoaded).Completed += OnAssetsLoaded;
        }
        
        private void OnAssetLoaded(GameObject obj)
        {
            if(obj.TryGetComponent(out LevelObjectProfile lvlObj))
                if (!LevelObjects.Exists(lo => lo.ID.Equals(lvlObj.ID)))
                    LevelObjects.Add(lvlObj);
        }
        
        private void OnAssetsLoaded(AsyncOperationHandle<IList<GameObject>> objs)
        {
        }
        #endregion
        
        public List<LevelObjectProfile> GetLevelObjects(LevelObjectType type)
        {
            if (!type) return LevelObjects;
            return LevelObjects.FindAll(lvlObj => lvlObj.Types.Any(_type => _type == type));
        }

        public LevelObjectProfile GetLevelObjectById(string guid)
        {
            LevelObjectProfile lvlObj = LevelObjects.Find(lvlObj => lvlObj.ID.Equals(guid));
            return lvlObj;
        }

        public void SetStacking(bool stackObjects)
        {
            this.stackObjects = stackObjects;
        }

        public void SetSticky(bool stickObjects)
        {
            this.stickObjects = stickObjects;
        }

        public string GetRuntimeSavePath()
        {
            return Application.persistentDataPath + runtimeSavePath;
        }

        public LevelIO.Type GetIOType()
        {
            if (Application.isEditor)
                return simulateRuntimeIO ? LevelIO.Type.LocalJson : LevelIO.Type.EditorObject;
            else
                return LevelIO.Type.LocalJson;
        }

        #region Settings Changed Actions
        public void OnSizeConstraintsChanged()
        {
            new SettingsChangedEvent(Settings.LevelSize).Invoke();
        }
        
        public class SettingsChangedEvent : Aether.Event<SettingsChangedEvent>
        {
            public readonly Settings Setting;
            
            public readonly object Value;

            public SettingsChangedEvent(Settings setting, object value = null)
            {
                Setting = setting;
                Value = value;
            }
        }
        
        public enum Settings
        {
            LevelSize,
            Stacking,
            Sticky,
            ShowAllEditingVisuals,
        }
        #endregion
    }

}