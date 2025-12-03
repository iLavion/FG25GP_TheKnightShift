using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public abstract class Unit : MonoBehaviour
    {
        private int             m_iHealth;
        protected Vector2Int    m_vCoord;
        private Cave.Node       m_node;
        private Material        m_health;

        static Material         sm_unitMaterial;
        static Material         sm_health;
        static Mesh[]           sm_unitMeshes = null;

        public static List<Unit> AllUnits = new List<Unit>();

        #region Properties

        public Cave.Node Node
        {
            get => m_node;
            set
            {
                if (m_node != null)
                {
                    m_node.Unit = null;
                }

                m_node = value;

                if (m_node != null)
                {
                    m_node.Unit = this;
                }
            }
        }

        protected abstract int MeshIndex { get; }

        protected abstract int AttackDamage { get; }

        protected abstract int MaxHealth { get; }

        #endregion

        protected virtual void OnEnable()
        {
            AllUnits.Add(this);
        }

        protected virtual void OnDisable()
        {
            AllUnits.Remove(this);
        }

        protected virtual void Start()
        {
            // coord from position
            m_vCoord = new Vector2Int(Mathf.RoundToInt(transform.position.x),
                                      Mathf.RoundToInt(transform.position.y));

            // initialize health
            m_iHealth = MaxHealth;

            // load unit material?
            if (sm_unitMaterial == null)
            {
                sm_unitMaterial = Resources.Load<Material>("Materials/Units");
            }

            // load unit health material?
            if (sm_health == null)
            {
                sm_health = Resources.Load<Material>("Materials/Health");
            }

            // create unit meshes?
            if (sm_unitMeshes == null)
            {
                Vector3[] corners = new Vector3[]
                {
                    new Vector3(0.0f, 0.0f, 0.0f),
                    new Vector3(0.0f, 1.0f, 0.0f),
                    new Vector3(1.0f, 1.0f, 0.0f),
                    new Vector3(1.0f, 0.0f, 0.0f),
                };

                sm_unitMeshes = new Mesh[4];
                for (int y = 0; y < 2; ++y)
                {
                    for (int x = 0; x < 2; ++x)
                    {
                        Mesh mesh = new Mesh();
                        mesh.name = "UnitMesh " + x + ", " + y;
                        mesh.hideFlags = HideFlags.DontSave;
                        mesh.subMeshCount = 2;

                        List<Vector3> vertices = new List<Vector3>();
                        vertices.AddRange(System.Array.ConvertAll(corners, c => (c - Vector3.right * 0.5f) * 1.3f));
                        vertices.AddRange(System.Array.ConvertAll(corners, c => new Vector3(c.x - 0.5f, c.y * 0.2f + 0.2f, -0.01f) * 0.6f));

                        List<Vector2> uv = new List<Vector2>();
                        uv.AddRange(System.Array.ConvertAll(corners, c => new Vector2(c.x * 0.5f + x * 0.5f, c.y * 0.5f + y * 0.5f)));
                        uv.AddRange(System.Array.ConvertAll(corners, c => (Vector2)c));

                        mesh.vertices = vertices.ToArray();
                        mesh.uv = uv.ToArray();
                        mesh.SetTriangles(new int[] { 0, 1, 2, 0, 2, 3 }, 0);
                        mesh.SetTriangles(new int[] { 4, 5, 6, 4, 6, 7 }, 1);
                        mesh.RecalculateNormals();
                        mesh.RecalculateBounds();
                        sm_unitMeshes[y * 2 + x] = mesh;
                    }
                }
            }

            // assign mesh and material
            m_health = new Material(sm_health);
            m_health.SetFloat("_Amount", Mathf.Clamp01(m_iHealth / (float)MaxHealth));
            GetComponent<MeshFilter>().sharedMesh = sm_unitMeshes[MeshIndex];
            GetComponent<MeshRenderer>().sharedMaterials = new Material[] { sm_unitMaterial, m_health };
        }

        public void TakeDamage(int iAmount)
        {
            m_iHealth = Mathf.Clamp(m_iHealth - iAmount, 0, MaxHealth);
            m_health.SetFloat("_Amount", Mathf.Clamp01(m_iHealth / (float)MaxHealth));

            // HP message
            GameCanvas.Instance.CreateMessage((iAmount < 0 ? "+" : "-") + Mathf.Abs(iAmount) + " HP", 
                                              transform.position + Vector3.up * 0.9f, 
                                              iAmount < 0 ? Color.green : Color.red);

            // death?
            if (m_iHealth <= 0.0f)
            {
                Destroy(gameObject);
            }
        }

        protected IEnumerator AttackUnit(Unit enemy)
        {
            float fDuration = Random.Range(0.5f, 1.0f);
            AnimationCurve rotationCurve = new AnimationCurve(new Keyframe[]
            {
                new Keyframe(0.0f, 0.0f),
                new Keyframe(fDuration * 0.2f, 15.0f),
                new Keyframe(fDuration * 0.4f, -15.0f),
                new Keyframe(fDuration * 0.6f, 15.0f),
                new Keyframe(fDuration * 0.8f, -15.0f),
                new Keyframe(fDuration, 0.0f),
            });

            // 'attack' animation :)
            for (float f = 0.0f; f < fDuration; f += Time.deltaTime)
            {
                transform.localEulerAngles = new Vector3(0.0f, 0.0f, rotationCurve.Evaluate(f));
                yield return null;
            }
            transform.localEulerAngles = Vector3.zero;

            // deal damage
            try
            {
                enemy.TakeDamage(AttackDamage);
            }
            catch (System.Exception)
            {
                // enemy might have died
            }
        }

        public static T GetClosestUnit<T>(Vector3 vPosition) where T : Unit
        {
            float fBestDistance = float.MaxValue;
            T bestUnit = null;
            foreach (Unit unit in AllUnits)
            {
                if (unit is T)
                {
                    float fDistance = Vector2.Distance(vPosition, unit.transform.position);
                    if (fDistance < fBestDistance)
                    {
                        fBestDistance = fDistance;
                        bestUnit = unit as T;
                    }
                }
            }

            return bestUnit;
        }

        public static T Create<T>(Vector3 vPosition, Transform parent) where T : Unit
        {
            GameObject go = new GameObject(typeof(T).Name);
            go.transform.parent = parent;
            Cave.Node node = Cave.Instance.GetClosestFreeNode(vPosition);
            go.transform.position = node.Center;
            T unit = go.AddComponent<T>();
            unit.Node = node;
            return unit;
        }
    }
}