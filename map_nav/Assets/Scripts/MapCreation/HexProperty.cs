using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//https://zhuanlan.zhihu.com/p/405708238 六边形网格寻路

namespace MapCreation
{
    [Serializable]
    public class HexProperty : MonoBehaviour
    {
        [SerializeField]
        public Vector3Int hexPosition;

        [SerializeField] public List<HexProperty> adjacentTiles = new List<HexProperty>();

        public bool isObstacle;
    }
}