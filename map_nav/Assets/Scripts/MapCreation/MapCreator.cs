using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tools;
using UnityEngine;
using UnityEngine.Rendering;

namespace MapCreation
{
    public class MapCreator : MonoBehaviour
    {
        private float _edgeLength = 1.0f;
        private float _inerLength;
        private float _offset = 1.5f;
        private float _doubleInerLength;
        [SerializeField] private List<Vector3Int> _hexPosOffset = new List<Vector3Int>();
        [SerializeField] private List<Vector3> _worldPosOffset = new List<Vector3>();

        private Vector3 hexRatation = new Vector3(0, 0, 0);
        private Dictionary<Vector3Int, HexProperty> m_PosToHex = new Dictionary<Vector3Int, HexProperty>();

        private MapInfo m_MapInfo = new MapInfo();

        private GameObject tile;
        private Transform tilesParent;
        
        [SerializeField] public bool isEditing;
        [SerializeField] public bool isDeleting;
        [SerializeField] public bool Navigation;
        private bool isNaving;
        /// <summary>
        /// 初始化坐标偏移 
        /// 读取地图信息，放入字典中
        /// </summary>
        void Start()
        {
            isNaving = false;
            _inerLength = (_edgeLength / 2) * Mathf.Sqrt(3);
            _doubleInerLength = _inerLength * 2;
            _hexPosOffset.Add(new Vector3Int(1, -1, 0));
            _hexPosOffset.Add(new Vector3Int(1, 0, -1));
            _hexPosOffset.Add(new Vector3Int(0, 1, -1));
            _hexPosOffset.Add(new Vector3Int(-1, 1, 0));
            _hexPosOffset.Add(new Vector3Int(-1, 0, 1));
            _hexPosOffset.Add(new Vector3Int(0, -1, 1));

            _worldPosOffset.Add(new Vector3(_inerLength, 0, -_offset));
            _worldPosOffset.Add(new Vector3(_doubleInerLength, 0, 0));
            _worldPosOffset.Add(new Vector3(_inerLength, 0, _offset));
            _worldPosOffset.Add(new Vector3(-_inerLength, 0, _offset));
            _worldPosOffset.Add(new Vector3(-_doubleInerLength, 0, 0));
            _worldPosOffset.Add(new Vector3(-_inerLength, 0, -_offset));

            
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

            HexProperty tmp;
            for (int i = 0; i < m_MapInfo.hexPosInfo.Count; ++i)
            {
                tmp = m_PosToHex[m_MapInfo.hexPosInfo[i]];
                UpdateList(tmp);
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

        void ResetMap()
        {
            m_MapInfo.worldPosInfo.Clear();
            m_MapInfo.hexPosInfo.Clear();
            m_MapInfo.hexPosInfo.Add(Vector3Int.zero);
            m_MapInfo.worldPosInfo.Add(Vector3.zero);
            for (int i = 1; i < tilesParent.childCount; ++i)
            {
                Destroy(tilesParent.GetChild(i));
            }
            tilesParent.GetChild(0).GetComponent<HexProperty>().adjacentTiles.Clear();
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
            CheckMouseInput();
        }

        void CheckMouseInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                var hex = GetClickedHex();
                if (hex == null) return;
                if (isEditing)
                {
                    GenerateHex(hex);
                }
                else if (isDeleting)
                {
                    DeleteHex(hex);
                }
                else if (Navigation && !isNaving)
                {
                    isNaving = true;
                    foreach (var pathNode in lastPath)
                    {
                        pathNode.Material.color = Color.white;
                    }
                    lastPath.Clear();
                    hex.Material.color = Color.red;
                    StartCoroutine(Navigate(hex.hexPosition));
                }
            }
        }

        #region Nav

        private List<HexProperty> lastPath = new List<HexProperty>();

        IEnumerator Navigate(Vector3Int startPos)
        {
            yield return new WaitForSeconds(0.5f);
            Debug.Log("Start!");
            while (!Input.GetMouseButton(0))
            {
                yield return null;
            }

            var endHex = GetClickedHex();
            endHex.Material.color = Color.red;

            if(endHex == null) Debug.Log("Can't Get end Point");
            var endPos = endHex.hexPosition;

            PriorityQueue<HexProperty> queue = new PriorityQueue<HexProperty>();
            var startHex = m_PosToHex[startPos];
            startHex.parent = null;
            startHex.distanceToStart = 0;
            queue.Add(startHex);
            StartCoroutine(AStar(queue, endPos));
        }

