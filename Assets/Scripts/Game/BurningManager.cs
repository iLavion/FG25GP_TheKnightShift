using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class BurningManager : MonoBehaviour
    {
        private class BurningNodeData
        {
            public float RemainingLifetime;
            public float TickTimer;
            public Unit LastUnit;
        }

        [SerializeField]
        private float m_burningDotDuration = 5.0f;
        [SerializeField]
        private float m_tickInterval = 1.0f;
        [SerializeField]
        private int m_damagePerTick = 3;
        private readonly Dictionary<Cave.Node, BurningNodeData> m_burningNodes = new Dictionary<Cave.Node, BurningNodeData>();
        private readonly List<Cave.Node> m_nodesToRemove = new List<Cave.Node>();
        public static BurningManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void AddOrRefreshNode(Cave.Node node, float lifetime)
        {
            if (node == null || lifetime <= 0.0f) return;
            if (!m_burningNodes.TryGetValue(node, out BurningNodeData data)) {
                data = new BurningNodeData();
                m_burningNodes[node] = data;
            }

            data.RemainingLifetime = Mathf.Max(data.RemainingLifetime, lifetime);
            data.TickTimer = 0.0f;
        }

        public bool IsNodeBurning(Cave.Node node)
        {
            return node != null && m_burningNodes.ContainsKey(node);
        }

        private void Update()
        {
            if (m_burningNodes.Count == 0) return;

            float dt = Time.deltaTime;
            m_nodesToRemove.Clear();

            foreach (KeyValuePair<Cave.Node, BurningNodeData> entry in m_burningNodes) {
                Cave.Node node = entry.Key;
                BurningNodeData data = entry.Value;

                data.RemainingLifetime -= dt;
                data.TickTimer -= dt;

                Unit occupant = node.Unit;
                if (occupant == null) data.LastUnit = null;
                else {
                    if (data.LastUnit != occupant) {
                        data.LastUnit = occupant;
                        data.TickTimer = 0.0f;
                    }

                    if (data.TickTimer <= 0.0f) {
                        occupant.AddDamageOverTime(new Burning(m_burningDotDuration, m_damagePerTick));
                        data.TickTimer = m_tickInterval;
                    }
                }

                if (data.RemainingLifetime <= 0.0f) m_nodesToRemove.Add(node);
            }
            if (m_nodesToRemove.Count > 0) foreach (Cave.Node node in m_nodesToRemove) m_burningNodes.Remove(node);
        }
    }
}
