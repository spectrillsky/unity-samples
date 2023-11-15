using System.Collections;
using System.Collections.Generic;
using LFG.LevelEditor;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LFG.LevelEditor.UI
{
    [RequireComponent(typeof(Button))]
    public class ToolbarButton : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private GameObject selected;

        [SerializeField] private Type _type;

        [ShowIf("_type", Type.BuildMode)]
        [SerializeField] private Controller.PlacementMode _mode;

        void OnValidate()
        {
            if (label) SetLabel();
        }

        void SetLabel()
        {
            string labelStr = _type.ToString();
            switch (_type)
            {
                case Type.BuildMode:
                    labelStr = _mode.ToString();                    
                    break;
            }
            label.text = labelStr;
        }
        
        void Awake()
        {
            GetComponent<Button>().onClick.AddListener(OnClicked);
            Controller.OnAction?.AddListener(OnLevelEditorAction);
        }
        
        void OnClicked()
        {
            switch (_type)
            {
                case Type.BuildMode:
                    Controller.instance.SetBuildMode(_mode);
                    break;
                case Type.Stack:
                    Controller.Settings.SetStacking(Controller.Settings.stackObjects);
                    break;
                case Type.Stick:
                    Controller.Settings.SetSticky(Controller.Settings.stickObjects);
                    break;
            }
        }

        void SetSelected(bool isSelected)
        {
            selected.SetActive(isSelected);
        }

        void OnLevelEditorAction(Controller.Actions action, object context)
        {
            if(action == Controller.Actions.ChangeMode && _type == Type.BuildMode)
                SetSelected(_mode == (Controller.PlacementMode)context);
            else if(action == Controller.Actions.SetStacking)
                SetSelected((bool)context);
        }

        public enum Type
        {
            BuildMode,
            Stack,
            Stick,
        }
    }
}
