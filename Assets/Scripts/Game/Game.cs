using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEditor.EditorTools;
using UnityEngine;

namespace Game
{
    public class Game : MonoBehaviour
    {
        private Transform m_unitsParent;

        #region Properties

        public Transform UnitsParent
        {
            get
            {
                if (m_unitsParent == null)
                {
                    m_unitsParent = transform.Find("Units");
                    if(m_unitsParent == null)
                    {
                        GameObject go = new GameObject("Units");
                        go.transform.parent = transform;
                        m_unitsParent = go.transform;
                    }
                }

                return m_unitsParent;
            }
        }

        #endregion

        private void Start()
        {
            // initialize cards
            Cards cards = GetComponentInChildren<Cards>();
            cards.InitializeCards();
            GetComponentInChildren<CardsWidget>().Initialize(cards);

            // spawn heroes
            Unit.Create<Hero>(new Vector2(2, 6), UnitsParent);
            Unit.Create<Hero>(new Vector2(2, 4), UnitsParent);
            Unit.Create<Hero>(new Vector2(2, 2), UnitsParent);

            StartCoroutine(SkeletonSpawnLoop());
        }

        private void Update()
        {
            Time.timeScale = Input.GetKey(KeyCode.X) ? 4.0f : 1.0f;
        }

        IEnumerator SkeletonSpawnLoop()
        {
            while (Unit.AllUnits.FindIndex(u => u is Hero) >= 0)
            {
                // spawn random skeleton
                Vector2 vSpawnPos = new Vector2(Cave.WIDTH, Random.Range(0, Cave.HEIGHT));
                Unit.Create<Skeleton>(vSpawnPos, UnitsParent);
                yield return new WaitForSeconds(Random.Range(1.0f, 3.0f));
            }

            // game over!
            transform.Find("GameCanvas/GameOver").gameObject.SetActive(true);
        }
    }
}