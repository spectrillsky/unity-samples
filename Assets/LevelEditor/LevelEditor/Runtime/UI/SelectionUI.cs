using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LFG.LevelEditor.UI
{
    public class SelectionUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI nameLabel;
        [SerializeField] private TextMeshProUGUI typeLabel;
        [SerializeField] private TextMeshProUGUI positionLabel;

        [SerializeField] private Button clearButton,
            toggleControlButton,
            deleteButton;

        [SerializeField] private TextMeshProUGUI toggleControlLabel;
        
        private LevelObject _selection;

        [FoldoutGroup("Cursor Modes")] [SerializeField]
        private Button
            idleModeButton,
            defaultModeButton,
            vertexSnappingModeButton,
            surfaceSnappingModeButton;

        void Start()
        {
            Controller.instance.Cursor.onEvent.AddListener(OnCursorEvent);
            
            clearButton.onClick.AddListener(OnClearButtonClicked);
            toggleControlButton.onClick.AddListener(OnToggleControlButtonClicked);
            deleteButton.onClick.AddListener(OnDeleteClicked);
            
            // idleModeButton.onClick.AddListener(() => OnPlacementModeButtonClicked(Cursor.Mode.Idle));
            // defaultModeButton.onClick.AddListener(() => OnPlacementModeButtonClicked(Cursor.Mode.Placing));
            // vertexSnappingModeButton.onClick.AddListener(() => OnPlacementModeButtonClicked(Cursor.Mode.VertexSnapping));
            // surfaceSnappingModeButton.onClick.AddListener(() => OnPlacementModeButtonClicked(Cursor.Mode.Idle));
        }

        void Init(LevelObject selection)
        {
            _selection = selection;
            if (_selection)
            {
                nameLabel.text = _selection.name;
                typeLabel.text = string.Join(", ", _selection.Profile.Types.Select(t => t.name));
                SetPosition(_selection.transform.position);
                Show();
            }
            else
                Hide();

            SetButtons();
        }

        void SetPosition(Vector3 pos)
        {
            positionLabel.text = $"({Math.Round(pos.x, 2)}, {Math.Round(pos.y, 2)}, {Math.Round(pos.z, 2)})";
        }

        void SetButtons()
        {
            toggleControlButton.gameObject.SetActive(Controller.instance.Cursor.ActiveObject && !Controller.instance.Cursor.IsUsingPlaceholder);
            deleteButton.gameObject.SetActive(Controller.instance.Cursor.ActiveObject && !Controller.instance.Cursor.IsUsingPlaceholder);
    
            toggleControlLabel.text = Controller.instance.Cursor.CurrentState == Cursor.State.Controlling ? "End Control" : "Begin Control";
        }

        void Show()
        {
            panel.SetActive(true);
        }

        void Hide()
        {
            panel.SetActive(false);
        }

        #region Button Listeners
        void OnClearButtonClicked()
        {
            Controller.instance.ClearSelection();
        }

        void OnToggleControlButtonClicked()
        {
            if(Controller.instance.Cursor.CurrentState == Cursor.State.Controlling)
                Controller.instance.Cursor.EndControlling();
            else
                Controller.instance.Cursor.BeginControlling();
        }

        void OnDeleteClicked()
        {
            if(!Controller.instance.Cursor.IsUsingPlaceholder)
                Controller.instance.DeleteObject(_selection);
        }
        #endregion

        private void OnCursorEvent(Cursor.Events e, object context)
        {
            if (e == Cursor.Events.SelectObject)
            {
                Init((LevelObject)context);
            }
            else if (e == Cursor.Events.DeselectObject)
            {
                Init(null);
            }
            else if (e == Cursor.Events.BeginControlling)
            {
                SetButtons();
            }
            else if (e == Cursor.Events.EndControlling)
            {
                SetButtons();
            }
            else if(e == Cursor.Events.UpdateObject)
            {
                LevelObject obj = (LevelObject)context;
                SetPosition(obj.transform.position);
            }
            else if(e == Cursor.Events.UpdatePosition)
            {
                if (_selection)
                    SetPosition(_selection.transform.position);
            }

        }
    }
}
