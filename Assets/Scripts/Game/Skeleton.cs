using DSA;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Skeleton : Unit
    {
        static int[] SKELETONS = new int[] { 0, 2, 3 };

        #region Properties

        protected override int MeshIndex => SKELETONS[Random.Range(0, SKELETONS.Length)];

        protected override int AttackDamage => Random.Range(6, 12);

        protected override int MaxHealth => 50;

        #endregion

        protected override void Start()
        {
            base.Start();
            StartCoroutine(SkeletonAI());
        }

        IEnumerator SkeletonAI()
        {
            int iStuckCounter = 0;

            while (true)
            {
                Hero hero = GetClosestUnit<Hero>(transform.position);
                if (hero != null)
                {
                    Vector2Int vToHero = hero.Node.Coord - Node.Coord;

                    // is next to hero?
                    if (Mathf.Abs(vToHero.x) <= 1 &&
                        Mathf.Abs(vToHero.y) <= 1)
                    {
                        // Attack!
                        yield return AttackUnit(hero);
                    }
                    else
                    {
                        // Move!
                        bool bXMove = Random.value < (Mathf.Abs(vToHero.x) / (float)(Mathf.Abs(vToHero.x) + Mathf.Abs(vToHero.y)));
                        Vector2Int vMove = bXMove ? new Vector2Int(vToHero.x < 0 ? -1 : 1, 0) :
                                                    new Vector2Int(0, vToHero.y < 0 ? -1 : 1);

                        // stuck? do random Y move!
                        if (iStuckCounter > 3)
                        {
                            vMove = new Vector2Int(0, Random.value < 0.5f ? -1 : 1);
                        }

                        Vector2Int vTargetCoord = Node.Coord + vMove;
                        Cave.Node targetNode = Cave.Instance[vTargetCoord];

                        // got valid target?
                        if (targetNode != null && 
                            targetNode.Unit == null)
                        {
                            iStuckCounter = 0;
                            Node = targetNode;
                            Vector3 vSource = transform.position;
                            for (float f = 0.0f; f < 1.0f; f += Time.deltaTime * 2.0f)
                            {
                                float fJump = 1.0f - MathUtils.EaseOut(Mathf.Clamp01(Mathf.Abs(f - 0.5f) / 0.5f));
                                transform.position = Vector3.Lerp(vSource, Node.Center, MathUtils.SmoothStep(f)) + fJump * Vector3.up * 0.3f;
                                yield return null;
                            }
                        }
                        else
                        {
                            iStuckCounter++;
                        }
                    }
                }

                yield return new WaitForSeconds(Random.Range(1.0f, 2.0f));
            }
        }
    }
}