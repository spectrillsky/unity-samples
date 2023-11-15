using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using Sirenix.Utilities;

namespace LFG.LevelEditor
{
    /// <summary>
    /// Handles logic and state for the selection and placement of level objects
    /// </summary>
    public class Cursor : Singleton<Cursor>
    {
        private State _currentState = State.Idle;

        [SerializeField] private InputData _inputData;
        
        [SerializeField] private Camera _camera;
        public void SetCamera(Camera camera) { _camera = camera; }
        
        [SerializeReference] LevelObject _activeObject;
        [SerializeReference] private LevelObjectData _activeObjectOriginalData;
        
        [SerializeField] private LevelObject _placeholder;
        
        [SerializeField, FoldoutGroup("Marker")] private Transform _marker, _levelMarker;
        [SerializeField, FoldoutGroup("Marker")] private LineRenderer _markersLine;

        private float _rayDistance = 100f;
        
        private Vector3 _lastPosition;
        [FoldoutGroup("Placing Mode")]
        private Vector3 _currentPosition;
        [FoldoutGroup("Placing Mode")]
        private Vector3 _currentAdjustedPosition;        
        [FoldoutGroup("Placing Mode")]
        private float _currentHeight = 0f;

        [FoldoutGroup("Vertex Snapping")]
        private bool _isVertexSnapping;
        [FoldoutGroup("Vertex Snapping")]
        private RaycastVertexResult _currentVertex;

        [FoldoutGroup("Surface Snapping")]
        private Vector3 _currentSurfacePoint;

        public class OnCursorEvent : UnityEvent<Events, object>{}; 
        public OnCursorEvent onEvent = new OnCursorEvent();
        
        
        #region Accessors
        public bool IsUsingPlaceholder { get { return _placeholder; }}
        public LevelObject ActiveObject { get { return _activeObject; }}
        public State CurrentState { get { return _currentState; }}
        #endregion
        
        #region Init/Deinit
        void Awake()
        {
            if (_camera == null) _camera = Camera.main;
            Controller.OnAction?.AddListener(OnLevelEditorAction);
        }

        void OnEnable()
        {
            if(Controller.instance.InputHandler) Controller.instance.InputHandler.OnDiscreteInput.AddListener(OnDiscreteInput);
        }
        #endregion

        void Update()
        {
            if (!Controller.CurrentLevel) return;
            
            if(_currentState == State.Idle || _currentState == State.Controlling)
                UpdatePosition();
            else if(_currentState == State.SelectingVertex)
                SelectVertex();
            else if(_currentState == State.VertexSnapping)
                UpdateVertexPosition();
        }
        
        #region State Functions

        void Initialize()
        {
            SetState(State.Idle);
        }

        void Reset()
        {
            //TODO Create default values and reset to them
        }

        void Deinitialize()
        {
            DestroyPlaceholder();
            DeselectObject();
            EndControlling();
        }
        
        public void SetState(State newState)
        {
            if (newState == _currentState) return;
            
            _currentState = newState;
            onEvent?.Invoke(Events.ChangeState, newState);
        }
        #endregion
        
        #region Event Listeners
        void OnLevelEditorAction(Controller.Actions action, object context)
        {

            if (!Controller.Settings.useDefaultInputs) return;
            
            if (action == Controller.Actions.ChangeState)
            {
                var state = (Controller.State)context;
                if(state == Controller.State.None) Initialize();
                else Deinitialize();
            }
            else if (action == Controller.Actions.SetSelection)
                SelectObject((LevelObject)context, false);
            else if (action == Controller.Actions.SetResource)
                CreatePlaceholder((LevelObjectProfile)context);
            else if (action == Controller.Actions.ClearSelection)
                CancelObject();
            else if (action == Controller.Actions.ChangeMode)
                SnapHeightToGrid();
            else if (action == Controller.Actions.UnloadLevel)
            {
                if(_activeObject) DeselectObject();
            }
            else if (action == Controller.Actions.ClearLevel)
                Deinitialize();
            else if (action == Controller.Actions.SetLevel)
                Initialize();
        }

