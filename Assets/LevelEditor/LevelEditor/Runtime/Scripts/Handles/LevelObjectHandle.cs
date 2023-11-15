using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LFG.LevelEditor
{
    public class LevelObjectHandle : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Camera _camera;

        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private Image _handle;

        public LevelObject LevelObject;
        private LevelObjectValidator _validator;
        
        private void OnEnable()
        {
            // _camera = HandleCamera.GetCamera();
            // GetComponent<Canvas>().worldCamera = _camera;
            _camera = Camera.main;
            GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            GetComponent<Canvas>().sortingOrder = -1;
            
            Cursor.instance.onEvent.AddListener(OnCursorEvent);
            
        }

        void OnCursorEvent(Cursor.Events e, object context)
        {
            if (e == Cursor.Events.SelectObject)
                SetColor();
            else if(e == Cursor.Events.DeselectObject)
                SetColor();
        }
        
        void Update()
        {
            SetPosition();
            LookAtCamera();
            SetFade();
        }
        
        public void Initialize(LevelObject levelObject)
        {
            LevelObject = levelObject;
            _label.text = levelObject.Profile.DisplayName;
            _validator = Controller.CurrentLevel.Validator.GetLevelObjectValidator(levelObject.Profile);
            LevelObjectValidator.Event.AddListener(OnValidatorEvent);
        }
        
        private void OnValidatorEvent(LevelObjectValidator.Event e)
        {
            if (e.Validator.LevelObject == LevelObject.Profile)
                SetColor();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Cursor.instance.SelectObject(LevelObject, false);
        }

        void LookAtCamera()
        {
            transform.forward = -_camera.transform.forward;
        }
        
        void SetPosition()
        {
            _handle.rectTransform.anchoredPosition = _camera.WorldToScreenPoint(LevelObject.transform.position);
        }

        void SetFade()
        {
            
        }

        void SetColor()
        {
            _handle.color = LevelObject.IsSelected ? Controller.Settings.HandleSelectedColor : Controller.Settings.HandleUnselectedColor;
            if (!LevelObject.IsSelected && _validator.CurrentStatus == LevelObjectValidator.Status.InvalidMax)
                _handle.color = Controller.Settings.HandleInvalidColor;
        }

        public void Show()
        {
            
        }

        public void Hide()
        {
            
        }
    }
}
