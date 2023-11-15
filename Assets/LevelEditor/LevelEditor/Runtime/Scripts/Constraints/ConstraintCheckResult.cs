using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LFG.LevelEditor
{
    [Serializable]
    public class ConstraintCheckResult
    {
        public readonly PlacementConstraint Constraint;
        public readonly LevelObject LevelObject;
        public bool IsValid { get; private set; }
        public List<string> Messages { get; private set; } = new List<string>();
        public readonly object Context;

        public ConstraintCheckResult(PlacementConstraint _constraint, LevelObject _levelObject, object _context = null)
        {
            Constraint = _constraint;
            LevelObject = _levelObject;
            Context = _context;
            IsValid = true;
        }
        
        public ConstraintCheckResult(PlacementConstraint _constraint, LevelObject _levelObject, bool _isValid, string _message = "", object _context = null)
        {
            Constraint = _constraint;
            LevelObject = _levelObject;
            IsValid = _isValid;
            Messages.Add(_message);
            Context = _context;
        }

        public ConstraintCheckResult Set(bool _isValid, string _message)
        {
            IsValid = _isValid;
            Messages.Add(_message);
            return this;
        }
    }
}