        void OnDiscreteInput(InputHandler.DiscreteEvents e, object context)
        {
            if (!Controller.Settings.useDefaultInputs) return;
            
            if (e == InputHandler.DiscreteEvents.SelectDown)
            {
                if (_activeObject && _currentState == State.Controlling)
                        PlaceObject();
            }
            else if (e == InputHandler.DiscreteEvents.Cancel)
                CancelObject();
            else if(e == InputHandler.DiscreteEvents.SecondaryRotate)
                RotateObject((float)context);
            else if(e == InputHandler.DiscreteEvents.Scroll)
                ChangeHeight((float)context);
            else if (e == InputHandler.DiscreteEvents.SecondaryScroll)
            {
                ChangeRayDistance((float)context);
            }
            else if (e == InputHandler.DiscreteEvents.SelectObject)
            {
                var data = (LevelObjectRaycastData)context;
                if(!_activeObject || (_activeObject && !_placeholder && _currentState != State.Controlling))
                    SelectObject(data.LevelObject, false);
            }
            else if (e == InputHandler.DiscreteEvents.BeginSelectingVertex)
            {
                if(_currentState == State.Selected)
                    BeginSelectingVertex();
            }
            else if (e == InputHandler.DiscreteEvents.BeginVertexSnapping)
            {
                if(_currentState == State.SelectingVertex)
                    BeginVertexSnapping();
            }
            else if (e == InputHandler.DiscreteEvents.EndVertexSnapping)
            {
                if(_currentState == State.SelectingVertex || _currentState == State.VertexSnapping)
                    EndVertexSnapping();
            }
        }
        #endregion
        
        #region Selection Actions
        public void SelectObject(LevelObject levelObject, bool controlObject)
        {
            if(levelObject != _activeObject) DeselectObject();
            if(levelObject != _placeholder) DestroyPlaceholder();
            
            _activeObject = levelObject;
            _activeObjectOriginalData = new LevelObjectData(levelObject);
            _activeObject.Select();
            
            if(controlObject)
                SetState(State.Controlling);
            else
                SetState(State.Selected);
            
            onEvent?.Invoke(Events.SelectObject, levelObject);
        }

        public void BeginControlling()
        {
            SetState(State.Controlling);
            onEvent?.Invoke(Events.BeginControlling, _activeObject);
        }

        public void EndControlling()
        {
            if(_activeObject)
                SetState(State.Selected);
            else
                SetState(State.Idle);
            onEvent?.Invoke(Events.EndControlling, _activeObject);
        }

        public void CreatePlaceholder(LevelObjectProfile lvlObjProfile)
        {
            DestroyPlaceholder();
            _placeholder = Instantiate(lvlObjProfile.Prefab).GetComponent<LevelObject>();
            SelectObject(_placeholder, true);
            onEvent?.Invoke(Events.CreatePlaceholder, _placeholder);
            BeginControlling();
        }
        
        public void DestroyPlaceholder()
        {
            if(_placeholder) Destroy(_placeholder.gameObject);
            _placeholder = null;
        }

        public void UpdatePosition(bool useLastPos = false)
        {
            Vector3 pos = _lastPosition;
            if (!useLastPos)
            {
                Ray ray = _camera.ScreenPointToRay(_inputData.MousePosition);
                
                LevelObjectRaycastData raycastData =
                    Utility.RaycastLevelObject(
                        ray,
                        _rayDistance,
                        Controller.Settings.strictGridPlacement
                    );
                pos = raycastData.Point;
                
                if (Controller.Settings.ignoreEmptyRaycasts && !raycastData.Level &&
                    !raycastData.LevelObject) return;
            }
            
            _lastPosition = pos;
            if (Controller.Settings.defaultPlacementMode == Controller.PlacementMode.Grid)
                TransformToGrid(ref pos);
            
            _currentPosition = pos;
            _currentAdjustedPosition = _currentPosition + _currentHeight * Vector3.up;
            
            ClampPosition(ref _currentPosition);
            
            if(Controller.Settings.stackObjects)
                _currentAdjustedPosition = Utility.GetStackPosition(_currentAdjustedPosition);

            if (_activeObject && _currentState == State.Controlling)
                _activeObject.transform.position = _currentAdjustedPosition;

            SetMarkers();
            onEvent?.Invoke(Events.UpdatePosition, _currentAdjustedPosition);
            
        }

        public void RotateObject(float direction)
        {
            if (!_activeObject) return;

            float degrees = direction * _activeObject.GetRotationStep();
            _activeObject.transform.Rotate(degrees*Vector3.up, Space.World);
        }

