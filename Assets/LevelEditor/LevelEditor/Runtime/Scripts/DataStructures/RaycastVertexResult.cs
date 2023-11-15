using System;
using UnityEngine;

namespace LFG.LevelEditor
{
    [Serializable]
    public class RaycastVertexResult
    {
        public GameObject GameObject;
        public Vector3 Vertex;
        public float Distance;
        public bool HasResult;

        public RaycastVertexResult(Vector3 vertex, float distance, GameObject gameObject = null)
        {
            GameObject = gameObject;
            Vertex = vertex;
            Distance = distance;
            HasResult = Distance >= 0;
        }
    }
}