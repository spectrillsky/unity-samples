using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LFG.LevelEditor.UI
{
    public class GalleryItem : MonoBehaviour
    {
        #region Components
        [SerializeField] private TextMeshProUGUI _nameLabel;
        [SerializeField] private Button _button;
        [SerializeField] private GameObject _selectedGraphic;
        [SerializeField] private TextMeshProUGUI _instancesCountText;
        [SerializeField] private TextMeshProUGUI _minCountText;
        [SerializeField] private GameObject _invalidGraphic;
        #endregion
        
        #region Members
        private LevelObjectProfile _levelObject;
        private LevelObjectValidator _validator;
        #endregion
        
        public void Init(LevelObjectProfile lvlObj)
        {
            _nameLabel.text = lvlObj.DisplayName;
            _levelObject = lvlObj;
            _button.onClick.AddListener(() => Controller.instance.SetResource(lvlObj));
            Controller.OnAction?.AddListener(OnLevelEditorAction);
            SetValidator();
            SetSelected(Controller.instance.currentResource == _levelObject);
        }

        void SetValidator()
        {
            if (!Controller.CurrentLevel) return;
            
            _validator = Controller.CurrentLevel.Validator.GetLevelObjectValidator(_levelObject);
            
            LevelObjectValidator.Event.AddListener(OnValidatorEvent);
            SetInstanceCount();
            SetInvalid();
        }

        private void OnValidatorEvent(LevelObjectValidator.Event obj)
        {
            if (obj.Validator != _validator) return;
            
            SetInvalid();
            SetInstanceCount();
        }

        void OnLevelEditorAction(Controller.Actions action, object context)
        {
            if (action == Controller.Actions.SetResource)
                SetSelected((LevelObjectProfile)context == _levelObject);
            else if (action == Controller.Actions.SetLevel)
            {
                SetValidator();
            }
        }

        void SetSelected(bool isSelected)
        {
            _selectedGraphic.SetActive(isSelected);
        }

        void SetInstanceCount()
        {
            if (_levelObject.HasMaxInstances())
                _instancesCountText.text = $"{_validator.CurrentCount}/{_levelObject.MaxInstances}";
            else
                _instancesCountText.text = $"{_validator.CurrentCount}";

            if (_levelObject.MinInstances != 0)
            {
                _minCountText.enabled = true;
                _minCountText.text = $"Min: {_levelObject.MinInstances}";
            }
            else
                _minCountText.enabled = false;
        }

        void SetInvalid()
        {
            bool isInvalid = _validator.CurrentStatus != LevelObjectValidator.Status.Valid;
            _invalidGraphic.SetActive(isInvalid);
        }
    }
}