        public void PlaceObject()
        {
            ConstraintCheckResult result = _activeObject.CanBePlacedAtPosition(_activeObject.transform.position);
            if(!result.IsValid)
            {
                onEvent?.Invoke(Events.CantPlaceObject, result);
                return;
            }

            if (_placeholder)
            {
                LevelObject lvlObj = Instantiate(
                    _placeholder.Profile.Prefab,
                    _placeholder.transform.position,
                    _placeholder.transform.rotation,
                    Controller.CurrentLevel.transform
                ).GetComponent<LevelObject>();
                lvlObj.InitializeData(new LevelObjectData(lvlObj));
                onEvent?.Invoke(Events.PlaceObject, lvlObj);
            }
            else
            {
                EndControlling();
                onEvent?.Invoke(Events.PlaceObject, _activeObject);
            }
        }
        public LevelObject PlaceObjectNetworked()
        {
            ConstraintCheckResult result = _activeObject.CanBePlacedAtPosition(_activeObject.transform.position);
            if (!result.IsValid)
            {
                onEvent?.Invoke(Events.CantPlaceObject, result);
                return null;
            }

            if (_placeholder)
            {
                return _placeholder;
            }
            else
            {
                EndControlling();
                onEvent?.Invoke(Events.PlaceObject, _activeObject);
            }
            return null;
        }

        public void PlaceObjectEmitEvent(LevelObject lvlObj)
        {
            onEvent?.Invoke(Events.PlaceObject, lvlObj);
        }

        public void CancelObject()
        {
            if (_activeObject)
            {
                if (_placeholder)
                {
                    DestroyPlaceholder();
                    DeselectObject();
                }
                else
                {
                    if (_currentState == State.Controlling)
                    {
                        _activeObject.InitializeData(_activeObjectOriginalData);
                        EndControlling();
                    }
                    else
                        DeselectObject();
                }
            }
        }

        public void DeselectObject()
        {
            if(_currentState == State.VertexSnapping || _currentState == State.SelectingVertex)
                EndVertexSnapping();
            if (_activeObject) _activeObject.Deselect();
            
            onEvent?.Invoke(Events.DeselectObject, _activeObject);
            _activeObject = null;
            _activeObjectOriginalData = null;
            EndControlling();
        }
        
        #endregion

        #region Manipulation Methods
        void ClampPosition(ref Vector3 pos)
        {
            if (!Controller.CurrentLevel) return;
            
            Vector3 minPoint = Controller.CurrentLevel.GetMinPoint();
            Vector3 maxPoint = Controller.CurrentLevel.GetMaxPoint();

            pos.x = Mathf.Clamp(pos.x, minPoint.x, maxPoint.x);
            pos.y = Mathf.Clamp(pos.y, minPoint.y, maxPoint.y);
            pos.z = Mathf.Clamp(pos.z, minPoint.z, maxPoint.z);
        }
        
        public void ChangeHeight(float direction)
        {
            float newHeight = _currentHeight + direction * (Controller.Settings.defaultPlacementMode == Controller.PlacementMode.Grid
                ? Controller.Settings.cellSize
                : Controller.Settings.heightStep);
            SetHeight(newHeight);
        }

        public void SnapHeightToGrid()
        {
            int closestStep = (int)(_currentHeight / Controller.Settings.cellSize);
            SetHeight(closestStep*Controller.Settings.cellSize);
        }
        
        public void SetHeight(float height)
        {
            _currentHeight = Mathf.Clamp(height, 0, Controller.Settings.maxBuildHeight);
            _currentAdjustedPosition = new Vector3(_currentAdjustedPosition.x, _currentHeight, _currentAdjustedPosition.z);
            UpdatePosition(true);
        }

        public void ChangeRayDistance(float direction)
        {
            float newRayDistance = _rayDistance + direction * Controller.Settings.rayDistanceStep;
            SetRayDistance(newRayDistance);
        }

        public void SetRayDistance(float rayDistance)
        {
            _rayDistance = Mathf.Clamp(rayDistance, Controller.Settings.minRayDistance,
                Controller.Settings.maxRayDistance);
        }
        #endregion
        
        #region Marker methods
        void SetMarkers()
        {
            if (!Controller.Settings.showMarkers)
            {
                if(_marker)
                    _marker.gameObject.SetActive(false);
                if(_markersLine)
                    _markersLine.enabled = false;
                if (_levelMarker)
                    _levelMarker.gameObject.SetActive(false);
                return;
            }
            else
            {
                if(!_markersLine) _markersLine = GetComponentInChildren<LineRenderer>();
                if (!_markersLine) _markersLine = gameObject.AddComponent<LineRenderer>();
            }
                
            if (_currentState == State.SelectingVertex || _currentState == State.VertexSnapping)
            {
                _markersLine.enabled = false;
                _levelMarker.gameObject.SetActive(false);
                if (_currentVertex != null)
                {
                    _marker.gameObject.SetActive(true);
                    _marker.position = Utility.ConvertVertexToWorldSpace(_currentVertex.GameObject.transform, _currentVertex.Vertex);
                }
                else
                    _marker.gameObject.SetActive(false);
            }
            else
            {
                _markersLine.enabled = true;
                _levelMarker.gameObject.SetActive(true);
                _marker.gameObject.SetActive(true);
                
                _marker.position = _currentAdjustedPosition;
                _levelMarker.position = _currentPosition;
                _markersLine.SetPosition(0, _currentPosition);
                _markersLine.SetPosition(1, _currentAdjustedPosition);
            }
        }
        #endregion
        
