using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEditor;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace LFG.LevelEditor
{
    /// <summary>
    /// A class to assist in visualizing the 3d volume that a level object takes up
    /// </summary>
    [RequireComponent(typeof(LevelObject))]
    public class LevelObjectVolume : MonoBehaviour
    {
        [SerializeField] private Transform _volume;
        private List<LineRenderer> lineRenderers = new List<LineRenderer>(); 
        
        void CreateVolume()
        {
            _volume = transform.Find("Volume");
            if (!_volume) _volume = new GameObject("Volume").transform;
            _volume.SetParent(transform);
            _volume.transform.localPosition = Vector3.zero;
            UpdateVolume();

            DrawVolume();
        }

        void ClearCorners()
        {
            foreach(var lineRenderer in lineRenderers) Destroy(lineRenderer.gameObject);
            lineRenderers.Clear();
        }
        
        void DrawVolume()
        {
            ClearCorners();
            if (!Controller.Settings.drawVolumeCorners) return;
            DrawCorners(true);
            DrawCorners(false);
        }

        private float hatchSize = 0.25f;
        void DrawCorners(bool isTop)
        {
            Vector3 size = GetComponentInParent<LevelObject>().Size;

            Vector3 start = size/2;
            if (!isTop)
                start.y = -start.y;
            for (var i = 0; i < 4; i++)
            {
                LineRenderer lineRenderer = new GameObject($"corner{i}").AddComponent<LineRenderer>();
                lineRenderers.Add(lineRenderer);
                lineRenderer.transform.SetParent(transform);
                lineRenderer.transform.localPosition = Vector3.zero;
                
                lineRenderer.material = Controller.Settings.latticeMaterial;
                lineRenderer.useWorldSpace = false;
                lineRenderer.startWidth = lineRenderer.endWidth = 0.05f;
                lineRenderer.positionCount = 6;
                
                lineRenderer.SetPosition(0, start);
                lineRenderer.SetPosition(1, start + (i is 0 or 3 ? Vector3.left : Vector3.right) * size.x * hatchSize);
                lineRenderer.SetPosition(2, start);
                lineRenderer.SetPosition(3,  start + (i is < 2 ? Vector3.back : Vector3.forward) * size.z * hatchSize);
                lineRenderer.SetPosition(4, start);
                lineRenderer.SetPosition(5,  start + (isTop ? Vector3.down : Vector3.up) * size.y * hatchSize);
                if (i == 0)
                    start += size.x * Vector3.left;
                else if (i == 1)
                    start += size.z * Vector3.back;
                else if (i == 2)
                    start += size.x * Vector3.right;
            }
        }


        public void UpdateVolume()
        {
            if (!_volume) CreateVolume();
            var box = _volume.gameObject.GetComponent<BoxCollider>(); 
            if(!box) box = _volume.gameObject.AddComponent<BoxCollider>();

            box.size = GetComponent<LevelObject>().Size * (Controller.Settings.cellSize - Controller.Settings.volumePadding);
            box.center = Vector3.up * box.size.y / 2;
            _volume.gameObject.layer = LayerMask.NameToLayer("LevelObject");
            
            DrawVolume();
        }

        void DestroyVolume()
        {
            if(_volume)
                Destroy(_volume.gameObject);
        }
        
        void OnLevelEditorAction(Controller.Actions action, object context)
        {
            if (action == Controller.Actions.ChangeState)
            {
                Controller.State state = (Controller.State)context;
                if(state == Controller.State.Editing)
                    CreateVolume();
                else if (state == Controller.State.None)
                    DestroyVolume();
            }
        }
        void OnEnable()
        {
            Controller.OnAction?.AddListener(OnLevelEditorAction);
            CreateVolume();
        }

        private void OnDisable()
        {
            DestroyVolume();
        }

        private void OnDestroy()
        {
            DestroyVolume();
        }
    }
}
