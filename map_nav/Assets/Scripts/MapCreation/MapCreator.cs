using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MapCreation
{
    public class MapCreator : MonoBehaviour
    {
        [SerializeField] private float _edgeLength = 1.0f;
        [SerializeField] private float _inerLength;
        [SerializeField] private float _halfInerLength;
        [SerializeField] private float _offset = 0.75f;
        [SerializeField] private List<Vector3Int> _hexPosOffset = new List<Vector3Int>();
        [SerializeField] private List<Vector3> _worldPosOffset = new List<Vector3>();
        [SerializeField] public bool isEditing;
        [SerializeField] public bool isDeleting;
        [SerializeField] private Vector3 hexRatation = new Vector3(90, 0, 0);
        private Dictionary<Vector3Int, HexProperty> m_PosToHex = new Dictionary<Vector3Int, HexProperty>();

        private MapInfo m_MapInfo = new MapInfo();

        private GameObject tile;
        private Transform tilesParent;
        /// <summary>
        /// 初始化坐标偏移
        /// 读取地图信息，放入字典中
        /// </summary>
        void Start()
        {
            _inerLength = (_edgeLength / 2) * Mathf.Sqrt(3);
            _halfInerLength = _inerLength / 2;
            _hexPosOffset.Add(new Vector3Int(1, -1, 0));
            _hexPosOffset.Add(new Vector3Int(1, 0, -1));
            _hexPosOffset.Add(new Vector3Int(0, 1, -1));
            _hexPosOffset.Add(new Vector3Int(-1, 1, 0));
            _hexPosOffset.Add(new Vector3Int(-1, 0, 1));
            _hexPosOffset.Add(new Vector3Int(0, -1, 1));

            _worldPosOffset.Add(new Vector3(_offset, 0, -_halfInerLength));
            _worldPosOffset.Add(new Vector3(_offset, 0, _halfInerLength));
            _worldPosOffset.Add(new Vector3(0, 0, _inerLength));
            _worldPosOffset.Add(new Vector3(-_offset, 0, _halfInerLength));
            _worldPosOffset.Add(new Vector3(-_offset, 0, -_halfInerLength));
            _worldPosOffset.Add(new Vector3(0, 0, -_inerLength));
            
            tile = Resources.Load<GameObject>("Prefab/tilemap");
            tilesParent = GameObject.Find("Map").transform;
            LoadMapInfo();
        }

        /// <summary>
        /// 加载JSON存储好的地图信息
        /// 创建地图
        /// </summary>
        void LoadMapInfo()
        {
            string jsonPath = Application.dataPath + "/Resources/MapInfo.json";
            string json = File.ReadAllText(jsonPath);
            m_MapInfo = JsonUtility.FromJson<MapInfo>(json);

            Vector3Int tmpVector3Int;
            Vector3 tmpVector3;
            for (int i = 0; i < m_MapInfo.hexPosInfo.Count; ++i)
            {
                tmpVector3Int = m_MapInfo.hexPosInfo[i];
                tmpVector3 = m_MapInfo.worldPosInfo[i];
                GenerateHexObject(tmpVector3Int, tmpVector3, i);
                
            }
        }

        /// <summary>
        /// 在场景中生成地图组件， 并在hexPosition到HexProperty的Dict中添加信息
        /// 因为 初始化生成地图 和 后来生成地图都会调用这个函数
        /// 所以添加新增的 tile 信息就不放在这里了
        /// </summary>
        /// <param name="tmpVector3Int"></param>
        /// <param name="tmpVector3"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        HexProperty GenerateHexObject(Vector3Int tmpVector3Int, Vector3 tmpVector3, int index)
        {
            var maptile = Instantiate(tile,tilesParent);
            maptile.name = "tile" + index.ToString();
            maptile.transform.position = tmpVector3;
            maptile.transform.rotation = Quaternion.Euler(hexRatation);
            var hex = maptile.GetComponent<HexProperty>();
            hex.hexPosition = tmpVector3Int;
            m_PosToHex.Add(tmpVector3Int,hex);
            return hex;
        }
        
        void SaveMap()
        {
            string filePath = Application.dataPath + "/Resources/MapInfo.json";
            string saveJsonStr = JsonUtility.ToJson(m_MapInfo);
            StreamWriter sw = new StreamWriter(filePath);
            sw.Write(saveJsonStr);
            sw.Close();
            //Debug.Log(saveJsonStr);
        }

        private void OnEnable()
        {
            throw new NotImplementedException();
        }

        private void OnDisable()
        {
            throw new NotImplementedException();
        }

        private void OnDestroy()
        {
            SaveMap();
        }

        // Update is called once per frame
        void Update()
        {
            if (isEditing)
            {
                CheckMouseInput();
            }
        }

        void CheckMouseInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                // Debug.Log("Mouse Position is " + Input.mousePosition.ToString());
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    var hex = hit.collider.transform.GetComponent<HexProperty>();
                    if (hex != null)
                    {
                        GenerateHex(hex);
                    }
                }
            }
        }

        /// <summary>
        /// 为六边形生成 邻格
        /// 先检查是否已经有邻格 从全局地图信息中查找 有 则生成
        /// 生成完毕后执行一次本格子和新生成格子的 邻格list的更新
        /// </summary>
        /// <param name="hex"></param>
        void GenerateHex(HexProperty hex)
        {
            int childCount = GameObject.Find("Map").transform.childCount;
            List<Vector3Int> tmpList = new List<Vector3Int>();
            Vector3Int hexPos;
            for (int i = 0; i < 6; ++i)
            {
                hexPos = hex.hexPosition;
                hexPos += _hexPosOffset[i];
                tmpList.Add(hexPos);
                if (m_PosToHex.ContainsKey(hexPos))
                {
                    // Debug.Log("发现一个重复的");
                    continue;
                }
                var worldPos = hex.transform.position;
                worldPos += _worldPosOffset[i];
                var otherHex = GenerateHexObject(hexPos, worldPos, childCount + i);
                hex.adjacentTiles.Add(otherHex);
                m_MapInfo.hexPosInfo.Add(hexPos);
                m_MapInfo.worldPosInfo.Add(worldPos);
            }

            foreach (var vec3Int in tmpList)
            {
                var other = m_PosToHex[vec3Int];
                UpdateList(other);
            }
        }

        void UpdateList(HexProperty hex)
        {
            var hexPos = hex.hexPosition;
            for (int i = 0; i < 6; ++i)
            {
                hexPos = hex.hexPosition;
                hexPos += _hexPosOffset[i];
                if (m_PosToHex.ContainsKey(hexPos))
                {
                    if(!hex.adjacentTiles.Contains(m_PosToHex[hexPos]))
                        hex.adjacentTiles.Add(m_PosToHex[hexPos]);
                }
            }
        }
}
}