        #region Grid Transform Methods
        void TransformToGrid(ref Vector3 point)
        {
            float closestDistance = -1;
            Vector3 closestPoint = point;
            if (_activeObject && _activeObject.Profile.HasGridPlacement(GridPlacement.Strict))
            {
                point = TransformToGrid_Strict(point);
                return;
            }
            if (!_activeObject || _activeObject.Profile.HasGridPlacement(GridPlacement.Center))
            {
                
                Vector3 _point = TransformToGrid_Corner(point);
                float d = Vector3.Distance(point, _point);
                if (closestDistance == -1 || d < closestDistance)
                {
                    closestDistance = d;
                    closestPoint = _point;
                }
            }
            if (_activeObject && _activeObject.Profile.HasGridPlacement(GridPlacement.Corner))
            {
                
                Vector3 _point = TransformToGrid_Center(point);
                float d = Vector3.Distance(point, _point);
                if (closestDistance == -1 || d < closestDistance)
                {
                    closestDistance = d;
                    closestPoint = _point;
                }
            }
            if (_activeObject && _activeObject.Profile.HasGridPlacement(GridPlacement.Side))
            {
                Vector3 _point = TransformToGrid_Side(point);
                float d = Vector3.Distance(point, _point);
                if (closestDistance == -1 || d < closestDistance)
                {
                    closestDistance = d;
                    closestPoint = _point;
                }
            }
            point = closestPoint;
        }
        
        /// <summary>
        /// Transforms a point to closest grid corner
        /// based on the current grid placement mode
        /// </summary>
        /// <param name="point"></param>
        Vector3 TransformToGrid_Corner(Vector3 point)
        {
            Vector3 newPoint = point;
            Level currentLevel = Controller.CurrentLevel;

            float x = Mathf.RoundToInt(point.x / Controller.Settings.cellSize);
            newPoint.x = x * Controller.Settings.cellSize;

            float z = Mathf.RoundToInt(point.z / Controller.Settings.cellSize);
            newPoint.z = z * Controller.Settings.cellSize;

            newPoint = ApplyOffset(newPoint, point);
            return newPoint;
        }

        /// <summary>
        /// Applys an offset to a grid position
        /// This is used because a grid may be off-center
        /// If a grid's position isn't integer based Ex: (0.1, 0, 0.2)
        /// </summary>
        /// <param name="point"></param>
        /// <param name="comparePoint"></param>
        /// <returns></returns>
        Vector3 ApplyOffset(Vector3 point, Vector3 comparePoint)
        {
            Vector3 levelPos = Controller.CurrentLevel.transform.position;
            float offsetX = levelPos.x % 1;
            float offsetZ = levelPos.z % 1;
            List<Vector3> offsets = new List<Vector3>()
            {
                point + offsetX * Vector3.right + offsetZ * Vector3.forward,
                point + offsetX * Vector3.right + (1-offsetZ) * Vector3.back,
                point + (1-offsetX) * Vector3.left + offsetZ * Vector3.forward,
                point + (1-offsetX) * Vector3.left + (1-offsetZ) * Vector3.back
            };
            
            offsets.Sort((offset1, offset2) => Vector3.Distance(offset1, comparePoint) < Vector3.Distance(offset2, comparePoint) ? -1 : 1);
            return offsets[0];
        }

        /// <summary>
        /// Transforms a point to the center of the closest grid cell (Height independent)
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        Vector3 TransformToGrid_Center(Vector3 point)
        {
            Vector3 corner = TransformToGrid_Corner(point);
            float cellSize = Controller.Settings.cellSize;

            Vector3[] centers = new[]
            {
                corner + (0.5f)*cellSize* new Vector3(1,0,1),
                corner + (0.5f)*cellSize* new Vector3(1, 0, -1),
                corner + (0.5f)*cellSize* new Vector3(-1, 0, 1),
                corner + (0.5f)*cellSize* new Vector3(-1, 0, -1),
            };
            centers.Sort((center1, center2) => Vector3.Distance(center1, point) < Vector3.Distance(center2, point) ? -1 : 1);
            
            return centers[0];
        }

