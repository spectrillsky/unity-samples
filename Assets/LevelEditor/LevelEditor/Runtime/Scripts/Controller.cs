using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace LFG.LevelEditor
{
    /// <summary>
    /// The main controller class for the level editor.
    /// </summary>
    public class Controller : Singleton<Controller>
    {
        [SerializeField] LevelEditorSettings _settings;
        public static LevelEditorSettings Settings { get { return instance._settings; }}
        
        //Runtime components
        public InputHandler InputHandler;
        public Cursor Cursor;
        
        private Level _currentLevel;
        public static Level CurrentLevel { get{ return instance._currentLevel; }}

        private State _currentState = State.None;
        public static State CurrentState { get { return instance._currentState; }}
        
        public class OnActionEvent : UnityEvent<Actions, object> { };
        private OnActionEvent _onAction = new OnActionEvent();
        public static OnActionEvent OnAction { get { return instance._onAction; }}
        
        private LevelObject _currentSelection;
        
        public LevelObjectProfile currentResource;

        public List<LevelData> Levels = new List<LevelData>();

        public bool _showAllEditingVisuals;

        public bool _enterEditingModeOnStart = false;
        
        void Awake()
        {
            if (_settings.useDefaultInputs)
            {
                InputHandler = FindObjectOfType<InputHandler>();
                if (!InputHandler) InputHandler = gameObject.AddComponent<InputHandler>();
                InputHandler.OnDiscreteInput.AddListener(OnDiscreteInput);
            }
            Cursor = FindObjectOfType<Cursor>();
        }
        
        public void Start()
        {
            Level level = FindObjectOfType<Level>();
            SetLevel(level, _enterEditingModeOnStart);
            SetBuildMode(_settings.defaultPlacementMode);
        }
        
        public void SetState(State newState)
        {
            _currentState = newState;
            _onAction.Invoke(Actions.ChangeState, newState);
        }

        #region Actions
        [FoldoutGroup("Actions")]
        [Button]
        public LevelData CreateLevel(string levelName, Level layout = null)
        {
            if(_currentLevel) UnloadLevel();
            LevelData levelData = ScriptableObject.CreateInstance<LevelData>();
            levelData.Size = _settings.defaultLevelSize;
            levelData.DisplayName = levelData.name = levelName;

            if (!layout) layout = Settings.DefaultLayout;
            levelData.Layout = layout.Layout;
            
            LevelIO.SaveLevel(levelData, GetIOType());
            return levelData;
        }

        [FoldoutGroup("Actions")]
        [Button]
        public void SetLevel(Level level, bool beginEditing = false)
        {
            if (!level) return;
            
            _currentLevel = level;
            _onAction.Invoke(Actions.SetLevel, level);
            if(beginEditing) SetState(State.Editing);
        }

        [FoldoutGroup("Actions")]
        [Button]
        public void ClearLevel(bool unloadLevel = false)
        {
            if (_currentLevel) return;
            if(unloadLevel) UnloadLevel(_currentLevel);
            _currentLevel = null;
            _onAction.Invoke(Actions.ClearLevel, null);
        }

        [FoldoutGroup("Actions")]
        [Button]
        public Level LoadLevel(LevelData levelData)
        {
            
            if (_currentLevel)
                UnloadLevel(_currentLevel);

            Level _layout = Settings.GetLayout(levelData.Layout);
            Level _level = Instantiate(_layout);

            _level.transform.position = levelData.Position;
            _level.transform.eulerAngles = levelData.Rotation;
            //This has been more trouble than its worth
            // _level.transform.localScale = levelData.Scale;
            _level.Init(levelData);
            
            SetLevel(_level);
            _onAction.Invoke(Actions.LoadLevel, levelData);
            return _currentLevel;
        }

        [FoldoutGroup("Actions")]
        [Button]
        public LevelData[] GetLevels()
        {
            Levels.Clear();
            Levels.AddRange(LevelIO.LoadLevels(GetIOType()));
            return Levels.ToArray();
        }

        [FoldoutGroup("Actions")]
        [Button]
        public void UnloadLevel(Level level = null)
        {
            if (!level) level = _currentLevel;
            if (level)
            {
                if(level == _currentLevel)
                    ClearLevel();
                DestroyImmediate(level.gameObject);
                _onAction.Invoke(Actions.UnloadLevel, level);
            }
        }

        [FoldoutGroup("Actions")]
        [Button]
        public void SaveLevel(Level level = null)
        {
            if (!level) return;
            
            LevelIO.SaveLevel(level.GetData(), GetIOType());
            _onAction.Invoke(Actions.SaveLevel, level);
        }

        [FoldoutGroup("Actions")]
        [Button]
        public void DeleteLevel(Level level)
        {
            if (!level) return;
            
            LevelIO.DeleteLevel(level.GetData(), GetIOType());
            _onAction.Invoke(Actions.DeleteLevel, level);
        }

        [FoldoutGroup("Actions")]
        [Button]
        public LevelData CopyLevel(LevelData data)
        {
            LevelData copy = new LevelData(data);
            LevelIO.SaveLevel(copy, GetIOType());
            if(_currentLevel.GetData().Equals(data)) _currentLevel.Init(copy);
            SetLevel(_currentLevel);
            return copy;
        }
        #endregion

        public void DeleteObject(LevelObject levelObject)
        {
            if (!levelObject) return;
            if(Cursor.ActiveObject == levelObject) Cursor.DeselectObject();

            LevelObjectProfile profile = levelObject.Profile;
            DestroyImmediate(levelObject.gameObject);
            _onAction.Invoke(Actions.DeleteLevelObject, profile);
        }
        
        public void SetResource(LevelObjectProfile resource)
        {
            currentResource = resource;
            _onAction.Invoke(Actions.SetResource, resource);
        }
        
        public void ClearSelection()
        {
            _currentSelection = null;
            _onAction.Invoke(Actions.ClearSelection, null);
        }

        public void SetBuildMode(PlacementMode mode)
        {
            _settings.defaultPlacementMode = mode;
            _onAction.Invoke(Actions.ChangeMode, mode);   
        }

        public LevelIO.Type GetIOType()
        {
            return (Application.isEditor && !_settings.simulateRuntimeIO) ? LevelIO.Type.EditorObject : LevelIO.Type.LocalJson;
        }

        void OnDiscreteInput(InputHandler.DiscreteEvents e, object context) { }

        void OnLoaderEvent(LevelLoader.Events e, object context)
        {
            
        }
        
        public enum State
        {
            None,
            Editing,
            Previewing,
        }

        public enum PlacementMode
        {
            FreeFlow,
            Grid,
            StrictGrid,
        }
        
        public enum Actions
        {
            /// <summary> Context: Level </summary>
            CreateLevel,
            /// <summary> Context: Level </summary>
            SetLevel,
            /// <summary> Context: Level </summary>
            ClearLevel,
            /// <summary> Context: Level </summary>
            LoadLevel,
            /// <summary> Context: Level </summary>
            UnloadLevel,
            /// <summary> Context: Level </summary>
            SaveLevel,
            /// <summary> Context: Level </summary>
            DeleteLevel,
            /// <summary> Context: BuildMode </summary>
            ChangeMode,
            /// <summary> Context: State </summary>
            ChangeState,
            /// <summary> Context: LevelObject </summary>
            SetResource,
            /// <summary> Context: LevelObject </summary>
            PlaceResource,
            /// <summary> Context: LevelObject </summary>
            SelectLevelObject,
            /// <summary> Context: LevelObject </summary>
            CreateLevelObject,
            /// <summary> Context: LevelObject </summary>
            DeselectLevelObject,
            /// <summary> Context: LevelObjectProfile </summary>
            DeleteLevelObject,
            SetSelection,
            ClearSelection,
            /// <summary> Context: boolean </summary>
            SetStacking,
            ChangeEditorSettings,
        }

    }
    
    [Flags]
    public enum GridPlacement
    {
        Center = 1 << 0,
        Side = 1 << 1,
        Corner = 1 << 2,
        Strict = 1 << 3,
    }
}