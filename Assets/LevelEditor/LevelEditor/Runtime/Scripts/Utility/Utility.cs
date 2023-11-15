using System.Collections.Generic;
using UnityEngine;

namespace LFG.LevelEditor
{
    public static class Utility
    {
        /// <summary>
        /// Returns all level objects at a position
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static LevelObject[] GetObjectsAtPosition(Vector3 position)
        {
            List<LevelObject> lvlObjs = new List<LevelObject>();
            foreach (var hit in Physics.RaycastAll(position + 1000 * Vector3.down, Vector3.up, 2000))
            {
                LevelObject lvlObj = hit.collider.GetComponentInParent<LevelObject>();
                if(lvlObj && !lvlObj.CompareTag("Placeholder"))
                    lvlObjs.Add(lvlObj);
            }
            return lvlObjs.ToArray();
        }

        public static LevelObject GetHighestLevelObject(Vector3 position)
        {
            LevelObject highestLvlObj = null;
            LevelObject[] lvlObjs = GetObjectsAtPosition(position);
            foreach (var lvlObj in lvlObjs)
            {
                if (!highestLvlObj) highestLvlObj = lvlObj;
                if (lvlObj.transform.position.y > highestLvlObj.transform.position.y)
                    highestLvlObj = lvlObj;
            }
            return highestLvlObj;
        }

        public static Vector3 GetHighestPosition(Vector3 position)
        {
            List<LevelObject> lvlObjs = new List<LevelObject>();
            List<RaycastHit> hits =
                new List<RaycastHit>(Physics.RaycastAll(position + 1000 * Vector3.up, Vector3.down, 2000));
            hits.Sort((hit1, hit2) => hit1.distance > hit2.distance ? 1 : -1);
            Vector3 highestPoint = Vector3.zero;
            foreach (var hit in hits)
            {
                LevelObject lvlObj = hit.collider.GetComponentInParent<LevelObject>();
                
                if (lvlObj && !lvlObj.CompareTag("Placeholder"))
                {
                    if (hit.point.y > highestPoint.y) highestPoint = hit.point;
                }
            }

            return highestPoint;
        }
        /// <summary>
        /// Finds a position below a particular point that a levelObject can be placed.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static Vector3 GetStackPosition(Vector3 position)
        {
            List<RaycastHit> hits =
                new List<RaycastHit>(Physics.RaycastAll(position + Controller.Settings.cellSize * Vector3.up, Vector3.down, 2000));
            hits.Sort((hit1, hit2) => hit1.distance > hit2.distance ? 1 : -1);
            GameObject highestObj = null;
            Vector3 highestPoint = Vector3.zero;
            foreach (var hit in hits)
            {
                LevelObject lvlObj = hit.collider.GetComponentInParent<LevelObject>();
                
                if (lvlObj && !lvlObj.CompareTag("Placeholder"))
                {
                    if (hit.point.y > highestPoint.y)
                    {
                        highestObj = lvlObj.gameObject;
                        highestPoint = hit.point;
                    }
                }
                else if (hit.collider.TryGetComponent(out LevelGrid grid) && !highestObj)
                        highestPoint = hit.point;
                    
            }

            return highestPoint;
        }
        
        public static LevelObjectRaycastData RaycastLevelObject(Ray ray, float distance, bool ignoreObjects = false)
        {
            List<RaycastHit> hits =
                new List<RaycastHit>(Physics.RaycastAll(ray, distance));
            hits.Sort((hit1, hit2) => hit1.distance < hit2.distance ? -1 : 1);
            foreach (var hit in hits)
            {
                LevelObject lvlObj = hit.collider.GetComponentInParent<LevelObject>();
                if (!ignoreObjects && lvlObj && !lvlObj.GetComponent<Placeholder>())
                    return new LevelObjectRaycastData(lvlObj, hit.point);
                else if (hit.collider.TryGetComponent(out LevelGrid level))
                    return new LevelObjectRaycastData(level, hit.point);
            }
            return new LevelObjectRaycastData(ray.origin + distance * ray.direction);
        }

        #region Vertex Snapping
        public static Vector3[] ConvertMeshVerticesToWorldSpace(MeshFilter meshFilter)
        {
            List<Vector3> vertices = new List<Vector3>();
            
            Matrix4x4 localToWorld = meshFilter.transform.localToWorldMatrix;
            foreach(var v in meshFilter.mesh.vertices)
                vertices.Add(localToWorld.MultiplyPoint3x4(v));

            return vertices.ToArray();
        }
        
