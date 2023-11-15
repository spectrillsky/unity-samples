using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LFG.LevelEditor.UI
{
    public class LevelListItem : MonoBehaviour
    {
        public static LevelData Selected; 
        
        public Button selectButton;
        [SerializeField] private TextMeshProUGUI levelName;
        
        private LevelData _levelData;

        public void Init(LevelData levelData)
        {
            levelName.text = levelData.DisplayName;
            _levelData = levelData;
            selectButton.onClick.AddListener(() => Selected = _levelData);
            
        }

        public void Select()
        {
            Selected = _levelData;
        }
    }
}
