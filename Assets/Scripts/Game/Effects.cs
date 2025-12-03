using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using UI;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.WebRequestMethods;

namespace Game
{
    public static class Effects
    {
        private abstract class EffectBase : MonoBehaviour
        {
            public float     m_fTimeToLive;

            protected virtual void Update()
            {
                // self destruct
                m_fTimeToLive -= Time.deltaTime;
                if(m_fTimeToLive < 0.0f)
                {
                    Destroy(gameObject);
                }
            }
        }

        [RequireComponent(typeof(Image))]
        private class Fire : EffectBase
        {
            public Transform        m_parent;
            public Vector3          m_vLocalPosition;
            public float            m_fSize;

            private Image           m_image;
            private RectTransform   m_transform;

            static Sprite[]         sm_fireFrames = null;

            private void Start()
            {
                if (sm_fireFrames == null)
                {
                    sm_fireFrames = Resources.LoadAll<Sprite>("Textures/fire");
                }

                m_image = GetComponent<Image>();
                m_image.sprite = sm_fireFrames[0];

                m_transform = GetComponent<RectTransform>();
                m_transform.anchorMin = Vector2.zero;
                m_transform.anchorMax = Vector2.zero;
                m_transform.pivot = new Vector2(0.5f, 0.1f);
                m_transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 69 * m_fSize);
                m_transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 150 * m_fSize);
                m_transform.localScale = Vector3.one;
            }

            protected override void Update()
            {
                base.Update();

                // update position
                if (m_parent != null)
                {
                    Vector3 vWP = m_parent.TransformPoint(m_vLocalPosition);
                    Vector2 vAP = GameCanvas.Instance.GetCanvasPosition(vWP);
                    m_transform.anchoredPosition = vAP;
                }

                // update fire frame
                int iFrame = Mathf.RoundToInt(m_fTimeToLive / 0.3f) % sm_fireFrames.Length;
                m_image.sprite = sm_fireFrames[iFrame];
            }
        }

        private class Lightning : EffectBase
        {
            public int                      m_iNumLines = 3;
            public Vector3                  m_vSource;
            public Vector3                  m_vTarget;
            private List<Vector3>[]         m_lines;
            private List<LineRenderer>      m_renderers;

            static Material[]               sm_materials;

            private void Start()
            {
                if (sm_materials == null)
                {
                    Material m = Resources.Load<Material>("Materials/Lightning");
                    sm_materials = new Material[10];
                    for (int i = 0; i < sm_materials.Length; ++i)
                    {
                        sm_materials[i] = new Material(m);
                        sm_materials[i].color = Color.Lerp(new Color(0.3f, 0.3f, 1.0f, 0.7f), new Color(0.8f, 0.8f, 1.0f, 0.7f), i / (float)sm_materials.Length);
                    }
                }

                m_lines = new List<Vector3>[m_iNumLines];
                m_renderers = new List<LineRenderer>(m_iNumLines);
                float fDistance = Vector3.Distance(m_vSource, m_vTarget);
                for (int i = 0; i < m_iNumLines; ++i)
                {
                    float fStep = Random.Range(0.5f, 1.0f);
                    int iNumSteps = Mathf.Max(Mathf.RoundToInt(fDistance / fStep), 2);
                    m_lines[i] = new List<Vector3>();

                    GameObject line = new GameObject("Line #" + i);
                    line.transform.parent = transform;
                    m_renderers.Add(line.AddComponent<LineRenderer>());

                    for (int j = 0; j <= iNumSteps; ++j)
                    {
                        float f = j / (float)iNumSteps;
                        Vector3 v = Vector3.Lerp(m_vSource, m_vTarget, f) - Vector3.forward * 0.1f;

                        if (j != 0 && j != iNumSteps)
                        {
                            v += (Vector3)Random.insideUnitCircle * fStep * 0.4f;
                        }

                        m_lines[i].Add(v);
                    }

                    float fWidth = Random.Range(fStep * 0.05f, fStep * 0.1f);
                    m_renderers[i].startWidth = fWidth;
                    m_renderers[i].endWidth = fWidth;
                    m_renderers[i].sharedMaterial = sm_materials[Random.Range(0, sm_materials.Length)];
                    m_renderers[i].positionCount = m_lines[i].Count;
                    m_renderers[i].SetPositions(m_lines[i].ToArray());
                }

                StartCoroutine(UpdateLightning());
            }

            IEnumerator UpdateLightning()
            {
                while(true)
                {
                    yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));

                    for (int i = 0; i < m_lines.Length; ++i)
                    {
                        float fStep = Vector3.Distance(m_lines[i][0], m_lines[i][1]);
                        for (int j = 1; j < m_lines[i].Count - 1; ++j)
                        {
                            float f = j / (float)(m_lines[i].Count - 1);
                            Vector3 v = Vector3.Lerp(m_vSource, m_vTarget, f) - Vector3.forward * 0.1f;
                            v += (Vector3)Random.insideUnitCircle * fStep * 0.4f;
                            m_lines[i][j] = v;
                        }

                        m_renderers[i].SetPositions(m_lines[i].ToArray());
                    }
                }
            }
        }

        public static void CreateFire(Vector3 vWorldPosition, float fSize, float fDuration)
        {
            CreateFire(Cave.Instance.transform, vWorldPosition, fSize, fDuration);
        }

        public static void CreateFire(Transform parent, Vector3 vLocalPosition, float fSize, float fDuration)
        {
            GameObject go = new GameObject("Fire");
            go.transform.parent = GameCanvas.Instance.EffectParent;
            Fire fire = go.AddComponent<Fire>();
            fire.m_parent = parent;
            fire.m_vLocalPosition = vLocalPosition;
            fire.m_fTimeToLive = fDuration;
            fire.m_fSize = fSize;
        }

        public static void CreateLightning(Vector3 vSource, Vector3 vTarget, float fDuration = 1.0f, int iNumLines = 3)
        {
            GameObject go = new GameObject("Lightning");
            go.transform.parent = Cave.Instance.transform;
            Lightning lightning = go.AddComponent<Lightning>();
            lightning.m_iNumLines = iNumLines;
            lightning.m_fTimeToLive = fDuration;
            lightning.m_vSource = vSource;
            lightning.m_vTarget = vTarget;
        }
    }
}
