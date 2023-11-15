using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LFG.LevelEditor.UI
{
    public class GalleryFilterItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameLabel;
        [SerializeField] private Button _button;
        [SerializeField] private GameObject _selectedGraphic;
        
        public LevelObjectType Type { get; private set; }

        public void Init(LevelObjectType type = null)
        {
            Type = type;
            _nameLabel.text = type ? type.DisplayName : "All";
            _button.onClick.AddListener(() => LevelObjectGalleryUI.instance.SetFilterType(Type));
        }

        public void SetSelected(bool isSelected)
        {
            _selectedGraphic.SetActive(isSelected);
        }
    }
}
