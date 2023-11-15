using System;
using System.Collections.Generic;
using UnityEngine;

namespace LFG.LevelEditor
{
    
    //TODO: Create validation modules
    //TODO: Separate between whether or not they are verifiable by validating the data versus validating through runtime dependencies (object collision checks)
    //TODO: May be possible to load and validate inside an additive scene asynchronously 
    [Serializable]
    public class LevelValidator : IDisposable
    {
        [SerializeField] private Level _level;
        public Level Level { get { return _level; } }
        
        [field: SerializeField] public List<LevelObjectValidator> ObjectValidators { get; private set; } = new List<LevelObjectValidator>();

        [field: SerializeField] public  Result LastValidationResult { get; private set; }
        
        public LevelValidator(Level level)
        {
            Controller.OnAction.AddListener(OnControllerAction);
            Controller.instance.Cursor.onEvent.AddListener(OnCursorEvent);
            LevelObjectValidator.Event.AddListener(OnLevelObjectValidatorEvent);
            _level = level;
            Initialize(level);
        }

        public void Dispose()
        {
            LevelObjectValidator.Event.RemoveListener(OnLevelObjectValidatorEvent);
            Controller.OnAction?.RemoveListener(OnControllerAction);
            if(Controller.instance)
                Controller.instance.Cursor.onEvent.RemoveListener(OnCursorEvent);
            foreach(var objValidator in ObjectValidators) objValidator.Dispose();
            
            GC.SuppressFinalize(this);
        }
        
        private void OnLevelObjectValidatorEvent(LevelObjectValidator.Event e)
        {
            Validate();
        }

        private void OnCursorEvent(Cursor.Events e, object context)
        {
            if (e == Cursor.Events.PlaceObject)
            {
                CreateLevelObjectValidator(((LevelObject)context).Profile);
            }
        }

        private void OnControllerAction(Controller.Actions action, object context)
        {
        }

        public void Initialize(Level level)
        {
            _level = level;
            ObjectValidators.Clear();
            foreach (var levelObject in Controller.Settings.LevelObjects)
            {
                CreateLevelObjectValidator(levelObject);
            }
                    
            foreach (var levelObject in level.GetLevelObjects())
            {
                CreateLevelObjectValidator(levelObject.Profile);
            }
            new Event(EventType.Initialize, this).Invoke();
            Validate();
        }

        public LevelObjectValidator CreateLevelObjectValidator(LevelObjectProfile profile)
        {
            if (ObjectValidators.Exists(v => v.LevelObject == profile))
                return ObjectValidators.Find(v => v.LevelObject == profile);

            LevelObjectValidator validator = new LevelObjectValidator(this, profile);
            ObjectValidators.Add(validator);
            new Event(EventType.CreateLevelObjectValidator, validator).Invoke();
            return validator;
        }

        public Result Validate()
        {
            LevelObjectValidator[] objectValidators = GetInvalidLevelObjectValidators();
            Result result = new Result(objectValidators);
            LastValidationResult = result;

            new Event(EventType.ValidationResult, result).Invoke();

            if (!result.IsValid())
            {
                string levelName = Level.Data ? Level.Data.DisplayName : Level.name;
                Debug.LogWarning($"[LevelEditor] Level failed validation - {levelName}");
            }
            
            return result;
        }

        public LevelObjectValidator[] GetInvalidLevelObjectValidators()
        {
            return ObjectValidators.FindAll(lov => lov.CurrentStatus != LevelObjectValidator.Status.Valid).ToArray();
        }

        public LevelObjectValidator GetLevelObjectValidator(LevelObjectProfile profile)
        {
            var validator = ObjectValidators.Find(v => v.LevelObject == profile);
            validator ??= CreateLevelObjectValidator(profile);
            return validator;
        }
        
        public class Event : Aether.Event<Event>
        {
            public readonly EventType Type;
            public readonly object Context;

            public Event(EventType type, object context)
            {
                Type = type;
                Context = context;
            }

        }
        
        public enum EventType
        {
            Initialize,
            CreateLevelObjectValidator,
            ValidationResult,
        }
        
        [Serializable]
        public class Result
        {
            public List<LevelObjectValidator> InvalidObjectValidators = new List<LevelObjectValidator>();

            public Result(LevelObjectValidator[] invalidObjectValidators)
            {
                InvalidObjectValidators.AddRange(invalidObjectValidators);
            }

            public bool IsValid()
            {
                return InvalidObjectValidators.Count == 0;
            }
        }
    }








}
