using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class EnemyFOVVisualizer : MonoBehaviour
{
    private EnemyFOV fov;
    private MeshFilter meshFilter;
    private Mesh mesh;

    [Header("การแสดงผล")]
    public Material fovMaterial;         // ลาก Material FOV_Shader มาใส่
    public int meshResolution = 30;      // ความละเอียดของกรวย (ยิ่งเยอะยิ่งเรียบ)
    public float meshHeight = 1f;       // ความสูงของกรวย (เพื่อให้สูงพ้นพื้น)

    void Start()
    {
        fov = GetComponentInParent<EnemyFOV>(); 
        
        meshFilter = GetComponent<MeshFilter>();
        
        MeshRenderer rend = GetComponent<MeshRenderer>();
        if (rend == null) rend = gameObject.AddComponent<MeshRenderer>();
        rend.material = fovMaterial;

        mesh = new Mesh();
        mesh.name = "FOV Mesh";
        meshFilter.mesh = mesh;
    }

    void LateUpdate()
    {
        DrawFOVMesh();
    }

    void DrawFOVMesh()
    {
        int stepCount = Mathf.RoundToInt(fov.viewAngle * meshResolution);
        float stepAngleSize = fov.viewAngle / stepCount;
        List<Vector3> viewPoints = new List<Vector3>();

        for (int i = 0; i <= stepCount; i++)
        {
            float angle = transform.eulerAngles.y - fov.viewAngle / 2 + stepAngleSize * i;
            viewPoints.Add(DirFromAngle(angle, true));
        }

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.up * meshHeight;
        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(transform.position + viewPoints[i] * fov.viewRadius) + Vector3.up * meshHeight;

            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    private Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal) { angleInDegrees += transform.eulerAngles.y; }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}