using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Card_Firewall : Cards.Card
    {
        private static Color32  sm_fireColor = new Color32(255, 92, 10, 64);

        #region Properties
        public override string Name => "Firewall";
        public override int ManaCost => 12;
        public override string IconName => "Fire";
        #endregion

        protected Queue<Cave.Node> CalculateNodes(Vector2Int vMouseCoord)
        {
            // TODO:             
            //// 1. Get the Cave.Node at vMouseCoord (use Cave.Instance[vMouseCoord])
            //// 2. If it is a valid Node, grow the result by alternating North and South in a growing pattern
            //// 3. Only add Valid / Walkable nodes
            //// 4. Store the result in a queue and return from this function
            
            Queue<Cave.Node> nodes = new Queue<Cave.Node>();

            Cave.Node center = Cave.Instance[vMouseCoord];
            if (center == null) return nodes;
            nodes.Enqueue(center);

            bool northBlocked = false;
            bool southBlocked = false;

            for (int offset = 1; offset < Cave.HEIGHT && (!northBlocked || !southBlocked); ++offset)
            {
                if (!northBlocked)
                {
                    Vector2Int northCoord = vMouseCoord + Vector2Int.up * offset;
                    if (!Cave.WithinBounds(northCoord)) northBlocked = true;
                    else
                    {
                        Cave.Node north = Cave.Instance[northCoord];
                        if (north == null) northBlocked = true;
                        else nodes.Enqueue(north);
                    }
                }

                if (!southBlocked)
                {
                    Vector2Int southCoord = vMouseCoord + Vector2Int.down * offset;
                    if (!Cave.WithinBounds(southCoord)) southBlocked = true;
                    else
                    {
                        Cave.Node south = Cave.Instance[southCoord];
                        if (south == null) southBlocked = true;
                        else nodes.Enqueue(south);
                    }
                }
            }

            return nodes;
        }

        public override List<(Vector2Int, Color32)> GetPreviewEffect(Vector2Int vMouseCoord)
        {
            // TODO: 
            //// use CalculateNodes() to get the affected nodes from vMouseCoord
            //// Create a result list of type List<(Vector2Int, Color32)>.
            //// For each Node in the node queue add a tuple (Node.Coord, sm_fireColor) in your
            //// result list and return this list
            Queue<Cave.Node> nodes = CalculateNodes(vMouseCoord);
            List<(Vector2Int, Color32)> result = new List<(Vector2Int, Color32)>(nodes.Count);
            foreach (Cave.Node node in nodes) result.Add((node.Coord, sm_fireColor));
            return result;
        }

        public override void Perform(Vector2Int vMouseCoord)
        {
            // TODO: Use CalculateNodes() to get the affected node queue
            //// Add a burning effect at this node (see the Effects.CreateFire() function)
            //// Add this Node to a container/manager of 'Burning nodes' 
            //// (Optional Bonus) Add a little delay between each Node so that
            //// the firewall "grows" in the alternating North / South pattern over time
            Queue<Cave.Node> nodes = CalculateNodes(vMouseCoord);
            if (nodes.Count == 0) return;
            if (Cards.Instance != null) Cards.Instance.StartCoroutine(GrowFirewall(nodes));
            else IgniteNodesImmediate(nodes);
        }

        private IEnumerator GrowFirewall(Queue<Cave.Node> nodes)
        {
            const float FIRE_SIZE = 1.0f;
            const float FIRE_DURATION = 6.0f;
            const float STEP_DELAY = 0.15f;

            while (nodes.Count > 0)
            {
                Cave.Node node = nodes.Dequeue();
                Effects.CreateFire((Vector3)node.Center, FIRE_SIZE, FIRE_DURATION);
                BurningManager.Instance?.AddOrRefreshNode(node, FIRE_DURATION);
                if (nodes.Count > 0) yield return new WaitForSeconds(STEP_DELAY);
            }
        }

        private void IgniteNodesImmediate(Queue<Cave.Node> nodes)
        {
            const float FIRE_SIZE = 1.0f;
            const float FIRE_DURATION = 6.0f;

            while (nodes.Count > 0)
            {
                Cave.Node node = nodes.Dequeue();
                Effects.CreateFire((Vector3)node.Center, FIRE_SIZE, FIRE_DURATION);
                BurningManager.Instance?.AddOrRefreshNode(node, FIRE_DURATION);
            }
        }
    }
}