        /// <summary>
        /// Transforms a point to the closest side of a grid cell (Height independent)
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        Vector3 TransformToGrid_Side(Vector3 point, Vector3[] directions = default)
        {
            Vector3 center = TransformToGrid_Center(point);

            float cellSize = Controller.Settings.cellSize;

            List<Vector3> sides = new List<Vector3>();
            
            if (directions != default)
            {
                foreach(var d in directions)
                    sides.Add(center + (0.5f)*cellSize*d.normalized);
            }
            else
            {
                sides.AddRange(new[]{
                    center + (0.5f)*cellSize*Vector3.forward,
                    center + (0.5f)*cellSize*Vector3.back,
                    center + (0.5f)*cellSize*Vector3.right,
                    center + (0.5f)*cellSize*Vector3.left,
                });
            }

            sides.Sort((side1, side2) => Vector3.Distance(side1, point) < Vector3.Distance(side2, point) ? -1 : 1);
            
            return sides[0];
        }

        Vector3 TransformToGrid_Strict(Vector3 point)
        {
            if (!_activeObject) return TransformToGrid_Center(point);
            
            Vector3 size = _activeObject.Profile.Size;
            if (size.x % 2 == 1 && size.z % 2 == 1)
                return TransformToGrid_Center(point);
            else if (size.x % 2 == 0 && size.z % 2 == 0)
                return TransformToGrid_Corner(point);
            else
            {
                float orientation = _activeObject.transform.eulerAngles.y % 180f;
                if (Mathf.RoundToInt(size.z) % 2 != 0)
                {
                    return TransformToGrid_Side(point,
                        new[]
                        {
                            orientation != 0 ? Vector3.forward : Vector3.right,
                            orientation != 0 ? Vector3.back : Vector3.left
                        });
                }
                else
                {
                    return TransformToGrid_Side(point,
                        new[]
                        {
                            orientation != 0 ? Vector3.right : Vector3.forward,
                            orientation != 0 ? Vector3.left : Vector3.back
                        });
                }
            }

        }
        #endregion

        #region Vertex Snapping
        void BeginSelectingVertex()
        {
            if (!Controller.Settings.enableVertexSnapping) return;
            
            SetState(State.SelectingVertex);
        }

        void SelectVertex()
        {
            if (!Controller.Settings.enableVertexSnapping) return;

            RaycastVertexResult result = Utility.RaycastClosestVertex(
                Camera.main.ScreenPointToRay(_inputData.MousePosition),
                1,
                _rayDistance,
                _activeObject.GetComponentsInChildren<MeshFilter>());
            
            if (result.HasResult)
            {
                _currentVertex = result;
                onEvent?.Invoke(Events.SelectVertex, _currentVertex);
            }
            else
                _currentVertex = null;
            SetMarkers();
        }

        void BeginVertexSnapping()
        {
            if (!Controller.Settings.enableVertexSnapping) return;
            
            if(_currentVertex != null)
                SetState(State.VertexSnapping);
        }
        void UpdateVertexPosition()
        {
            if (!Controller.Settings.enableVertexSnapping) return;
            
            //Logic to find the closest vertex to the raycast
            RaycastVertexResult result = Utility.RaycastClosestVertex(
                Camera.main.ScreenPointToRay(_inputData.MousePosition),
                1,
                _rayDistance,
                _activeObject.GetComponentsInChildren<MeshFilter>(),
                true);
            
            //If there is a vertex, attach the selected vertex to it
            if (result.HasResult)
            {
                Vector3 resultGlobal = Utility.ConvertVertexToWorldSpace(result.GameObject.transform, result.Vertex);
                _activeObject.transform.position = resultGlobal;
                Vector3 currentGlobal = Utility.ConvertVertexToWorldSpace(_currentVertex.GameObject.transform, _currentVertex.Vertex);
                _activeObject.transform.position += _activeObject.transform.position - currentGlobal;
                SetMarkers();
            }
        }
        
        void EndVertexSnapping()
        {
            if (!Controller.Settings.enableVertexSnapping) return;
            
            _currentVertex = null;
            SetMarkers();
            SetState(State.Selected);
            onEvent?.Invoke(Events.EndVertexSnapping, null);
        }
        #endregion

        #region Enumerators
        public enum State
        {
            Idle,
            Selected,
            Controlling,
            SelectingVertex,
            VertexSnapping,
            SurfaceSnapping,
        }

        public enum Events
        {
            SelectObject,
            CreatePlaceholder,
            BeginControlling,
            EndControlling,
            PlaceObject,
            CantPlaceObject,
            DeselectObject,
            UpdateObject,
            UpdatePosition,
            ChangeState,
            BeginVertexSnapping,
            SelectVertex,
            EndVertexSnapping,
        }
        #endregion
    }
}
