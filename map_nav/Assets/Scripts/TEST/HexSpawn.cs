using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class HexSpawn : MonoBehaviour
{
    Mesh HexagonMesh;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    //正六边形外接圆半径，等于正六边形边长
    public const float outerRadius = 1;
    //正六边形内切圆半径，等于sqrt(3)*outerRaidus/2,其中sqrt(3)/2 = 0.866025404f
    public const float innerRadius = outerRadius * 0.866025404f;
    
    private List<Vector3> corners = new List<Vector3>();

    private MeshCollider m_MeshCollider;
    private void Awake()
    {
        corners.Add(new Vector3(0,0,outerRadius));
        corners.Add(new Vector3(innerRadius,0,0.5f*outerRadius));
        corners.Add(new Vector3(innerRadius,0,-0.5f*outerRadius));
        corners.Add(new Vector3(0,0,-outerRadius));
        corners.Add(new Vector3(-innerRadius,0,-0.5f*outerRadius));
        corners.Add(new Vector3(-innerRadius,0,0.5f*outerRadius));
        corners.Add(new Vector3(0,0,outerRadius));
        
        GameObject MeshSpwan = new GameObject("MeshSpwan");
        MeshSpwan.AddComponent<MeshFilter>();
        MeshSpwan.AddComponent<MeshRenderer>();
        m_MeshCollider = MeshSpwan.AddComponent<MeshCollider>();
        HexagonMesh = MeshSpwan.GetComponent<MeshFilter>().mesh;
        HexagonMesh.Clear();
    }
    private void Start()
    {
        Triangulate();
 
        HexagonMesh.vertices = vertices.ToArray();
        HexagonMesh.triangles = triangles.ToArray();

        m_MeshCollider.sharedMesh = HexagonMesh;
    }
 
    private void Triangulate()
    {
        //循环画出每一个三角形
       
        for (int i = 0; i < 6; i++)
        {
            AddRriangle(Vector3.zero, corners[i], corners[i + 1]);
        }
    }
    private void AddRriangle(Vector3 v1,Vector3 v2,Vector3 v3)
    {
        int verterIndex = vertices.Count;
        //三角形1顶点数据（V0,V1,V2)
        //三角形2顶点数据（V0,V2,V3)
        //三角形3顶点数据（V0,V3,V4）
        //三角形4顶点数据（V0,V4,V5）
        //三角形5顶点数据（V0,V5,V6）
        //三角形3顶点数据（V0,V6,V1）
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        //第一个三角形对应的triangles索引在verterices的索引是（0,1,2）
        //第而个三角形对应的triangles索引在verterices的索引是（3,4,5）
        ///...
        triangles.Add(verterIndex);
        triangles.Add(verterIndex + 1);
        triangles.Add(verterIndex + 2);
    }
 
 
}