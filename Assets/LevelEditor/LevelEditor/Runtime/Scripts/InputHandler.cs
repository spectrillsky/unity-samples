using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR;

namespace LFG.LevelEditor
{
    public class InputHandler : MonoBehaviour
    {
        [SerializeField] private InputData _inputData;
        public InputData Data { get { return _inputData; } }

        [SerializeField] private PlayerInput _playerInput;
        
        public class OnInputEvent : UnityEvent<DiscreteEvents, object> { };
        public OnInputEvent OnDiscreteInput = new OnInputEvent();
        
        public class OnInputStreamEvent : UnityEvent<StreamEvents, object>{ };
        public OnInputStreamEvent OnInputStream = new OnInputStreamEvent();
        
        
        [SerializeField] private EventSystem _eventSystem;
        [SerializeField] private Camera _camera;
        
        void Start()
        {
            if(!_camera)
                SetCamera(Camera.main);

            _eventSystem = FindObjectOfType<EventSystem>();
            if (!_eventSystem)
                _eventSystem = new GameObject("EventSystem", typeof(EventSystem)).GetComponent<EventSystem>();
        }

        public void SetCamera(Camera camera)
        {
            _camera = camera;
        }

        void CaptureInput()
        {
            _inputData.MousePosition = _playerInput.actions["MousePosition"].ReadValue<Vector2>();
            
            _inputData.BeginSelect = _playerInput.actions["Select"].triggered;
            _inputData.Select = _playerInput.actions["Select"].triggered;
            _inputData.EndSelect = _playerInput.actions["Select"].WasReleasedThisFrame();

            _inputData.Cancel = _playerInput.actions["Cancel"].triggered;
            
            _inputData.VertexSnapping = _playerInput.actions["VertexSnapping"].inProgress;
            _inputData.BeginSelectVertex = _playerInput.actions["SelectVertex"].triggered;
            _inputData.SelectVertex = _playerInput.actions["SelectVertex"].inProgress;
            _inputData.EndSelect = _playerInput.actions["VertexSnapping"].WasReleasedThisFrame();
            
            _inputData.MoveVector = _playerInput.actions["Move"].ReadValue<Vector2>();

            _inputData.Rotate = _playerInput.actions["Rotate"].ReadValue<float>();

            _inputData.RotateObject = _playerInput.actions["RotateObject"].ReadValue<float>();
            _inputData.Scroll = _playerInput.actions["Scroll"].ReadValue<float>();
            _inputData.ShiftScroll = _playerInput.actions["ShiftScroll"].ReadValue<float>();

            _inputData.Next = _playerInput.actions["Next"].triggered;
            _inputData.Previous = _playerInput.actions["Previous"].triggered;

            _inputData.MouseDelta = _playerInput.actions["MouseDelta"].ReadValue<Vector2>();
            _inputData.MouseDrag = _playerInput.actions["MouseDrag"].ReadValue<Vector2>();

            _inputData.HoldingRightClick = _playerInput.actions["HoldRightClick"].inProgress;
        }
        
        void Update()
        {
            
            if (Controller.CurrentState == Controller.State.Editing)
            {
                CaptureInput();
                HandleDiscreteInputs();
                HandleStreamInputs();
            }
        }

        void HandleDiscreteInputs()
        {
            
            #region Next/Previous
                if(_inputData.Next)
                    OnDiscreteInput.Invoke(DiscreteEvents.Previous,null);
                else if(_inputData.Previous)
                    OnDiscreteInput.Invoke(DiscreteEvents.Next,null);
            #endregion

            #region Selection
            if (!IsCursorOverUI())
            {
                if (_inputData.BeginSelect)
                    OnDiscreteInput.Invoke(DiscreteEvents.SelectDown,null);
                else if(_inputData.EndSelect)
                    OnDiscreteInput.Invoke(DiscreteEvents.SelectUp, null);
                if(_inputData.Cancel)
                    OnDiscreteInput.Invoke(DiscreteEvents.Cancel,null);
            }
            #endregion
            
            #region Vertex Snapping
            if (_inputData.BeginSelectVertex)
                OnDiscreteInput.Invoke(DiscreteEvents.BeginSelectingVertex, null);
            else if (_inputData.SelectVertex)
            {
                if(_inputData.EndSelect)
                    OnDiscreteInput.Invoke(DiscreteEvents.BeginVertexSnapping, null);
                else if(_inputData.EndSelect)
                    OnDiscreteInput.Invoke(DiscreteEvents.EndVertexSnapping, null);
            }
            else if(_inputData.EndVertexSnapping)
                OnDiscreteInput.Invoke(DiscreteEvents.EndVertexSnapping, null);
            #endregion

            #region Mouse Scroll
                if (_inputData.RotateObject != 0)
                    OnDiscreteInput.Invoke(DiscreteEvents.SecondaryRotate, _inputData.RotateObject);
                else if (_inputData.ShiftScroll != 0)
                    OnDiscreteInput.Invoke(DiscreteEvents.SecondaryScroll, _inputData.ShiftScroll);
                else if(_inputData.Scroll != 0)
                    OnDiscreteInput.Invoke(DiscreteEvents.Scroll, _inputData.Scroll);
            #endregion
        }
        
        void HandleStreamInputs()
        {
            bool isSelecting = !IsCursorOverUI() && _inputData.BeginSelect;
            if (isSelecting)
            {
                Ray ray = _camera.ScreenPointToRay(_inputData.MousePosition);
                
                LevelObjectRaycastData levelObjectData = Utility.RaycastLevelObject(ray, Controller.Settings.maxRayDistance);
                if (levelObjectData.LevelObject)
                    OnDiscreteInput.Invoke(DiscreteEvents.SelectObject, levelObjectData);

                LevelObjectRaycastData gridData = Utility.RaycastLevelObject(ray, Controller.Settings.maxRayDistance, true);
                if(gridData.Level)
                    OnDiscreteInput.Invoke(DiscreteEvents.SelectLevel, gridData.Point);
            }

            #region Rotation
            if(_inputData.Rotate != 0)
                OnDiscreteInput.Invoke(DiscreteEvents.Rotate, _inputData.Rotate);
            #endregion
        }

        bool IsCursorOverUI()
        {
            return _eventSystem.IsPointerOverGameObject();
        }

        public enum DiscreteEvents
        {
            SelectDown,
            SelectUp,
            Cancel,
            Next,
            Previous,
            SelectLevel,
            SelectObject,
            Rotate,
            SecondaryRotate,
            Scroll,
            SecondaryScroll,
            BeginSelectingVertex,
            BeginVertexSnapping,
            EndVertexSnapping,
        }

        public enum StreamEvents
        {
            RaycastLevel,
            RaycastObject,
            Rotate,
            Move,
            PrimaryDrag,
            SecondaryDrag,
        }
    }
}