        public static Vector3 ConvertVertexToWorldSpace(Transform transform, Vector3 vertex)
        {
            return transform.localToWorldMatrix.MultiplyPoint3x4(vertex);
        }
        
        public static Vector3 ConvertVertexToLocalSpace(Transform transform, Vector3 vertex)
        {
            return transform.worldToLocalMatrix.MultiplyPoint3x4(vertex);
        }

        public static RaycastVertexResult RaycastClosestVertex(Ray ray, float radius, float maxDistance, MeshFilter[] meshFilters = null, bool ignoreMeshFilters = false)
        {
            List<MeshFilter> relevantMeshFilters = new List<MeshFilter>();
            if(meshFilters != null)
                relevantMeshFilters.AddRange(meshFilters);
            Vector3 closestVertex = Vector3.zero;
            float closestDistance = -1;
            GameObject closestObject = null;
            
            foreach (var hit in Physics.SphereCastAll(ray, 5, 100))
            {
                
                MeshFilter[] _meshFilters = hit.collider.GetComponentsInChildren<MeshFilter>();
                foreach (var meshFilter in _meshFilters)
                {
                    LevelObject levelObject = meshFilter.GetComponentInParent<LevelObject>();
                    if (!levelObject) continue;
                    if (relevantMeshFilters.Count > 0)
                    {
                        //Skip if it is a revelant mesh filter and ignoring
                        if (relevantMeshFilters.Exists(mf => mf.Equals(meshFilter)) && ignoreMeshFilters) continue;
                        //Skip mesh filter if it is not a relevant mesh filter and not ignoring
                        else if(!relevantMeshFilters.Exists(mf => mf.Equals(meshFilter)) && !ignoreMeshFilters) continue;

                    }

                    foreach (var point in Utility.ConvertMeshVerticesToWorldSpace(meshFilter))
                    {
                        float distance = CalculateDistanceFromRay(ray, point);
                        if (closestDistance == -1 || distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestVertex = ConvertVertexToLocalSpace(meshFilter.transform, point);
                            closestObject = meshFilter.gameObject;
                        }
                    }
                }
            }
            return new RaycastVertexResult(closestVertex, closestDistance, closestObject);
        }
        #endregion
        
        #region Raycasting
        public static float CalculateDistanceFromRay(Ray ray, Vector3 point)
        {
            return Vector3.Cross(ray.direction.normalized, point - ray.origin).magnitude;
        }
        #endregion

        /// <summary>
        /// Returns an array of cell positions that encapsulate a size
        /// </summary>
        /// <param name="gridPosition"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static LevelObject[] GetOverlappingObjects(Vector3 gridPosition, Vector3 size, bool rotate = false)
        {
            List<LevelObject> overlappingObjects = new List<LevelObject>();
            foreach (var collider in Physics.OverlapBox(gridPosition,
                         0.9f * new Vector3(rotate ? size.z / 2 : size.x / 2, size.y / 2,
                             rotate ? size.x / 2 : size.z / 2)))
            {
                LevelObject lvlObj = collider.GetComponentInParent<LevelObject>();
                if (lvlObj && !overlappingObjects.Exists(_lvlObj => _lvlObj == lvlObj))
                {
                    if(!lvlObj.GetComponent<Placeholder>())
                        overlappingObjects.Add(lvlObj);
                }
            }
            return overlappingObjects.ToArray();
        }
        
        #region General

        public static LevelObjectType[] GetTypes(IEnumerable<LevelObjectProfile> levelObjects)
        {
            List<LevelObjectType> types = new List<LevelObjectType>();
            foreach (var levelObject in levelObjects)
            {
                foreach (var type in levelObject.Types)
                    if (!types.Exists(t => t == type))
                        types.Add(type);
            }

            return types.ToArray();
        }

        #endregion
    }
    
    public struct LevelObjectRaycastData
    {
        public Vector3 Point;
        public LevelObject LevelObject;
        public LevelGrid Level;

        public LevelObjectRaycastData(LevelObject levelObject, Vector3 point)
        {
            LevelObject = levelObject;
            Point = point;
            Level = null;
        }

        public LevelObjectRaycastData(LevelGrid level, Vector3 point)
        {
            Level = level;
            Point = point;
            LevelObject = null;
        }

        public LevelObjectRaycastData(Vector3 point)
        {
            Point = point;
            LevelObject = null;
            Level = null;
        }
    }
}
