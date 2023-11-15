using System;
using System.Collections.Generic;
using UnityEngine;

namespace LFG.LevelEditor
{
    [RequireComponent(typeof(LevelObject))]
    public class LevelObjectPlaceholder : MonoBehaviour
    {
        private List<LevelObject> overlappingObjects = new List<LevelObject>();

        private List<MeshRenderer> renderers;
        
        private void OnEnable()
        {
            
        }

        void ChangeCollidersToTriggers(bool toTriggers)
        {
            // GetComponentsInChildren<MeshRenderer>()
        }

        void OnTriggerEnter(Collider other)
        {
            LevelObject levelObject = other.GetComponentInParent<LevelObject>();
            if (!overlappingObjects.Exists(lvlObj => lvlObj.Equals(levelObject)))
            {
                overlappingObjects.Add(levelObject);
                OnOverlappingObjectsChange();
            }
        }

        void OnTriggerExit(Collider other)
        {
            LevelObject levelObject = other.GetComponentInParent<LevelObject>();
            if (overlappingObjects.Exists(lvlObj => lvlObj.Equals(levelObject)))
            {
                overlappingObjects.Remove(levelObject);
                OnOverlappingObjectsChange();
            }
        }

        void OnOverlappingObjectsChange()
        {
            
        }
    }
}