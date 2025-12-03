using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Card_LightningStrike : Cards.Card
    {
        private static Color32  sm_electrictyColor = new Color32(10, 92, 255, 64);

        #region Properties

        public override string Name => "Lightning\nStrike";

        public override int ManaCost => 7;

        public override string IconName => "Lightning";

        #endregion

        public override List<(Vector2Int, Color32)> GetPreviewEffect(Vector2Int vMouseCoord)
        {
            return new List<(Vector2Int, Color32)>()
            {
                (vMouseCoord, sm_electrictyColor)
            };
        }

        public override void Perform(Vector2Int vMouseCoord)
        {
            Cave.Node node = Cave.Instance[vMouseCoord];
            Effects.CreateLightning((Vector3)node.Center + Vector3.up * 10.0f, node.Center);

            // take some damage
            if (node.Unit != null)
            {
                node.Unit.TakeDamage(Random.Range(40, 75));
            }
        }
    }
}
