using UnityEngine;

namespace ModStuff.CreativeMenu
{
    static class MechanismManager
    {
        const float CONE_HEIGHT = 0.25f;
        const float CONE_RADIUS = 0.125f;
        const int CONE_SUBDIVISIONS = 32;

        const float CUBE_LENGTH = 0.25f * 0.5f;
        const float CUBE_WIDTH = 0.25f;
        const float CUBE_HEIGHT = 0.75f;
        
        public static bool MeshVisibility { get; set; } = true;

        static PlayerController _controller;

        public static bool DoUpdate()
        {
            if (_controller == null)
            {
                GameObject go = GameObject.Find("PlayerController");
                if(go != null) _controller = go.GetComponent<PlayerController>();
                return false;
            }
            return _controller.IsUpdateLayerPaused;
        }

        public static Mesh CreateCone()
        {
            return CreateCone(CONE_SUBDIVISIONS, CONE_RADIUS, CONE_HEIGHT);
        }

        public static Mesh CreateCube()
        {
            return CreateBox(CUBE_LENGTH, CUBE_LENGTH, CUBE_LENGTH, Vector3.zero);
        }

        public static Mesh CreateLongShiftedCube()
        {
            return CreateBox(CUBE_LENGTH, CUBE_WIDTH, CUBE_HEIGHT, new Vector3(0f, CUBE_HEIGHT * 0.5f, 0f));
        }

        public static Mesh CreateCone(int subdivisions, float radius, float height)
        {
            Mesh mesh = new Mesh();

            Vector3[] vertices = new Vector3[subdivisions + 2];
            Vector2[] uv = new Vector2[vertices.Length];
            int[] triangles = new int[(subdivisions * 2) * 3];

            vertices[0] = Vector3.zero;
            uv[0] = new Vector2(0.5f, 0f);
            for (int i = 0, n = subdivisions - 1; i < subdivisions; i++)
            {
                float ratio = (float)i / n;
                float r = ratio * (Mathf.PI * 2f);
                float x = Mathf.Cos(r) * radius;
                float z = Mathf.Sin(r) * radius;
                vertices[i + 1] = new Vector3(x, 0f, z);

                Debug.Log(ratio);
                uv[i + 1] = new Vector2(ratio, 0f);
            }
            vertices[subdivisions + 1] = new Vector3(0f, height, 0f);
            uv[subdivisions + 1] = new Vector2(0.5f, 1f);

            // construct bottom

            for (int i = 0, n = subdivisions - 1; i < n; i++)
            {
                int offset = i * 3;
                triangles[offset] = 0;
                triangles[offset + 1] = i + 1;
                triangles[offset + 2] = i + 2;
            }

            // construct sides

            int bottomOffset = subdivisions * 3;
            for (int i = 0, n = subdivisions - 1; i < n; i++)
            {
                int offset = i * 3 + bottomOffset;
                triangles[offset] = i + 1;
                triangles[offset + 1] = subdivisions + 1;
                triangles[offset + 2] = i + 2;
            }

            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            return mesh;
        }
        
        public static Mesh CreateBox(float length, float width, float height, Vector3 offset)
        {
            Mesh mesh = new Mesh();

            Vector3 p0 = new Vector3(-length * .5f, -height * .5f, width * .5f) + offset;
            Vector3 p1 = new Vector3(length * .5f, -height * .5f, width * .5f) + offset;
            Vector3 p2 = new Vector3(length * .5f, -height * .5f, -width * .5f) + offset;
            Vector3 p3 = new Vector3(-length * .5f, -height * .5f, -width * .5f) + offset;

            Vector3 p4 = new Vector3(-length * .5f, height * .5f, width * .5f) + offset;
            Vector3 p5 = new Vector3(length * .5f, height * .5f, width * .5f) + offset;
            Vector3 p6 = new Vector3(length * .5f, height * .5f, -width * .5f) + offset;
            Vector3 p7 = new Vector3(-length * .5f, height * .5f, -width * .5f) + offset;

            Vector3[] vertices = new Vector3[]
            {
	            // Bottom
	            p0, p1, p2, p3,
 
	            // Left
	            p7, p4, p0, p3,
 
	            // Front
	            p4, p5, p1, p0,
 
	            // Back
	            p6, p7, p3, p2,
 
	            // Right
	            p5, p6, p2, p1,
 
	            // Top
	            p7, p6, p5, p4
            };
            
            Vector3 up = Vector3.up;
            Vector3 down = Vector3.down;
            Vector3 front = Vector3.forward;
            Vector3 back = Vector3.back;
            Vector3 left = Vector3.left;
            Vector3 right = Vector3.right;

            Vector3[] normales = new Vector3[]
            {
	            // Bottom
	            down, down, down, down,
 
	            // Left
	            left, left, left, left,
 
	            // Front
	            front, front, front, front,
 
	            // Back
	            back, back, back, back,
 
	            // Right
	            right, right, right, right,
 
	            // Top
	            up, up, up, up
            };
            
            Vector2 _00 = new Vector2(0f, 0f);
            Vector2 _10 = new Vector2(1f, 0f);
            Vector2 _01 = new Vector2(0f, 1f);
            Vector2 _11 = new Vector2(1f, 1f);

            Vector2[] uvs = new Vector2[]
            {
	            // Bottom
	            _11, _01, _00, _10,
 
	            // Left
	            _11, _01, _00, _10,
 
	            // Front
	            _11, _01, _00, _10,
 
	            // Back
	            _11, _01, _00, _10,
 
	            // Right
	            _11, _01, _00, _10,
 
	            // Top
	            _11, _01, _00, _10,
            };

            int[] triangles = new int[]
            {
	            // Bottom
	            3, 1, 0,
                3, 2, 1,			
 
	            // Left
	            3 + 4 * 1, 1 + 4 * 1, 0 + 4 * 1,
                3 + 4 * 1, 2 + 4 * 1, 1 + 4 * 1,
 
	            // Front
	            3 + 4 * 2, 1 + 4 * 2, 0 + 4 * 2,
                3 + 4 * 2, 2 + 4 * 2, 1 + 4 * 2,
 
	            // Back
	            3 + 4 * 3, 1 + 4 * 3, 0 + 4 * 3,
                3 + 4 * 3, 2 + 4 * 3, 1 + 4 * 3,
 
	            // Right
	            3 + 4 * 4, 1 + 4 * 4, 0 + 4 * 4,
                3 + 4 * 4, 2 + 4 * 4, 1 + 4 * 4,
 
	            // Top
	            3 + 4 * 5, 1 + 4 * 5, 0 + 4 * 5,
                3 + 4 * 5, 2 + 4 * 5, 1 + 4 * 5,

            };

            mesh.vertices = vertices;
            mesh.normals = normales;
            mesh.uv = uvs;
            mesh.triangles = triangles;

            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
