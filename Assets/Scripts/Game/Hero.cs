using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Hero : Unit
    {
        #region Properties

        protected override int MeshIndex => 1;

        protected override int AttackDamage => Random.Range(20, 30);

        protected override int MaxHealth => 200;

        #endregion

        protected override void Start()
        {
            base.Start();
            StartCoroutine(HeroAI());
        }

        IEnumerator HeroAI()
        {
            while (true)
            {
                Skeleton skeleton = GetClosestUnit<Skeleton>(transform.position);
                if (skeleton != null)
                {
                    Vector2Int vToEnemy = skeleton.Node.Coord - Node.Coord;

                    // is next to hero?
                    if (Mathf.Abs(vToEnemy.x) <= 1 &&
                        Mathf.Abs(vToEnemy.y) <= 1)
                    {
                        // Attack!
                        yield return AttackUnit(skeleton);
                    }
                }

                yield return new WaitForSeconds(Random.Range(1.0f, 2.0f));
            }
        }
    }
}