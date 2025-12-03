using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Card_Heal : Cards.Card
    {
        private static Color32  sm_healColor = new Color32(32, 255, 32, 64);

        #region Properties

        public override string Name => "Heal";

        public override int ManaCost => 6;

        public override string IconName => "Heal";

        #endregion

        public override List<(Vector2Int, Color32)> GetPreviewEffect(Vector2Int vMouseCoord)
        {
            return new List<(Vector2Int, Color32)>()
            {
                (vMouseCoord, sm_healColor)
            };
        }

        public override void Perform(Vector2Int vMouseCoord)
        {
            // get node at coord
            Cave.Node node = Cave.Instance[vMouseCoord];
            if (node == null ||
                node.Unit == null)
            {
                return;
            }

            // TODO: perhaps a nice particle fx?
            //       play a healing sound perhaps?  :)

            if (node.Unit is Hero hero)
            {
                node.Unit.TakeDamage(-50);          // <-- give some health to hero
            }
            else if (node.Unit is Skeleton skeleton)
            {
                node.Unit.TakeDamage(25);           // <-- give some damage to undead!
            }
        }
    }
}
