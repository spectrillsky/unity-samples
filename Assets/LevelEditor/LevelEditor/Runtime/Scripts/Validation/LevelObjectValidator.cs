using System;
using UnityEngine;

namespace LFG.LevelEditor
{

    [Serializable]
    public class LevelObjectValidator : IDisposable
    {
        [field: SerializeField] public readonly LevelValidator LevelValidator;
        [field: SerializeField] public readonly LevelObjectProfile LevelObject;
    
        [field: SerializeField] public Status CurrentStatus { get; private set; }
        [field: SerializeField] public int CurrentCount { get; private set; }

        public LevelObjectValidator(LevelValidator levelValidator, LevelObjectProfile levelObject)
        {
            LevelValidator = levelValidator;
            LevelObject = levelObject;

            Controller.OnAction.AddListener(OnControllerAction);
            Controller.instance.Cursor.onEvent.AddListener(OnCursorEvent);

            Validate();
        }

        public void Dispose()
        {
            Controller.OnAction?.RemoveListener(OnControllerAction);
            if(Controller.instance)
                Controller.instance.Cursor.onEvent?.RemoveListener(OnCursorEvent);
            GC.SuppressFinalize(this);
        }

        private void OnCursorEvent(Cursor.Events e, object context)
        {
            if (e == Cursor.Events.PlaceObject)
            {
                LevelObject levelObject = (LevelObject)context;
                if (levelObject.Profile == LevelObject)
                    Validate();
            }
        }

        private void OnControllerAction(Controller.Actions action, object context)
        {
            if (action == Controller.Actions.DeleteLevelObject)
            {
                LevelObjectProfile levelObject = (LevelObjectProfile)context;
                if (levelObject == LevelObject) Validate();
            }
        }

        public Status Validate()
        {
            CurrentCount = LevelValidator.Level.GetLevelObjectCount(LevelObject);
            if (CurrentCount < LevelObject.MinInstances)
            {
                CurrentStatus = Status.InvalidMin;
            }
            else if (LevelObject.HasMaxInstances() && CurrentCount > LevelObject.MaxInstances)
            {
                CurrentStatus = Status.InvalidMax;
            }
            else
                CurrentStatus = Status.Valid;

            new Event(this).Invoke();
            return CurrentStatus;
        }

        public bool HasReachedMaxInstances()
        {
            return CurrentCount >= LevelObject.MaxInstances;
        }

        public enum Status
        {
            Valid,
            InvalidMin,
            InvalidMax,
            InvalidPosition,
        }

        public class Event : Aether.Event<Event>
        {
            public readonly LevelObjectValidator Validator;
            
            public Event(LevelObjectValidator validator)
            {
                Validator = validator;
            }
        }
    }
}