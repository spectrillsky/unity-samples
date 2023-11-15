using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LFG.LevelEditor.UI
{
    public class LayoutListItem : MonoBehaviour
    {
        public static Level Selected;
        
        [field:SerializeField] public Button SelectButton { get; private set; }
        [SerializeField] private TextMeshProUGUI _layoutName;
        
        [field:SerializeField] public Level Layout { get; private set; }

        public void Initialize(Level layout)
        {
            _layoutName.text = layout.Layout;
            Layout = layout;
            SelectButton.onClick.AddListener(Select);
        }

        public void Select()
        {
            Selected = Layout;
        }
    }
}
