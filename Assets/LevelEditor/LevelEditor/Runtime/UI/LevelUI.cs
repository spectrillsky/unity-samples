using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LFG.LevelEditor.UI
{
    public class LevelUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_InputField nameField;
        [SerializeField] private TextMeshProUGUI currentSizeText;
        
        [SerializeField] private Slider xSlider, ySlider, zSlider;
        [SerializeField] private TMP_InputField xInput, yInput, zInput;
        
        [SerializeField] private Button
            saveButton,
            unloadButton,
            deleteButton,
            setSizeButton,
            copyButton;

        private Level _currentLevel;
        
        void Awake()
        {
            Controller.OnAction?.AddListener(OnLevelEditorAction);
            
            saveButton.onClick.AddListener(OnSaveClicked);
            unloadButton.onClick.AddListener(OnUnloadClicked);
            deleteButton.onClick.AddListener(OnDeleteClicked);
            setSizeButton.onClick.AddListener(OnSetSizeClicked);
            copyButton.onClick.AddListener(OnCopyClicked);

            xSlider.wholeNumbers = ySlider.wholeNumbers = zSlider.wholeNumbers = true;

            xInput.onDeselect.AddListener((value) => ValidateSizeInput(value, "x"));
            yInput.onDeselect.AddListener((value) => ValidateSizeInput(value, "y"));
            zInput.onDeselect.AddListener((value) => ValidateSizeInput(value, "z"));
            
            xSlider.onValueChanged.AddListener((value) => OnSizeChanged_Slider(value, "x"));
            ySlider.onValueChanged.AddListener((value) => OnSizeChanged_Slider(value, "y"));
            zSlider.onValueChanged.AddListener((value) => OnSizeChanged_Slider(value, "z"));
        }
        
        void OnLevelEditorAction(Controller.Actions action, object context)
        {
            if(action == Controller.Actions.SetLevel)
                Init((Level)context);
        }

        void Init(Level level)
        {
            _currentLevel = level;
            if (level)
            {
                LevelData data = level.GetData();
                nameField.text = $"{data.DisplayName}";
                currentSizeText.text = $"Size: W:{data.Size.x} L:{data.Size.z} H:{data.Size.y} ";
                Show();
                InitSizeSliders(data.Size);
            }
            else
                Hide();
        }

        #region Size Logic
        void InitSizeSliders(Vector3 size)
        {
            xSlider.minValue = Controller.Settings.minimumLevelSize.x;
            xSlider.maxValue = Controller.Settings.maximumLevelSize.x;
            xSlider.value = size.x;
            
            ySlider.minValue = Controller.Settings.minimumLevelSize.y;
            ySlider.maxValue = Controller.Settings.maximumLevelSize.y;
            ySlider.value = size.y;
            
            zSlider.minValue = Controller.Settings.minimumLevelSize.z;
            zSlider.maxValue = Controller.Settings.maximumLevelSize.z;
            zSlider.value = size.z;
        }
        
        void OnSizeChanged_Slider(float value, string component)
        {
            Vector3 minSize = Controller.Settings.minimumLevelSize;
            Vector3 maxSize = Controller.Settings.maximumLevelSize;
            
            if (component == "x")
            {
                xSlider.value = Mathf.RoundToInt(value);
                xInput.text = value.ToString();
            }
            else if (component == "y")
            {
                ySlider.value = Mathf.RoundToInt(value);
                yInput.text = value.ToString();
            }
            else if (component == "z")
            { 
                zSlider.value = Mathf.RoundToInt(value);
                zInput.text = value.ToString();
            }
        }

        void ValidateSizeInput(string value, string component)
        {
            if (Int32.TryParse(value, out int intValue))
            {
                Vector3 minSize = Controller.Settings.minimumLevelSize;
                Vector3 maxSize = Controller.Settings.maximumLevelSize;
                if (component == "x")
                {
                    intValue = Mathf.Clamp(intValue, (int)minSize.x, (int)maxSize.x);
                    xSlider.value = intValue;
                    xInput.text = intValue.ToString();
                }
                else if (component == "y")
                {
                    intValue = Mathf.Clamp(intValue, (int)minSize.y, (int)maxSize.y);
                    ySlider.value = intValue;
                    yInput.text = intValue.ToString();
                }
                else if (component == "z")
                { 
                    intValue = Mathf.Clamp(intValue, (int)minSize.z, (int)maxSize.z);
                    zSlider.value = intValue;
                    zInput.text = intValue.ToString();
                }                    
            }
                
        }

        void OnSetSizeClicked()
        {
            Vector3 newSize = new Vector3(xSlider.value, ySlider.value, zSlider.value);
            Debug.Log(newSize);
            _currentLevel.SetSize(newSize);
        }
        #endregion
        
        void Show()
        {
            panel.SetActive(true);
        }

        void Hide()
        {
            panel.SetActive(false);
        }

        #region Button Events
        void OnSaveClicked()
        {
            if (_currentLevel)
            {
                LevelData data = _currentLevel.GetData();
                data.DisplayName = nameField.text;
                Controller.instance.SaveLevel(_currentLevel);
            }
        }

        void OnUnloadClicked()
        {
            if(_currentLevel)
                Controller.instance.UnloadLevel(_currentLevel);
        }

        void OnDeleteClicked()
        {
            if (_currentLevel)
            {
                Controller.instance.DeleteLevel(_currentLevel);
                Controller.instance.UnloadLevel(_currentLevel);
            }
                
        }

        void OnCopyClicked()
        {
            if (_currentLevel)
                Controller.instance.CopyLevel(_currentLevel.GetData());
        }
        #endregion

    }
}