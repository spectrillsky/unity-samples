using System.Collections;
using System.Collections.Generic;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LFG.LevelEditor.UI
{
    public class ValidationUI : MonoBehaviour
    {
        [SerializeField] private Button _toggleButton;
        [SerializeField] private GameObject _listPanel;
        [SerializeField] private Transform _list;
        [SerializeField] private TextMeshProUGUI _textInvalidCount;
        [SerializeField] private Image _toggleButtonImage;
        [SerializeField] private Color _validColor, _invalidColor;
        
        [SerializeField] private Image _statusImage;
        [SerializeField] private Sprite _validSprite, _invalidSprite;

        [SerializeField] private GameObject _validationListItemPrefab;

        private LevelValidator.Result _result;
        
        void Awake()
        {
            Controller.OnAction.AddListener(OnControllerAction);
            LevelValidator.Event.AddListener(OnLevelValidatorEvent);
            _toggleButton.onClick.AddListener(ToggleList);
        }

        private void OnLevelValidatorEvent(LevelValidator.Event obj)
        {
            if(obj.Type == LevelValidator.EventType.ValidationResult)
                Initialize((LevelValidator.Result)obj.Context);
        }

        private void OnControllerAction(Controller.Actions action, object context)
        {
            if (action == Controller.Actions.SetLevel)
            {
                Initialize(Controller.CurrentLevel.Validator.LastValidationResult);
            }
            else if (action == Controller.Actions.ChangeState)
            {
                Controller.State state = (Controller.State)context;
                if(state == Controller.State.Editing)
                    Initialize(Controller.CurrentLevel.Validator.LastValidationResult);
                else
                    Deinitialize();
            }
        }

        private void Initialize(LevelValidator.Result result)
        {
            _result = result;
            SetInvalidCount(result.InvalidObjectValidators.Count);
            _statusImage.sprite = result.IsValid() ? _validSprite : _invalidSprite;
            _toggleButtonImage.color = result.IsValid() ? _validColor : _invalidColor;
            if (result.IsValid())
            {
                ClearList();
                HideList();
            }
            else
            {
                PopulateList();
                ShowList();
            }
            
        }

        private void Deinitialize()
        {
            ClearList();
            HideList();
            SetInvalidCount(0);
            _statusImage.sprite =  _validSprite;
        }

        void PopulateList()
        {
            ClearList();
            foreach (var validator in _result.InvalidObjectValidators)
            {
                ValidationListItem item = Instantiate(_validationListItemPrefab, _list)
                    .GetComponent<ValidationListItem>();
                item.Initialize(validator);
            }
        }

        void SetInvalidCount(int count)
        {
            _textInvalidCount.enabled = count != 0;
            _textInvalidCount.text = count.ToString();
        }

        void ToggleList()
        {
            if (_result.IsValid()) return;
            
            if(_listPanel.activeInHierarchy) HideList();
            else ShowList();
        }
        
        void ShowList()
        {
            _listPanel.SetActive(true);
        }

        void HideList()
        {
            _listPanel.SetActive(false);
        }

        void ClearList()
        {
            foreach(var child in _list.GetComponentsInChildren<ValidationListItem>())
                Destroy(child.gameObject);
        }
    }
}
