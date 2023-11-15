using UnityEngine;
using System.Collections;

namespace LFG.LevelEditor
{
	[CreateAssetMenu(fileName = "OverlappingConstraint", menuName = "LifeForce/LevelEditor/Constraint/Overlapping")]
	public class OverlappingConstraint : PlacementConstraint
	{
		public override ConstraintCheckResult CanBePlaced(LevelObject levelObject, object context = null)
		{
			ConstraintCheckResult result = new ConstraintCheckResult(this, levelObject, context);
			
			Placeholder placeholder = levelObject.GetComponent<Placeholder>();
			if (!placeholder)
				return result.Set(false, "No placeholder found for overlapping check");
			return result.Set(!placeholder.IsOverlapping(), "Result of Placeholder's IsOverlapping check");
		}
	}
}