        void popLogic(HexProperty hex)
        {
            hex.isInQueue = false;
            hex.hasBeenChosen = true;
            hex.Material.color = Color.green;
        }
        
        private IEnumerator AStar(PriorityQueue<HexProperty> queue, Vector3Int endPos)
        {
            List<HexProperty> ChosenList = new List<HexProperty>();
            bool findEndPoint = false;
            while (queue.Count() > 0)
            {
                var hex = queue.GetTop();
                queue.Pop();
                popLogic(hex);
                ChosenList.Add(hex);
                // yield return new WaitForSeconds(0.5f);
                if (hex.hexPosition == endPos)
                {
                    findEndPoint = true;
                    break;
                }
                foreach (var tile in hex.adjacentTiles)
                {
                    if (tile.isInQueue == false && tile.hasBeenChosen == false)
                    {
                        tile.distanceToStart = hex.distanceToStart + tile.cost;
                        tile.distanceToEnd = Math.Abs(endPos.x - tile.hexPosition.x) +
                                             Math.Abs(endPos.y - tile.hexPosition.y) +
                                             Math.Abs(endPos.z - tile.hexPosition.z);
                        tile.evaluateDistance = tile.distanceToStart + tile.distanceToEnd;
                        queue.Add(tile);
                        tile.isInQueue = true;
                        tile.parent = hex;
                    }
                    else if (tile.isInQueue == true)
                    {
                        int newDis2Start = hex.distanceToStart + tile.cost;
                        int newDis2End = Math.Abs(endPos.x - tile.hexPosition.x) +
                                         Math.Abs(endPos.y - tile.hexPosition.y) +
                                         Math.Abs(endPos.z - tile.hexPosition.z);
                        if (newDis2End + newDis2Start < tile.evaluateDistance)
                        {
                            tile.distanceToStart = newDis2Start;
                            tile.distanceToEnd = newDis2End;
                            tile.evaluateDistance = tile.distanceToStart + tile.distanceToEnd;
                            tile.parent = hex;
                        }
                        
                    }
                }
            }

            if (findEndPoint == true)
            {
                var hex = m_PosToHex[endPos];
                while (hex != null)
                {
                    lastPath.Add(hex);
                    // hex.Material.color = Color.blue;
                    hex.hasBeenChosen = false;
                    hex = hex.parent;
                    // yield return new WaitForSeconds(0.5f);
                }
            }

            while (queue.Count() > 0)
            {
                var hex = queue.GetTop();
                queue.Pop();
                hex.isInQueue = false;
            }

            foreach (var hex in ChosenList)
            {
                hex.Material.color = Color.white;
            }
            ChosenList.Clear();
            isNaving = false;
            yield return null;
            foreach (var hexProperty in lastPath)
            {
                hexProperty.Material.color = Color.blue;
            }
        }
        #endregion


        HexProperty GetClickedHex()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                var hex = hit.collider.transform.GetComponent<HexProperty>();
                return hex;
            }

            return null;
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
            int cnt = 0;
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
                var otherHex = GenerateHexObject(hexPos, worldPos, childCount + cnt);
                hex.adjacentTiles.Add(otherHex);
                m_MapInfo.hexPosInfo.Add(hexPos);
                m_MapInfo.worldPosInfo.Add(worldPos);
                ++cnt;
            }

            foreach (var vec3Int in tmpList)
            {
                var other = m_PosToHex[vec3Int];
                UpdateList(other);
            }
        }

        void DeleteHex(HexProperty hex)
        {
            foreach (var adjacentTile in hex.adjacentTiles)
            {
                adjacentTile.adjacentTiles.Remove(hex);
            }

            int pos = m_MapInfo.hexPosInfo.IndexOf(hex.hexPosition);
            m_PosToHex.Remove(hex.hexPosition);
            m_MapInfo.worldPosInfo.RemoveAt(pos);
            m_MapInfo.hexPosInfo.RemoveAt(pos);
            Destroy(hex.gameObject);
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

