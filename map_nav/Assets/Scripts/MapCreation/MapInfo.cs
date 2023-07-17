using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MapInfo
{
    //public Dictionary<Vector3Int, Vector3> mapInfo;
    public List<Vector3Int> hexPosInfo;
    public List<Vector3> worldPosInfo; 
    public MapInfo()
    {
        //mapInfo = new Dictionary<Vector3Int, Vector3>();
        hexPosInfo = new List<Vector3Int>();
        worldPosInfo = new List<Vector3>();
    }
    //
    // public MapInfo Add(Vector3Int hexPos, Vector3 worldPos)
    // {
    //     m_MapInfo.TryAdd(hexPos, worldPos);
    //     return this;
    // }
    //
    // public MapInfo Remove(Vector3Int hexPos)
    // {
    //     if (m_MapInfo.ContainsKey(hexPos))
    //     {
    //         m_MapInfo.Remove(hexPos);
    //     }
    //
    //     return this;
    // }
}