using System.Collections;
using System.Collections.Generic;
using LFG.LevelEditor;
using UnityEngine;

public class Map : Level
{
    public new MapData _data;

    public string testString;
    public Material testMaterial;
    
    public override void Init(LevelData data)
    {
        base.Init(data);
        var mapData = (MapData)data;
        Debug.Log(mapData.testString);
        Grid.GetComponent<MeshRenderer>().material = mapData.material;
    }

    public override LevelData GetData(bool capture = true)
    {
        // return base.GetData(capture);
        if (!_data)
            _data = ScriptableObject.CreateInstance<MapData>();
        _data.Capture(this);
        return _data;
    }
}
