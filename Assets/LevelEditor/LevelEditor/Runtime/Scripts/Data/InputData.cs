using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LFG.LevelEditor
{
    [CreateAssetMenu(fileName="InputData", menuName = "LifeForce/LevelEditor/InputData")]
    public class InputData : ScriptableObject
    {
        public bool BeginSelect;
        public bool Select;
        public bool EndSelect;
        
        public bool Cancel;
        
        public Vector2 MousePosition;
        
        public bool Next;
        public bool Previous;

        public Vector2 MoveVector;

        public bool BeginSelectVertex;
        public bool SelectVertex;
        public bool VertexSnapping;
        
        public bool EndVertexSnapping;

        public float Rotate;
        public float RotateObject;
        public float Scroll;
        public float ShiftScroll;

        public Vector2 MouseDelta;
        public Vector2 MouseDrag;

        public bool HoldingRightClick;
    }
}
