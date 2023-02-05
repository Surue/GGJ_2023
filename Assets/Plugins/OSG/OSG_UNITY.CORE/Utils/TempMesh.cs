using UnityEngine;

namespace OSG
{
    public class TempMesh
    {
        public Vector3[] vertices = new Vector3[4];
        public Vector2[] uv = new Vector2[4];
        public int[] triangles= new int[6];
        public Color32[] colors32 = new Color32[4];

        Mesh mesh;

        public TempMesh()
        {
        }
        public TempMesh(Mesh mesh)
        {
            this.mesh = mesh;
        }

        public void UpdateMesh()
        {
            if (mesh == null)
                mesh = new Mesh();

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uv;
            mesh.colors32 = colors32;
        }

        public void Clear()
        {
            Vector3 zero = Vector3.zero;
            for(int i = 0; i < vertices.Length; i++)
                vertices[i] = zero;
        }

        public Mesh Mesh {
            get { return mesh; }
        }
    }
}

