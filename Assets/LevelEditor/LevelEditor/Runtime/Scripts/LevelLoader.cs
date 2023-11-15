using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

namespace LFG.LevelEditor
{
    public class LevelLoader : MonoBehaviour
    {
        public class OnEvent : UnityEvent<Events, object>{};

        public OnEvent onEvent = new OnEvent();
        
        public Level LoadLevel(LevelData levelData)
        {
            Level level = Instantiate(Controller.Settings.DefaultLayout);
            level.transform.position = levelData.Position;
            level.transform.eulerAngles = levelData.Rotation;
            level.transform.localScale = levelData.Scale;
            
            foreach (var lvlObjData in levelData.LevelObjects)
            {
                LevelObjectProfile profile = Controller.Settings.GetLevelObjectById(lvlObjData.ResourceID);
                if (profile)
                {
                    LevelObject lvlObj = Instantiate(profile.Prefab, level.transform).GetComponent<LevelObject>();
                    lvlObj.InitializeData(lvlObjData);
                }
            }

            level.Init(levelData);
            onEvent.Invoke(Events.EndLoad, level);
            return level;
        }

        public virtual void UnloadLevel(Level level)
        {
            onEvent.Invoke(Events.BeginUnload, null);
            Destroy(level.gameObject);
            onEvent.Invoke(Events.EndUnload, null);
        }

        public virtual void ReloadLevel(Level level)
        {
            
        }

        public enum Events
        {
            BeginLoad,
            EndLoad,
            BeginUnload,
            EndUnload
        }
    }
}
