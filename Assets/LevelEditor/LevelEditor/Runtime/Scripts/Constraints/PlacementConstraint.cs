using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LFG.LevelEditor
{
    /// <summary>
    /// A base class used to determine whether or not a levelobject can be placed
    /// </summary>
    [CreateAssetMenu(fileName = "PlacementConstraint", menuName = "LifeForce/LevelEditor/Constraint/Empty")]
    public class PlacementConstraint : ScriptableObject
    {
        public string description;
        
        [PropertyTooltip("Which modes should this constraint apply to")]
        public GridPlacement Modes { get; private set; }

        public bool HasMode(GridPlacement mode)
        {
            return Modes.HasFlag(mode);
        }

        public virtual ConstraintCheckResult CanBePlaced(LevelObject levelObject, object context = null)
        {
            return new ConstraintCheckResult(this, levelObject, true, "Default check always returns true", context);
        }
    }
}
