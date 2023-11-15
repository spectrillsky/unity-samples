using Sirenix.OdinInspector;
using UnityEngine;

namespace LFG.LevelEditor
{
    public class LevelEditorCamera : MonoBehaviour
    {
        [SerializeField] private Transform _cameraTarget;
        [SerializeField] private LevelEditorCameraData _data;
        [SerializeField] private InputData _inputData;

        private Mode _currentMode = Mode.Topdown;
        
        void Start()
        {
            ChangeMode(_currentMode);
        }

        void OnLevelEditorAction(Controller.Actions action, object context)
        {
            if (action == Controller.Actions.SetLevel)
            {
                Level level = (Level)context;
                Init(level);
            }
        }

        void Init(Level level)
        {
            SetTargetPosition(level.GetCenterPoint());
        }

        [Button]
        public void ChangeMode(Mode mode)
        {
            _currentMode = mode;
        }
        
        void Update()
        {
            if(_currentMode == Mode.Freelook && _inputData.HoldingRightClick)
                MoveVertical(_inputData.Rotate);
            
            Rotate(_inputData.Rotate);
            Rotate(_inputData.MouseDrag);
           
            Move(_inputData.MoveVector);
            
           
            Zoom(_inputData.ShiftScroll);
        }

        Vector3 GetLateralInput()
        {
            Vector3 lateralInput = Vector3.zero;
            if (Input.GetKey(KeyCode.W))
                lateralInput.z += 1;
            if (Input.GetKey(KeyCode.S))
                lateralInput.z -= 1;
            if (Input.GetKey(KeyCode.D))
                lateralInput.x += 1;
            if (Input.GetKey(KeyCode.A))
                lateralInput.x -= 1;
            return lateralInput;
        }
        
        [Button]
        void Reset()
        {
            if (_currentMode == Mode.Topdown)
                _cameraTarget.position = Controller.CurrentLevel.GetCenterPoint();
        }

        void Zoom(float input)
        {
            
        }

        void Rotate(float direction)
        {
            if (_currentMode == Mode.Topdown)
            {
                float degreesToRotate = direction * _data.RotationStep * Time.deltaTime;
                _cameraTarget.Rotate(degreesToRotate * Vector3.up, Space.World);
                SetTopdownPosition();
            }
        }

        void Rotate(Vector2 input)
        {
            if (_currentMode == Mode.Freelook)
            {
                float xAngle = transform.eulerAngles.x;
                if (xAngle > 90) xAngle -= 360;
                transform.eulerAngles = new Vector3(
                    Mathf.Clamp(xAngle - _data.RotationStep * input.y * Time.deltaTime, -89.99f, 89.99f),
                    transform.eulerAngles.y + _data.RotationStep * input.x * Time.deltaTime,
                    0);
            }
        }

        void Move(Vector2 input)
        {
            if (_currentMode == Mode.Topdown)
            {
                Vector3 direction = Quaternion.Euler(0, transform.eulerAngles.y, 0) * (input.x * Vector3.right + input.y * Vector3.forward);
                _cameraTarget.position += _data.Speed * Time.deltaTime * direction;
                SetTopdownPosition();
            }
            else if (_currentMode == Mode.Freelook)
            {
                Vector3 direction = new Vector3(input.x, 0, input.y);
                transform.position += _data.Speed * transform.TransformDirection(direction) * Time.deltaTime;
            }
        }

        void MoveVertical(float input)
        {
            if (_currentMode == Mode.Freelook)
            {
                Vector3 direction = input * Vector3.up;
                transform.position += _data.Speed * transform.TransformDirection(direction) * Time.deltaTime;
            }
        }

        void SetTargetPosition(Vector3 position)
        {
            _cameraTarget.position = position;
        }

        void SetTopdownPosition()
        {
            transform.position = _cameraTarget.position + Quaternion.Euler(0, _cameraTarget.eulerAngles.y, 0) * _data.FollowVector;
            transform.LookAt(_cameraTarget.position);
        }

        void Clamp()
        {
            var size = Controller.CurrentLevel.Size;
            Vector3 minPoint = Controller.CurrentLevel.GetMinPoint();
            Vector3 maxPoint = Controller.CurrentLevel.GetMaxPoint();
            _cameraTarget.position = new Vector3(
                Mathf.Clamp(_cameraTarget.position.x, minPoint.x, maxPoint.x),
                Mathf.Clamp(_cameraTarget.position.y, minPoint.y, maxPoint.y),
                Mathf.Clamp(_cameraTarget.position.z, minPoint.z, maxPoint.z));
        }
        
        #region Event Listeners

        void OnDiscreteInput(InputHandler.DiscreteEvents e, object context)
        {
            
        }

        void OnInputStream(InputHandler.StreamEvents e, object context)
        {
            if(e == InputHandler.StreamEvents.SecondaryDrag)
                Rotate((UnityEngine.Vector2)context);
        }
        #endregion

        public enum Mode
        {
            Topdown,
            Freelook,
            Override,
        }
    }
}
