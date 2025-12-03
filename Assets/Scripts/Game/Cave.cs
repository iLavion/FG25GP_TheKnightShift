using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class Cave : MonoBehaviour
    {
        public class Node
        {
            private Vector2Int m_vCoord;

            #region Properties

            public Vector2Int Coord => m_vCoord;

            public Vector2 Center => new Vector2(m_vCoord.x + 0.5f, m_vCoord.y + 0.1f);

            public Unit Unit { get; set; }

            #endregion

            public Node(Vector2Int vCoord)
            {
                m_vCoord = vCoord;
            }
        }

        private Node[,]             m_nodes = new Node[WIDTH, HEIGHT];

        private static Cave         sm_instance;

        public const int            WIDTH = 15;
        public const int            HEIGHT = 8;

        #region Properties

        public IEnumerable<Node> Nodes
        {
            get
            {
                for (int y = 0; y < HEIGHT; y++)
                {
                    for (int x = 0; x < WIDTH; x++)
                    {
                        if (m_nodes[x, y] != null)
                        {
                            yield return m_nodes[x, y];
                        }
                    }
                }
            }
        }

        public Node this[Vector2Int v] => this[v.x, v.y];

        public Node this[int x, int y] => WithinBounds(x, y) ? m_nodes[x, y] : null;

        public static Cave Instance => sm_instance;

        #endregion

        private void Awake()
        {
            sm_instance = this;

            // Cave array [true = walkable]
            Random.InitState(33);
            for (int y = 0; y < HEIGHT; y++)
            {
                for (int x = 0; x < WIDTH; x++)
                {
                    m_nodes[x, y] = x <= 2 || x >= WIDTH - 2 || Random.value < 0.94f ? new Node(new Vector2Int(x, y)) : null;
                }
            }

            CreateCaveMesh();
        }

        public static bool WithinBounds(Vector2Int v)
        {
            return WithinBounds(v.x, v.y);
        }

        public static bool WithinBounds(int x, int y)
        {
            return x >= 0 &&
                   y >= 0 &&
                   x < WIDTH &&
                   y < HEIGHT;
        }

        protected void CreateCaveMesh()
        {
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uv = new List<Vector2>();
            List<int> floors = new List<int>();
            List<int> rocks = new List<int>();

            for (int y = 0; y < m_nodes.GetLength(1); y++)
            {
                for (int x = 0; x < m_nodes.GetLength(0); x++)
                {
                    Rect r = new Rect(x, y, 1.0f, 1.0f);
                    AddQuad(r, 0.01f, true, vertices, uv, floors);

                    if (m_nodes[x, y] == null)
                    {
                        AddQuad(r, 0.0f, false, vertices, uv, rocks);
                    }
                }
            }

            // create mesh
            Mesh mesh = new Mesh();
            mesh.subMeshCount = 2;
            mesh.vertices = vertices.ToArray();
            mesh.uv = uv.ToArray();
            mesh.SetTriangles(floors.ToArray(), 0);
            mesh.SetTriangles(rocks.ToArray(), 1);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            // assign mesh
            GetComponent<MeshFilter>().mesh = mesh;
        }

        private void AddQuad(Rect r, float fZ, bool bRotateUV, List<Vector3> vertices, List<Vector2> uv, List<int> triangles)
        {
            // vertices
            int iStart = vertices.Count;
            vertices.AddRange(new Vector3[]{
                new Vector3(r.x, r.y, fZ),
                new Vector3(r.x, r.yMax, fZ),
                new Vector3(r.xMax, r.yMax, fZ),
                new Vector3(r.xMax, r.y, fZ)
            });

            // UV
            float fStep = 0.25f;
            Vector2 vUV = new Vector2(Random.Range(0, 3), Random.Range(0, 3)) * fStep;
            uv.Add(vUV + new Vector2(0.0f, 0.0f));
            uv.Add(vUV + new Vector2(0.0f, fStep));
            uv.Add(vUV + new Vector2(fStep, fStep));
            uv.Add(vUV + new Vector2(fStep, 0.0f));

            // rotate UV?
            if (bRotateUV)
            {
                int iRotateUV = Random.Range(0, 3);
                for (int iRot = 0; iRot < iRotateUV; iRot++)
                {
                    Vector2 vFirst = uv[iStart];
                    for (int i = 1; i < 4; ++i)
                    {
                        uv[iStart + i - 1] = uv[iStart + i];
                    }
                    uv[iStart + 3] = vFirst;
                }
            }

            // triangles
            triangles.AddRange(new int[]{
                iStart + 0, iStart + 1, iStart + 2,
                iStart + 2, iStart + 3, iStart + 0
            });
        }

        public Node GetClosestFreeNode(Vector2 vPosition)
        {
            float fBestDistance = float.MaxValue;
            Node bestNode = null;
            foreach (Node node in Nodes)
            {
                if (node.Unit == null)
                {
                    float fDistance = Vector2.Distance(vPosition, node.Center);
                    if (fDistance < fBestDistance)
                    {
                        fBestDistance = fDistance;
                        bestNode = node;
                    }
                }
            }
            return bestNode;
        }
    }
}