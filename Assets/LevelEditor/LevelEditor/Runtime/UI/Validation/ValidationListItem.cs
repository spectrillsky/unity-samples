using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LFG.LevelEditor.UI
{
    public class ValidationListItem : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _textLabel;
        [SerializeField] private TextMeshProUGUI _textMessage;
        
        public LevelObjectValidator Validator { get; private set; }

        void Awake()
        {
            _button = GetComponentInChildren<Button>();
            _button.onClick.AddListener(OnClick);
        }
        
        public void Initialize(LevelObjectValidator lov)
        {
            _textLabel.text = lov.LevelObject.DisplayName;
            _textMessage.text = lov.CurrentStatus.ToString();
        }

        void OnClick()
        {
            
        }
    }
}
