using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Cards : MonoBehaviour
    {
        public abstract class Card
        {
            #region Properties

            public abstract string Name { get; }

            public abstract int ManaCost { get; }

            public abstract string IconName { get; }

            #endregion

            public abstract List<(Vector2Int, Color32)> GetPreviewEffect(Vector2Int vMouseCoord);

            public abstract void Perform(Vector2Int vMouseCoord);
        }

        private static Cards    sm_instance;

        #region Properties

        // TODO: Create and store a Stack and return here
        public Stack<Card> DrawCards => throw new System.NotImplementedException();

        // TODO: Create and store a Stack and return here
        public Stack<Card> DiscardCards => throw new System.NotImplementedException();

        public static Cards Instance => sm_instance;

        #endregion

        private void Awake()
        {
            sm_instance = this;
        }

        public void InitializeCards()
        {
            // NOTHING TO DO IN THIS FUNCTION... Leave as is :)

            try
            {
                // find all card types
                List<System.Type> cardTypes = new List<System.Type>(System.Array.FindAll(typeof(Card).Assembly.GetTypes(), t => !t.IsAbstract && typeof(Card).IsAssignableFrom(t)));

                // create cards
                List<Card> cards = new List<Card>();
                for (int i = 0; i < 10; ++i)
                {
                    System.Type t = cardTypes[i % cardTypes.Count];
                    Card card = System.Activator.CreateInstance(t) as Card;
                    if (card != null)
                    {
                        cards.Add(card);
                    }
                }

                // shuffle cards
                for (int i = 0; i < cards.Count; ++i)
                {
                    int i1 = Random.Range(0, DrawCards.Count);
                    int i2 = Random.Range(0, DrawCards.Count);
                    if (i1 != i2)
                    {
                        Card tmp = cards[i1];
                        cards[i1] = cards[i2];
                        cards[i2] = tmp;
                    }
                }

                // push onto draw stack
                foreach (Card card in cards)
                {
                    DrawCards.Push(card);
                }
            }
            catch (System.NotImplementedException)
            {
                Debug.Log("Looks like you have some work to do?");
            }
        }

        public Card DrawCard()
        {
            // TODO: get the next card on the DrawCards stack and return it
            // (removing it from the stack in the process)
            throw new System.NotImplementedException();
        }

        public void DiscardCard(Card card)
        {
            // TODO: push the card being passed in to this function
            // on the DiscardStack
            throw new System.NotImplementedException();
        }

        public void ShuffleDeck()
        {
            // TODO: 
            // 1. Add ALL the cards from the DiscardStack into a new temporary List
            // 2. Shuffle the new list by randomly swapping elements in the list 100 times
            // 3. Clear the DiscardCards stack
            // 4. Add all the Cards from your temporary (now shuffled) list into the DrawStack
            throw new System.NotImplementedException();
        }
    }
}