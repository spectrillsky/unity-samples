using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LFG.LevelEditor.UI
{
    public class VertexMarker : MonoBehaviour
    {
        [SerializeField] private Canvas _panel;
        [SerializeField] private RectTransform _rectTransform;
        
        void Start()
        {
            Controller.instance.Cursor.onEvent.AddListener(OnCursorEvent);
            Hide();
        }

        void OnCursorEvent(Cursor.Events e, object context)
        {
            if (e == Cursor.Events.ChangeState)
            {
                Cursor.State state = (Cursor.State)context;
                if (state == Cursor.State.SelectingVertex)
                    Show();
                else
                    Hide();
            }
            else if (e == Cursor.Events.SelectVertex)
            {
                SetPosition((RaycastVertexResult)context);
            }
        }

        void SetPosition(RaycastVertexResult result)
        {
            _rectTransform.anchoredPosition = Camera.main.WorldToScreenPoint(Utility.ConvertVertexToWorldSpace(result.GameObject.transform, result.Vertex));
        }

        void Show()
        {
            _panel.enabled = true;
        }

        void Hide()
        {
            _panel.enabled = false;
        }
    }
}
