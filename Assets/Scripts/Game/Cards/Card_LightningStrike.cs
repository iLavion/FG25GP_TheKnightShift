using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Card_LightningStrike : Cards.Card
    {
        private static Color32  sm_electrictyColor = new Color32(10, 92, 255, 64);
        private const float LINK_RADIUS_SQ = 4.0f;
        private const int MAX_BRANCHES = 3;
        private const int MIN_DAMAGE = 40;
        private const int MAX_DAMAGE = 75;

        private class ChainNode
        {
            public Unit Unit;
            public ChainNode Parent;
            public List<ChainNode> Children = new List<ChainNode>();
            public int Depth;
        }

        #region Properties
        public override string Name => "Lightning\nStrike";
        public override int ManaCost => 7;
        public override string IconName => "Lightning";
        #endregion

        public override List<(Vector2Int, Color32)> GetPreviewEffect(Vector2Int vMouseCoord)
        {
            return new List<(Vector2Int, Color32)> {
                (vMouseCoord, sm_electrictyColor)
            };
        }

        public override void Perform(Vector2Int vMouseCoord)
        {
            Cave.Node node = Cave.Instance[vMouseCoord];
            if (node == null || node.Unit == null) return;

            ChainNode root = BuildChain(node.Unit);
            ChainNode target = FindDeepestNode(root);
            if (target == null) return;

            List<Unit> path = BuildPathUnits(target);
            if (path.Count == 0) return;

            DealDamageAlongPath(path);
            PlayLightningEffects(path);
        }

        private ChainNode BuildChain(Unit startUnit)
        {
            ChainNode root = new ChainNode {
                Unit = startUnit,
                Parent = null,
                Depth = 0
            };

            HashSet<Unit> visited = new HashSet<Unit>();
            visited.Add(startUnit);
            BuildChildren(root, visited);
            return root;
        }

        private void BuildChildren(ChainNode parent, HashSet<Unit> visited)
        {
            List<Unit> neighbors = FindClosestUnits(parent.Unit, visited);
            foreach (Unit neighbor in neighbors) {
                ChainNode child = new ChainNode {
                    Unit = neighbor,
                    Parent = parent,
                    Depth = parent.Depth + 1
                };

                parent.Children.Add(child);
                visited.Add(neighbor);
                BuildChildren(child, visited);
            }
        }

        private List<Unit> FindClosestUnits(Unit origin, HashSet<Unit> visited)
        {
            List<Unit> result = new List<Unit>();
            if (origin == null) return result;

            Vector3 originPos = origin.transform.position;
            foreach (Unit unit in Unit.AllUnits) {
                if (unit == null || unit == origin || unit.Node == null || visited.Contains(unit)) continue;
                float distSq = (unit.transform.position - originPos).sqrMagnitude;
                if (distSq <= LINK_RADIUS_SQ) result.Add(unit);
            }

            result.Sort((a, b) => {
                float da = (a.transform.position - originPos).sqrMagnitude;
                float db = (b.transform.position - originPos).sqrMagnitude;
                return da.CompareTo(db);
            });

            if (result.Count > MAX_BRANCHES) result.RemoveRange(MAX_BRANCHES, result.Count - MAX_BRANCHES);
            return result;
        }

        private ChainNode FindDeepestNode(ChainNode root)
        {
            if (root == null) return null;

            List<ChainNode> stack = new List<ChainNode> { root };
            List<ChainNode> candidates = new List<ChainNode>();
            int maxDepth = -1;

            while (stack.Count > 0)
            {
                ChainNode current = stack[stack.Count - 1];
                stack.RemoveAt(stack.Count - 1);
                if (current.Depth > maxDepth) {
                    maxDepth = current.Depth;
                    candidates.Clear();
                    candidates.Add(current);
                }
                else if (current.Depth == maxDepth) candidates.Add(current);
                foreach (ChainNode child in current.Children) stack.Add(child);
            }
            if (candidates.Count == 0) return null;
            return candidates[Random.Range(0, candidates.Count)];
        }

        private List<Unit> BuildPathUnits(ChainNode target)
        {
            List<Unit> path = new List<Unit>();
            ChainNode current = target;
            while (current != null) {
                path.Add(current.Unit);
                current = current.Parent;
            }
            path.Reverse();
            return path;
        }

        private void DealDamageAlongPath(List<Unit> path)
        {
            foreach (Unit unit in path) {
                if (unit == null) continue;
                unit.TakeDamage(Random.Range(MIN_DAMAGE, MAX_DAMAGE + 1));
            }
        }

        private void PlayLightningEffects(List<Unit> path)
        {
            if (path.Count == 1) {
                Vector3 target = (Vector3)path[0].Node.Center + Vector3.up * 0.1f;
                Effects.CreateLightning(target + Vector3.up * 6.0f, target);
                return;
            }

            for (int i = 0; i < path.Count - 1; ++i) {
                Vector3 from = (Vector3)path[i].Node.Center + Vector3.up * 0.1f;
                Vector3 to = (Vector3)path[i + 1].Node.Center + Vector3.up * 0.1f;
                Effects.CreateLightning(from, to);
            }
        }
    }
}
