using System.Collections.Generic;
using System.Linq;
using Aether;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LFG.LevelEditor
{
    public class Level : MonoBehaviour, IContext
    {
        [field: SerializeField, Required] public string Layout { get; private set; }
        
        [SerializeField] private Vector3 _size;
        public Vector3 Size{ get { return _size; } }

        [field: SerializeField]
        public LevelData Data { get; private set; }
        

        [field: SerializeField]
        public LevelGrid Grid { get; private set; }

        [field: SerializeField] public LevelValidator Validator { get; private set; }

        protected virtual void Awake()
        {
            Controller.OnAction?.AddListener(OnLevelEditorAction);
            
            SetSize(Size);

            if(!Controller.Settings.DisableValidation)
                Validator = new LevelValidator(this);
        }

        [Button]
        public virtual void Init(LevelData data)
        {
            Data = data;
            SetSize(Data.Size);
            Clear();
            
            foreach (var lvlObjData in data.LevelObjects)
            {
                LevelObjectProfile profile = Controller.Settings.GetLevelObjectById(lvlObjData.ResourceID);
                if (profile)
                {
                    #if UNITY_EDITOR
                        LevelObject lvlObj = ((GameObject)PrefabUtility.InstantiatePrefab(profile.Prefab, transform)).GetComponent<LevelObject>();
                    #else
                        LevelObject lvlObj = Instantiate(profile.Prefab, transform).GetComponent<LevelObject>();
                    #endif
                    lvlObj.InitializeData(lvlObjData);
                }
                else
                    Debug.LogWarning($"[LevelEditor] Level Object not found: Lost LevelObjectResource ID reference - ( {lvlObjData.ResourceID} )");
            }
            
            if(!Controller.Settings.DisableValidation)
                Validator = new LevelValidator(this);
            
            new Event(this, EventType.Initialize).Invoke();
        }

        /// <summary>
        /// Cleans up level objects that are not included in the current LevelData
        /// </summary>
        [Button]
        public virtual void Clean()
        {
            foreach (var lvlObj in GetComponentsInChildren<LevelObject>())
            {
                if (!Data || !Data.LevelObjects.Exists(lvlObjData => lvlObjData.ID == lvlObj.Data.ID))
                {
                    if(Application.isPlaying)
                        Destroy(lvlObj.gameObject);
                    else
                        DestroyImmediate(lvlObj.gameObject);
                }
            }
            
            new Event(this, EventType.Clean).Invoke();
        }

        /// <summary>
        /// Clears all levelobjects in the level
        /// </summary>
        [Button]
        public virtual void Clear()
        {
            foreach (var lvlObj in GetComponentsInChildren<LevelObject>())
                if(Application.isPlaying)
                        Destroy(lvlObj.gameObject);
                else
                    DestroyImmediate(lvlObj.gameObject);
            
            new Event(this, EventType.Clear).Invoke();
        }

        [Button]
        public virtual LevelData GetData(bool createNewData = false)
        {
            LevelData data = Data;
            if (createNewData || !data)
                data = ScriptableObject.CreateInstance<LevelData>();

            data.Capture(this);
            if (!Data || !createNewData) Data = data;

            return data;
        }
        
        [Button]
        public virtual void Save(bool saveAsNew = false)
        {
            #if UNITY_EDITOR
                LevelIO.SaveLevel_Editor(GetData(saveAsNew));
            #else
                LevelIO.SaveLevel_LocalJson(GetData());
            #endif
            
        }
        
        void OnLevelEditorAction(Controller.Actions action, object context)
        {
            if (action == Controller.Actions.ChangeState)
            {
                Controller.State state = (Controller.State)context;
                if(state == Controller.State.Editing) CreateGrid();
                else DestroyGrid();
            }
            // else if (action == Controller.Actions.CreateLevelObject)
            // {
            //  TODO: Setting to choose whether or not to add created level objects stright to the data.
            //  TODO: Lost my thought but its there - whoa so deep lol anyways carry on
            // }
        }
        
        [Button]
        public void SetSize(Vector3 size)
        {
            Vector3 maxSize = Controller.Settings.maximumLevelSize;
            Vector3 minSize = Controller.Settings.minimumLevelSize;
            
            Vector3 newSize = new Vector3(
                Mathf.Clamp(size.x, minSize.x, maxSize.x),
                Mathf.Clamp(size.y, minSize.y, maxSize.y),
                Mathf.Clamp(size.z, minSize.z, maxSize.z));
            _size = newSize;
            
            if(!Grid) CreateGrid();
            Grid.SetSize(_size);
            
            new Event(this, EventType.SetSize).Invoke();
        }

        void CreateGrid()
        {
            Grid = GetComponentInChildren<LevelGrid>(); 
            if (!Grid)
            {
                Grid = new GameObject("LevelGrid", typeof(LevelGrid)).GetComponent<LevelGrid>();
                Grid.transform.SetParent(transform);
            }
        }

        void DestroyGrid()
        {
            if(!Grid) Grid = GetComponentInChildren<LevelGrid>();
            if(Grid) Destroy(Grid.gameObject);
        }

        void OnDestroy()
        {
            if(Validator != null) Validator.Dispose();
        }

        #region Utility

        public Vector3 GetMinPoint()
        {
            return transform.position - _size / 2;
        }

        public Vector3 GetMaxPoint()
        {
            return transform.position + _size / 2;
        }
        
        public Vector3 GetCenterPoint()
        {
            return transform.localPosition + _size / 2;
            

        }
        public LevelObjectType[] GetTypes(LevelObject[] levelObjects)
        {
            List<LevelObjectType> types = new List<LevelObjectType>();
            foreach (var levelObject in levelObjects)
            {
                foreach(var type in levelObject.Profile.Types)
                    if(!types.Exists(_type => type == type))
                        types.Add(type);
            }

            return types.ToArray();
        }

        public  LevelObjectProfile[] GetProfiles(LevelObject[] levelObjects)
        {
            return levelObjects.Select(levelObject => levelObject.Profile).Distinct().ToArray();
        }

        public int GetLevelObjectCount(LevelObjectProfile profile)
        {
            return GetLevelObjects().Where(levelObject => levelObject.Profile == profile).Count();
        }
        #endregion

        public LevelObject[] GetLevelObjects()
        {
            return GetComponentsInChildren<LevelObject>();
        }

        public class Event : Aether.ContextEvent<Event, Level>
        {
            public readonly EventType Type;

            public Event(Level level, EventType type) : base(level)
            {
                
            }
        }

        public enum EventType
        {
            Initialize,
            Clean,
            Clear,
            SetSize,
        }
    }
}
