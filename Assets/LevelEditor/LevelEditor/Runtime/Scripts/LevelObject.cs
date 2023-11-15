using System;
using Aether;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LFG.LevelEditor
{
    public class LevelObject : MonoBehaviour, IContext
    {
        
        [field: SerializeField, Required] public LevelObjectProfile Profile { get; private set; } 
        
        public Vector3 Size { get { return Profile.Size; } }

        [SerializeReference] private LevelObjectData _data;
        public LevelObjectData Data { get { return _data;} }

        private LevelObjectHandle _handle;
        
        [SerializeField] private Transform _platform;

        [SerializeField] private LevelObjectVolume _volume;

        public Placeholder Placeholder { get; private set; }
        public bool IsSelected = false;

        [SerializeField] private GameObject _editingVisuals;
        
        void Awake()
        {
            Controller.OnAction?.AddListener(OnLevelEditorAction);
            ToggleEditingVisual(false);
        }

        private void OnEnable()
        {
            Initialize();
        }

        void OnLevelEditorAction(Controller.Actions action, object context)
        {
            if(action == Controller.Actions.ChangeState
               && (Controller.State)context == Controller.State.Editing)
                Initialize();
        }

        
        
        public void InitializeData(LevelObjectData data)
        {
            _data = data;
            transform.localPosition = _data.Position;
            transform.localEulerAngles = _data.Rotation;
        }

        public virtual void Initialize()
        {
            if (Controller.CurrentState == Controller.State.Editing)
            {
                CreateVolume();
                CreateHandle();
                if (Profile.AlwaysShowEditingVisuals)
                    ToggleEditingVisual(true);
            }
            else
            {
                DestroyHandle();
                ToggleEditingVisual(false);
            }
            
        }
        
        void CreateVolume()
        {
            if (!Controller.Settings.useVolumes) return;
            
            _volume = GetComponent<LevelObjectVolume>();
            if (!GetComponent<LevelObjectVolume>())
                _volume = gameObject.AddComponent<LevelObjectVolume>();
        }

        void CreateHandle()
        {
            if (!Profile.UseHandle || !Controller.Settings.ShowHandles)
            {
                DestroyHandle();
                return;
            }

            _handle = Instantiate(Controller.Settings.HandlePrefab, transform);
            _handle.Initialize(this);
        }

        void ToggleEditingVisual(bool isActive)
        {
            if (!Controller.Settings.ShowEditingVisuals)
            {
                if(_editingVisuals) _editingVisuals.SetActive(false);
                return;
            }
            if(_editingVisuals) _editingVisuals.SetActive(isActive);
        }

        void DestroyHandle()
        {
            if (!_handle) return;
            
            Destroy(_handle.gameObject);
        }
        
        public virtual void Select()
        {
            if (IsSelected) return;
            IsSelected = true;

            ToggleEditingVisual(true);
            CreatePlaceholder();
            
            new Event(this, EventType.Selected).Invoke();
        }
        
        public virtual void Deselect()
        {
            if (!IsSelected) return;
            IsSelected = false;
            
            DestroyPlaceholder();
            if(!Profile.AlwaysShowEditingVisuals)
                ToggleEditingVisual(false);

            new Event(this, EventType.Deselected).Invoke();
        }
        
        public virtual Placeholder CreatePlaceholder()
        {
            if (Placeholder) return Placeholder;
            Placeholder = gameObject.AddComponent<Placeholder>();
            Placeholder.OnEvent.AddListener(OnPlaceholderEvent);
            if(Data == null) DestroyHandle();
            return Placeholder;
        }

        public virtual void OnPlaceholderEvent(Placeholder.Events e)
        {
            
        }

        public virtual void DestroyPlaceholder()
        {
            if (!Placeholder) return;
            if(Application.isPlaying)
                Destroy(Placeholder);
            else
                DestroyImmediate(Placeholder);            
        }

        public ConstraintCheckResult CanBePlacedAtPosition(Vector3 pos)
        {
            //Global Constraints
            foreach (var c in Controller.Settings.globalConstraints)
            {
                ConstraintCheckResult result = c.CanBePlaced(this, pos);
                if (!result.IsValid)
                    return result;
            }
            //Level Object Constraints
            foreach(var c in Profile.Constraints)
            {
                ConstraintCheckResult result = c.CanBePlaced(this, pos);
                if (!result.IsValid)
                    return result;
            }
            //Type Constraints
            foreach (var t in Profile.Types)
                foreach(var c in t.Constraints)
                {
                    ConstraintCheckResult result = c.CanBePlaced(this, pos);
                    if (!result.IsValid)
                        return result;
                }

            return new ConstraintCheckResult(null, this, true, "LevelObject is within constraints", pos);
        }

        public float GetRotationStep()
        {
            if (Profile.Types.Length == 0)
                return 90;
            else
                return Profile.Types[0].RotationStep;
        }
        
        #region Events
        public class Event : Aether.ContextEvent<Event, LevelObject>
        {
            public LevelObject LevelObject;
            public readonly EventType Type;
            public new readonly object Context;
            
            public Event(LevelObject levelObject, EventType type, object context = null) : base(levelObject)
            {
                LevelObject = levelObject;
                Type = type;
                Context = context;
            }
        }

        public enum EventType
        {
            Initialize,
            Selected,
            Deselected,
            CreatePlaceholder,
            DestroyPlaceholder,
        }
        #endregion
    }

}
