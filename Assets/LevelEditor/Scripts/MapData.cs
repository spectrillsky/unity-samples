using System.Collections;
using System.Collections.Generic;
using LFG.LevelEditor;
using UnityEngine;

public class MapData : LevelData
{
    public Material material;
    public string testString;

    public override void Capture(Level level)
    {
        base.Capture(level);
        
        Map map = (Map)level;
        testString = map.testString;
        material = map.testMaterial;
    }
}
