using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(Canvas))]
    public class GameCanvas : MonoBehaviour
    {
        [RequireComponent(typeof(Text))]
        private class Message : MonoBehaviour
        {
            private Text            m_text;
            private float           m_fTime;
            private Color           m_color;
            private RectTransform   m_transform;

            const float LIFETIME = 2.5f;

            static readonly AnimationCurve AlphaCurve = new AnimationCurve(new Keyframe[]
            {
                new Keyframe(0.0f, 1.0f),
                new Keyframe(LIFETIME - 1.0f, 1.0f),
                new Keyframe(LIFETIME, 0.0f),
            });

            #region Properties

            protected float Alpha => AlphaCurve.Evaluate(m_fTime);

            #endregion

            private void Awake()
            {
                m_text = GetComponent<Text>();
                m_transform = GetComponent<RectTransform>();
                m_color = m_text.color;
            }

            private void Update()
            {
                m_fTime += Time.deltaTime;
                m_transform.anchoredPosition += Vector2.up * Time.deltaTime * 10.0f;
                m_color.a = Alpha;
                m_text.color = m_color;

                if (m_fTime > LIFETIME)
                {
                    Destroy(gameObject);
                }
            }
        }

        private GameObject      m_messageTemplate;
        private Camera          m_camera;
        private CanvasScaler    m_scaler;
        private Transform       m_effectParent;

        static GameCanvas       sm_instance;

        #region Properties

        public Transform EffectParent => m_effectParent;

        public CanvasScaler Scaler => m_scaler;

        public static GameCanvas Instance => sm_instance;

        #endregion

        private void Awake()
        {
            sm_instance = this;
            m_effectParent = transform.Find("Effects");
            m_messageTemplate = transform.Find("Messages/MessageTemplate").gameObject;
            m_camera = transform.parent.GetComponentInChildren<Camera>();
            m_scaler = GetComponent<CanvasScaler>();
        }

        public Vector2 GetCanvasPosition(Vector3 vWorldPosition)
        {
            Vector2 vVP = m_camera.WorldToViewportPoint(vWorldPosition);
            return new Vector2(vVP.x * m_scaler.referenceResolution.x,
                               vVP.y * m_scaler.referenceResolution.y);
        }

        public void CreateMessage(string message, Vector3 vWorldPosition, Color color)
        {
            GameObject go = Instantiate(m_messageTemplate, m_messageTemplate.transform.parent);
            go.name = "Message";
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = GetCanvasPosition(vWorldPosition);
            Text txt = go.GetComponent<Text>();
            txt.text = message;
            txt.color = color;
            go.AddComponent<Message>();
            go.SetActive(true);
        }
    }
}