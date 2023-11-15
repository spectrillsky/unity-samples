using System.Collections;
using System.Collections.Generic;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LFG.LevelEditor.UI
{
    public class MenuUI : MonoBehaviour
    {
        [SerializeField] private Button selectLevelButton;
        [SerializeField] private TextMeshProUGUI selectedLevelName;
        [SerializeField] private Button loadLevelButton;

        #region Create
        [SerializeField] private TMP_InputField createLevelInput;
        [SerializeField] private Button createLevelButton;

        [SerializeField] private Button _selectLayoutButton;
        [SerializeField] private TextMeshProUGUI _selectedLayoutName;
        #endregion

        [SerializeField] private LayoutList _layoutList;
        [SerializeField] private LevelListUI _levelList;

        private LevelData selectedLevel;
        
        void Start()
        {
            _levelList.onLevelSelected.AddListener(SelectLevel);
            selectLevelButton.onClick.AddListener(OnClick_SelectLevel);
            loadLevelButton.onClick.AddListener(OnClick_LoadLevel);
            
            _layoutList.onLayoutSelected.AddListener(SelectLayout);
            _selectLayoutButton.onClick.AddListener(OnClick_SelectLayout);
            createLevelButton.onClick.AddListener(OnClick_CreateLevel);

            
            ClearInput_CreateLevel();
        }

        void SelectLevel(LevelData levelData)
        {
            selectedLevel = levelData;
            selectedLevelName.text = levelData.DisplayName;
        }

        void SelectLayout(Level layout)
        {
            _selectedLayoutName.text = layout.Layout;
        }
        
        void ClearSelectedLevel()
        {
            selectedLevel = null;
        }

        void SetLoadButton()
        {
            loadLevelButton.gameObject.SetActive(selectedLevel);
        }
        
        void OnClick_SelectLevel()
        {
            if(!_levelList.IsActive)
                _levelList.Show();
            else
                _levelList.Hide();
        }

        void OnClick_SelectLayout()
        {
            if(!_layoutList.IsActive)
                _layoutList.Show();
            else
                _layoutList.Hide();
        }
        
        void OnClick_CreateLevel()
        {
            if (createLevelInput.text.IsNullOrWhitespace()) return;
            if (!LayoutListItem.Selected) return;
            if (createLevelInput.text.Length < 3) return; // >.> don't @ me
            
            LevelData levelData = Controller.instance.CreateLevel(createLevelInput.text, LayoutListItem.Selected);
            Controller.instance.LoadLevel(levelData);
            ClearInput_CreateLevel();
        }

        void ClearInput_CreateLevel()
        {
            createLevelInput.text = "New Level Name";
        }

        void OnClick_LoadLevel()
        {
            if (selectedLevel)
            {
                Controller.instance.LoadLevel(selectedLevel);
            }
        }
    }
}
