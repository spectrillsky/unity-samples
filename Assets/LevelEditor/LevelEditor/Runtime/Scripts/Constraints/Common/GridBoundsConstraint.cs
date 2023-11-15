using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LFG.LevelEditor
{
    /// <summary>
    /// A check to see if the level object is hanging off the grid
    /// This is under the assumption of a strict grid placement
    /// </summary>
    [CreateAssetMenu(fileName = "GridBoundsConstraint", menuName = "LifeForce/LevelEditor/Constraint/Grid Bounds")]
    public class GridBoundsConstraint : PlacementConstraint
    {
        public override ConstraintCheckResult CanBePlaced(LevelObject levelObject, object context = null)
        {
            ConstraintCheckResult result = new ConstraintCheckResult(this, levelObject);
            
            Vector3 pos = (Vector3)context;
            
            Level level = Controller.CurrentLevel;
            Vector3 minPoint = level.GetMinPoint();
            Vector3 maxPoint = level.GetMaxPoint();

            if (pos.x < minPoint.x || pos.x > maxPoint.x)
            {
                result.Set(false, "LevelObject is outside grid area");
                return result;
            }

            if (pos.z < minPoint.z || pos.z > maxPoint.z)
            {
                result.Set(false, "LevelObject is outside grid area");
                return result;
            }

            //Calculate the bounds of the levelobject in the X-Z plane to verify that it is within the grid area
            float orientation = levelObject.transform.eulerAngles.y % 180;
            float upperXBounds = levelObject.transform.position.x + (orientation == 0
                ? levelObject.Size.x / 2
                : levelObject.Size.z / 2);
            if (upperXBounds > maxPoint.x)
                result.Set(false, "LevelObject's upper x bounds exceeds grid area");

            float lowerXBounds = levelObject.transform.position.x - (orientation == 0
                ? levelObject.Size.x / 2
                : levelObject.Size.z / 2);
            if (lowerXBounds < minPoint.x)
                result.Set(false, "LevelObject's lower x bounds exceeds grid area");
            
            float upperZBounds = levelObject.transform.position.z + (orientation == 0
                ? levelObject.Size.z / 2
                : levelObject.Size.x / 2);
            if (upperZBounds > maxPoint.z)
                result.Set(false, "LevelObject's upper z bounds exceeds grid area");
            
            float lowerZBounds = levelObject.transform.position.z - (orientation == 0
                ? levelObject.Size.z / 2
                : levelObject.Size.x / 2);
            if (lowerZBounds < minPoint.z)
                result.Set(false, "LevelObject's lower z bounds exceeds grid area");

            return result;
        }
    }
}
