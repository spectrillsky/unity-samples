using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine.AddressableAssets;

namespace LFG.LevelEditor.Editor
{
    [InitializeOnLoad]
    public class Setup
    {

        [UnityEditor.MenuItem("LifeForce/Level Editor/Setup")]
        static void Init()
        {
            CreateTag("Placeholder");
            CreateAddressableGroups();
        }
        
        /// <summary>
        /// Creates placeholder tag for use in raycasting logic
        /// </summary>
        /// <param name="tag"></param>
        static void CreateTag(string tag) {
            var asset = AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset");
            if (asset != null) { // sanity checking
                var so = new SerializedObject(asset);
                var tags = so.FindProperty("tags");

                var numTags = tags.arraySize;
                // do not create duplicates
                for (int i = 0; i < numTags; i++) {
                    var existingTag = tags.GetArrayElementAtIndex(i);
                    if (existingTag.stringValue == tag) return;
                }

                tags.InsertArrayElementAtIndex(numTags);
                tags.GetArrayElementAtIndex(numTags).stringValue = tag;
                so.ApplyModifiedProperties();
                so.Update();
            }
        }

        /// <summary>
        /// Creates addressable settings if they do not exist, creates relevant labels and groups, and adds LevelObjects in path
        /// to addressable group.
        /// </summary>
        static void CreateAddressableGroups()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (!settings)
            {
                //Create addressable asset settings
                settings = AddressableAssetSettings.Create("Assets", "AddressableAssetSettings", true, true);
            }
            var labels = settings.GetLabels(); 
            if(!labels.Exists(label => label.Equals("Level")))
                settings.AddLabel("Level");
            if(!labels.Exists(label => label.Equals("LevelObject")))
                settings.AddLabel("LevelObject");

            var lvlObjGuids = AssetDatabase.FindAssets("t:LevelObject",
                new[] { Controller.Settings.levelObjectsPath });

            var group = settings.DefaultGroup;
            
            var entriesAdded = new List<AddressableAssetEntry>();
            foreach(var guid in lvlObjGuids)
            {
                var entry = settings.CreateOrMoveEntry(guid, group, readOnly: false, postEvent: false);
                entry.address = AssetDatabase.GUIDToAssetPath(guid);
                entry.labels.Add("LevelObject");
                
                entriesAdded.Add(entry);
            }
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesAdded, true);
        }
    }
    
}
