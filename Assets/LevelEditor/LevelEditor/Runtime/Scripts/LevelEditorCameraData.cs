using UnityEngine;

namespace LFG.LevelEditor
{
    [CreateAssetMenu(fileName="LevelEditorCameraData", menuName="LifeForce/LevelEditor/CameraData")]
    public class LevelEditorCameraData : ScriptableObject
    {
        public float MaxDistance = 10;
        
        public Vector3 FollowVector = new Vector3(0, 10, 10);

        public float Speed = 10f;

        public float RotationStep = 10;
        
        public float MinZoom;
        public float MaxZoom;

        public float ClampPadding = 1f;
    }
}