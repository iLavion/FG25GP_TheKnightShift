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
            // 1. Get the Cave.Node at vMouseCoord (use Cave.Instance[vMouseCoord])
            // 2. If it is a valid Node, grow the result by alternating North and South in a growing pattern
            // 3. Only add Valid / Walkable nodes
            // 4. Store the result in a queue and return from this function
            throw new System.NotImplementedException();
        }

        public override List<(Vector2Int, Color32)> GetPreviewEffect(Vector2Int vMouseCoord)
        {
            // TODO: use CalculateNodes() to get the affected nodes from vMouseCoord
            // Create a result list of type List<(Vector2Int, Color32)>.
            // For each Node in the node queue add a tuple (Node.Coord, sm_fireColor) in your
            // result list and return this list
            throw new System.NotImplementedException();
        }

        public override void Perform(Vector2Int vMouseCoord)
        {
            // TODO: Use CalculateNodes() to get the affected node queue
            // Add a burning effect at this node (see the Effects.CreateFire() function)
            // Add this Node to a container/manager of 'Burning nodes' 
            
            // (Optional Bonus) Add a little delay between each Node so that
            // the firewall "grows" in the alternating North / South pattern over time
            throw new System.NotImplementedException();
        }
    }
}
