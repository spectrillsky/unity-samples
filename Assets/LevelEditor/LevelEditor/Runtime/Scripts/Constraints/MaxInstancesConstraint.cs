using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LFG.LevelEditor
{
    public class MaxInstancesConstraint : PlacementConstraint
    {
        public override ConstraintCheckResult CanBePlaced(LevelObject levelObject, object context = null)
        {
            if (!levelObject.Profile)
                return new ConstraintCheckResult(this, levelObject, false, "Max instances constraint: No Profile found");
            LevelObjectValidator validator = Controller.CurrentLevel.Validator.GetLevelObjectValidator(levelObject.Profile);
            return new ConstraintCheckResult(this, levelObject, !validator.HasReachedMaxInstances());
        }
    }
}
