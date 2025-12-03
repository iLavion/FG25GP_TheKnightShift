using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Card_Fireball : Cards.Card
    {
        private static readonly Color32 sm_fireColor = new Color32(255, 140, 32, 96);
        private static readonly Vector2Int[] sm_cardinalDirections = new Vector2Int[] {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        private const int FIREBALL_STEPS = 3;
        private const float FIRE_FX_SIZE = 1.1f;
        private const float FIRE_DURATION = 5.0f;

        #region Properties
        public override string Name => "Fireball";
        public override int ManaCost => 9;
        public override string IconName => "Fire";
        #endregion

        public override List<(Vector2Int, Color32)> GetPreviewEffect(Vector2Int vMouseCoord)
        {
            List<HashSet<Cave.Node>> layers = CalculateLayers(vMouseCoord);
            List<(Vector2Int, Color32)> preview = new List<(Vector2Int, Color32)>();
            foreach (HashSet<Cave.Node> layer in layers) {
                foreach (Cave.Node node in layer) preview.Add((node.Coord, sm_fireColor));
            }
            return preview;
        }

        public override void Perform(Vector2Int vMouseCoord)
        {
            List<HashSet<Cave.Node>> layers = CalculateLayers(vMouseCoord);
            if (layers.Count == 0) return;

            // ? Now why did i decide to not compute the 3 steps and spawn all flames in one go ?
            // * I need that spread of love. that way it actually feels like a fireball or wahtever
            // * that hits the ground and splashes the inferno across the ground.

            if (Cards.Instance != null) Cards.Instance.StartCoroutine(SpreadFire(layers));
            else IgniteLayersImmediate(layers);
        }

        private List<HashSet<Cave.Node>> CalculateLayers(Vector2Int vMouseCoord)
        {
            List<HashSet<Cave.Node>> layers = new List<HashSet<Cave.Node>>();
            Cave.Node startNode = Cave.Instance[vMouseCoord];
            if (startNode == null) return layers;

            HashSet<Cave.Node> visited = new HashSet<Cave.Node>();
            Queue<(Cave.Node node, int step)> frontier = new Queue<(Cave.Node, int)>();
            visited.Add(startNode);
            frontier.Enqueue((startNode, 0));

            while (frontier.Count > 0) {
                (Cave.Node node, int step) current = frontier.Dequeue();
                while (layers.Count <= current.step) layers.Add(new HashSet<Cave.Node>());
                layers[current.step].Add(current.node);
                if (current.step >= FIREBALL_STEPS) continue;

                foreach (Vector2Int dir in sm_cardinalDirections) {
                    Vector2Int nextCoord = current.node.Coord + dir;
                    if (!Cave.WithinBounds(nextCoord)) continue;

                    Cave.Node nextNode = Cave.Instance[nextCoord];
                    if (nextNode == null || visited.Contains(nextNode)) continue;

                    visited.Add(nextNode);
                    frontier.Enqueue((nextNode, current.step + 1));
                }
            }
            return layers;
        }

        private IEnumerator SpreadFire(List<HashSet<Cave.Node>> layers)
        {
            const float SPREAD_DELAY = 0.15f;
            foreach (HashSet<Cave.Node> layer in layers) {
                IgniteLayer(layer);
                yield return new WaitForSeconds(SPREAD_DELAY);
            }
        }

        private void IgniteLayersImmediate(List<HashSet<Cave.Node>> layers)
        {
            foreach (HashSet<Cave.Node> layer in layers) IgniteLayer(layer);
        }

        private void IgniteLayer(HashSet<Cave.Node> layer)
        {
            foreach (Cave.Node node in layer)
            {
                Effects.CreateFire((Vector3)node.Center, FIRE_FX_SIZE, FIRE_DURATION);
                BurningManager.Instance?.AddOrRefreshNode(node, FIRE_DURATION);
            }
        }
    }
}
