using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace LFG.LevelEditor
{
    public class HandleCamera : MonoBehaviour
    {
        public static HandleCamera instance;

        private Camera _camera;
        
        void Awake()
        {
            instance = this;
            Initialize();
        }

        void Initialize()
        {
            _camera = GetComponentInChildren<Camera>();
            if (!_camera) _camera = gameObject.AddComponent<Camera>();
            var cameraData = _camera.GetUniversalAdditionalCameraData();
            cameraData.renderType = CameraRenderType.Overlay;
        }

        public static Camera GetCamera()
        {
            if (!instance)
                instance = new GameObject("Handle Camera", typeof(HandleCamera)).GetComponent<HandleCamera>();
            return instance._camera;
        }
    }
}
