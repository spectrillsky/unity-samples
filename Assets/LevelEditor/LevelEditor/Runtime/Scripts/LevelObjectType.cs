using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LFG.LevelEditor
{
    [CreateAssetMenu(fileName = "LevelObjectType", menuName = "LifeForce/LevelEditor/LevelObjectType")]
    public class LevelObjectType : ScriptableObject
    {
        [field:SerializeField] public string DisplayName { get; private set; }
        public Vector3 DefaultSize;

        [field:SerializeField] public float RotationStep { get; private set; } = 90f;
        
        [field: SerializeField] public GridPlacement GridPlacements { get; private set; }
        [field: SerializeField] public List<PlacementConstraint> Constraints;

        [field: SerializeField] public int MaxInstances = -1;

        [field: SerializeField] public bool ShowInGalleryFilters = true;
        
        public bool HasGridPlacement(GridPlacement gridPlacement)
        {
            return GridPlacements.HasFlag(gridPlacement);
        }

        public bool HasMaxInstances()
        {
            return MaxInstances >= 0;
        }
    }
}


