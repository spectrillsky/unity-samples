using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace LFG.LevelEditor
{
    /// <summary>
    /// A helper component to detect the actively placed object.
    /// </summary>
    public class Placeholder : MonoBehaviour
    {
        private LevelObject _levelObject;
        
        private Dictionary<Transform, string> _tagDictionary = new Dictionary<Transform, string>();

        private bool _addedRigidbody = false;
        private Rigidbody _rigidbody;
        
        private List<LevelObject> _overlappingObjects = new List<LevelObject>();
        private List<Collider> _overlappingColliders = new List<Collider>();
        
        private Dictionary<Collider, bool> _colliderTriggerValues = new Dictionary<Collider, bool>();
        private Dictionary<MeshCollider, bool> _meshConvexValues = new Dictionary<MeshCollider, bool>();

        private Dictionary<Renderer, Material> _originalMeshMaterials = new Dictionary<Renderer, Material>();
        
        [SerializeField] private ConstraintCheckResult _lastConstraintCheckResult;

        public bool IsInitialized { get; private set; } 
        
        public class PlaceholderEvent : UnityEvent<Events>{};

        public PlaceholderEvent OnEvent = new PlaceholderEvent();
        
        public bool IsValid
        {
            get { return _lastConstraintCheckResult != null ? _lastConstraintCheckResult.IsValid : true; }
        }

        void Init()
        {
            if (IsInitialized) return;
            
            _levelObject = GetComponentInParent<LevelObject>();
            Validate();
            ChangeTags();
            SetRigidbody();
            SetMeshRenderers();
            ChangeColliders();
            Cursor.instance.onEvent.AddListener(OnCursorEvent);
            OnEvent.Invoke(Events.Init);
            IsInitialized = true;
        }

        void Deinit()
        {
            if (!IsInitialized) return;
            
            RevertTags();
            SetRigidbody(false);
            RevertMeshMaterials();
            RevertColliders();
            Cursor.instance.onEvent.RemoveListener(OnCursorEvent);
            OnEvent.Invoke(Events.Deinit);
            IsInitialized = false;
        }

        ConstraintCheckResult Validate()
        {
            RemoveMissingReferences();
            
            _lastConstraintCheckResult = _levelObject.CanBePlacedAtPosition(transform.position);

            if(_lastConstraintCheckResult.IsValid) OnValid();
            else OnInvalid();
            return _lastConstraintCheckResult;
        }

        void RemoveMissingReferences()
        {
            for (int i = _overlappingColliders.Count - 1; i >= 0; i--)
            {
                if (_overlappingColliders[i] == null)
                    _overlappingColliders.RemoveAt(i); 
            }
            
            for (int i = _overlappingObjects.Count - 1; i >= 0; i--)
            {
                if (_overlappingObjects[i] == null)
                    _overlappingObjects.RemoveAt(i); 
            }
        }

        void SetRigidbody(bool initializing = true)
        {
            _rigidbody = GetComponent<Rigidbody>();
            if (initializing)
            {
                if (!_rigidbody)
                {
                    _addedRigidbody = true;
                    _rigidbody = gameObject.AddComponent<Rigidbody>();
                }
                _rigidbody.isKinematic = true;
            }
            else
            {
                if(_addedRigidbody)
                    Destroy(_rigidbody);
            }
        }

        #region Mesh Methods
        void SetMeshRenderers()
        {
            foreach(var meshRenderer in GetComponentsInChildren<Renderer>())
                _originalMeshMaterials.Add(meshRenderer, meshRenderer.material);
        }

        void RevertMeshMaterials()
        {
            foreach (var pair in _originalMeshMaterials)
                pair.Key.material = pair.Value;
        }
        #endregion
        
        #region Collider Methods
        void ChangeColliders()
        {
            _colliderTriggerValues.Clear();
            _meshConvexValues.Clear();
            foreach (var collider in GetComponentsInChildren<Collider>())
            {
                _colliderTriggerValues.Add(collider, collider.isTrigger);
                if (collider.GetType() == typeof(MeshCollider))
                {
                    MeshCollider meshCollider = collider.GetComponent<MeshCollider>(); 
                    _meshConvexValues.Add(meshCollider, meshCollider.convex);
                    meshCollider.convex = true;
                }
                collider.isTrigger = true;
            }
        }

        void RevertColliders()
        {
            foreach (var pair in _colliderTriggerValues)
                if(pair.Key)
                    pair.Key.isTrigger = pair.Value;
            foreach (var pair in _meshConvexValues)
                pair.Key.convex = pair.Value;
        }
        #endregion
        
        void OnTriggerEnter(Collider other)
        {
            if (other.isTrigger) return;
            LevelObject lvlObj = other.GetComponentInParent<LevelObject>();
            if (lvlObj)
                AddOverlappingObject(lvlObj, other);
        }

        void OnTriggerStay(Collider other)
        {
            if (other.isTrigger) return;
            LevelObject lvlObj = other.GetComponentInParent<LevelObject>();
            if (lvlObj)
                AddOverlappingObject(lvlObj, other);
        }

        void OnTriggerExit(Collider other)
        {
            if (other.isTrigger) return;
            LevelObject lvlObj = other.GetComponentInParent<LevelObject>();
            if (lvlObj)
                RemoveOverlappingObject(lvlObj, other);
        }

        void AddOverlappingObject(LevelObject lvlObj, Collider collider)
        {
            if(!_overlappingColliders.Exists(c => c == collider))
                _overlappingColliders.Add(collider);
            if (_overlappingObjects.Exists(lo => lo.Equals(lvlObj))) return;
            _overlappingObjects.Add(lvlObj);
            ChangeAppearance(_overlappingObjects.Count > 0);
        }

        void RemoveOverlappingObject(LevelObject lvlObj, Collider collider)
        {
            _overlappingObjects.Remove(lvlObj);
            _overlappingColliders.Remove(collider);
            ChangeAppearance(_overlappingObjects.Count > 0);
        }

        public void OnValid()
        {
            ChangeAppearance(true);
            OnEvent.Invoke(Events.Valid);
        }

        public void OnInvalid()
        {
            ChangeAppearance(false);
            OnEvent.Invoke(Events.Invalid);
        }

        public void ChangeAppearance(bool isValid)
        {
            if (!IsInitialized || !Controller.Settings.usePlacementMaterials) return;
            
            foreach (var meshRenderer in GetComponentsInChildren<Renderer>())
            {
                if(meshRenderer is SpriteRenderer) continue;
                meshRenderer.material = isValid
                    ? Controller.Settings.validPlacementMaterial
                    : Controller.Settings.invalidPlacementMaterial;
            }
        }

        void ChangeTags()
        {
            _tagDictionary = new Dictionary<Transform, string>();
            foreach (var child in GetComponentsInChildren<Transform>())
            {
                _tagDictionary.Add(child, child.tag);
                child.tag = "Placeholder";
            }
        }

        void RevertTags()
        {
            foreach (var kV in _tagDictionary)
                if(kV.Key)
                    kV.Key.tag = kV.Value;
        }

        public bool IsOverlapping()
        {
            if (_overlappingColliders.Count == 0)
                return false;
            
            List<RaycastHit> hits = new List<RaycastHit>();
            hits.AddRange(_rigidbody.SweepTestAll(Vector3.up, 0.001f, QueryTriggerInteraction.Ignore));
            hits.AddRange(_rigidbody.SweepTestAll(Vector3.down, 0.001f, QueryTriggerInteraction.Ignore));
            hits.AddRange(_rigidbody.SweepTestAll(Vector3.left, 0.001f, QueryTriggerInteraction.Ignore));
            hits.AddRange(_rigidbody.SweepTestAll(Vector3.right, 0.001f, QueryTriggerInteraction.Ignore));
            hits.AddRange(_rigidbody.SweepTestAll(Vector3.forward, 0.001f, QueryTriggerInteraction.Ignore));
            hits.AddRange(_rigidbody.SweepTestAll(Vector3.back, 0.001f, QueryTriggerInteraction.Ignore));

            if (hits.Count == 0 && _overlappingColliders.Count > 0)
                return true;
            
            //Remove all the colliders that come back from the sweep tests
            //We can classify any overlapping colliders that touch within this buffer size to be only touching
            //If all the overlapping colliders are classified as touching, then we won't consider the object to be overlapping
            List<Collider> overlappingColliders = _overlappingColliders;
            foreach (var hit in hits)
                overlappingColliders.Remove(hit.collider);

            return overlappingColliders.Count > 0;
        }

        void OnCursorEvent(Cursor.Events e, object context)
        {
            if (e == Cursor.Events.UpdatePosition)
                Validate();
        }

        private void OnEnable()
        {
            Init();
        }

        private void OnDisable()
        {
            Deinit();
        }

        private void OnDestroy()
        {
            Deinit();
        }

        public enum Events
        {
            Init,
            Valid,
            Invalid,
            Deinit,
        }
    }
}
