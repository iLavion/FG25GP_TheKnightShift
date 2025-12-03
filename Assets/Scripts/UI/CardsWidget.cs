using DSA;
using Game;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CardsWidget : MonoBehaviour
    {
        private class CardUI : MonoBehaviour
        {
            private Cards.Card              m_card;
            private bool                    m_bShowFront;

            private static List<Sprite>     sm_cardIcons = null;

            #region Properties

            public RectTransform RectTransform => GetComponent<RectTransform>();

            public bool ShowFront
            {
                get => m_bShowFront;
                set
                {
                    m_bShowFront = value;
                    transform.Find("FrontSide").gameObject.SetActive(m_bShowFront);
                    transform.Find("BackSide").gameObject.SetActive(!m_bShowFront);
                }
            }

            public bool IsMoving { get; private set; }

            #endregion

            public void Move(Vector3 vTarget, bool bShowFront, float fDelay = 0.0f)
            {
                StopAllCoroutines();

                if (Vector3.Distance(transform.position, vTarget) > 0.1f)
                {
                    StartCoroutine(MoveLogic(vTarget, bShowFront, fDelay));
                }
            }

            IEnumerator MoveLogic(Vector3 vTarget, bool bShowFront, float fDelay)
            {
                IsMoving = true;

                // initial delay?
                if (fDelay > float.Epsilon)
                {
                    yield return new WaitForSeconds(fDelay);
                }

                float fDistance = Vector2.Distance(vTarget, transform.position);
                float fTime = fDistance / 720.0f;
                
                bool bShowFrontSource = ShowFront;
                Vector3 vSource = transform.position;
                bool bRotate = bShowFront != ShowFront;

                for (float f = 0.0f; f < fTime; f += Time.deltaTime)
                {
                    // move card
                    float fPrc = Mathf.Clamp01(f / fTime);
                    float fHalf = 1.0f - Mathf.Clamp01(Mathf.Abs(fPrc - 0.5f) / 0.5f);
                    transform.position = Vector3.Lerp(vSource, vTarget, MathUtils.SmoothStep(fPrc)) + fHalf * fDistance * 0.3f * Vector3.up;

                    // rotate card?
                    if (bRotate)
                    {
                        transform.localEulerAngles = new Vector3(0.0f, fHalf * 90.0f, 0.0f);

                        if (fPrc >= 0.5f && ShowFront != bShowFront)
                        {
                            ShowFront = bShowFront;
                        }
                    }

                    yield return null;
                }

                transform.position = vTarget;
                transform.localEulerAngles = Vector3.zero;
                ShowFront = bShowFront;
                IsMoving = false;
            }

            public void Shake()
            {
                if (!IsMoving)
                {
                    StopAllCoroutines();
                    StartCoroutine(ShakeLogic());
                }
            }

            IEnumerator ShakeLogic()
            {
                const float SHAKE_DURATION = 0.6f;
                AnimationCurve amountCurve = new AnimationCurve(new Keyframe[]
                {
                    new Keyframe(0.0f, 0.0f),
                    new Keyframe(SHAKE_DURATION * 0.5f, 16.0f),
                    new Keyframe(SHAKE_DURATION, 0.0f),
                });

                Vector3 vSource = transform.position;
                for (float f = 0.0f; f <= SHAKE_DURATION; f += Time.deltaTime)
                {
                    float fAmount = amountCurve.Evaluate(f);
                    transform.position = vSource + Vector3.right * fAmount * Mathf.Sin(Time.time * 24.0f);
                    yield return null;
                }
                transform.position = vSource;
            }

            public static CardUI Create(Cards.Card card, GameObject template)
            {
                if (sm_cardIcons == null)
                {
                    sm_cardIcons = new List<Sprite>(Resources.LoadAll<Sprite>("Textures/card_icons"));
                }

                GameObject go = Instantiate(template, template.transform.parent);
                go.name = card.GetType().Name;
                go.SetActive(true);
                go.transform.Find("FrontSide/CardName").GetComponent<Text>().text = card.Name;
                go.transform.Find("FrontSide/ManaCost").GetComponent<Text>().text = card.ManaCost.ToString();
                go.transform.Find("FrontSide/CardIcon").GetComponent<Image>().sprite = sm_cardIcons.Find(s => s.name == card.IconName);
                CardUI cu = go.AddComponent<CardUI>();
                cu.m_card = card;
                cu.ShowFront = false;
                return cu;
            }
        }

        private Cards                           m_cards;
        private GameObject                      m_cardTemplate;
        private Transform                       m_drawStackPosition;
        private Transform                       m_discardStackPosition;
        private Transform[]                     m_handPositions;
        private Dictionary<Cards.Card, CardUI>  m_cardUI = new Dictionary<Cards.Card, CardUI>();
        private Cards.Card[]                    m_hand;
        private Cards.Card                      m_selectedCard;
        private Mesh                            m_tileMesh;
        private Dictionary<Color32, Material>   m_tileMaterials = new Dictionary<Color32, Material>();
        private Camera                          m_camera;

        private static Material                 sm_tile;

        const int                               HAND_COUNT = 5;

        #region Properties

        protected Cards.Card SelectedCard
        {
            get => m_selectedCard;
            set
            {
                // cannot afford?
                if (value != null && 
                    ManaWidget.Instance.Mana < value.ManaCost)
                {
                    CardUI cu = m_cardUI[value];
                    cu.Shake();
                }
                else
                {
                    m_selectedCard = value;
                    for (int i = 0; i < HAND_COUNT; ++i)
                    {
                        Cards.Card card = m_hand[i];
                        if (card != null)
                        {
                            bool bSelected = card == m_selectedCard;
                            CardUI cu = m_cardUI[card];
                            cu.Move(this[i] + Vector3.up * (bSelected ? 48.0f : 0.0f), cu.ShowFront);
                        }
                    }
                }
            }
        }

        protected Vector3 this[int i] => Vector3.Lerp(m_handPositions[0].position, m_handPositions[1].position, i / (float)(HAND_COUNT - 1));

        protected bool IsDoingTheShuffle { get; private set; }

        #endregion

        public void Initialize(Cards cards)
        {
            try
            {
                m_cards = cards;
                m_camera = GetComponentInParent<Game.Game>().GetComponentInChildren<Camera>();
                IsDoingTheShuffle = true;

                CreateTileMesh();

                // grab positions and templates
                m_drawStackPosition = transform.Find("DrawStack");
                m_discardStackPosition = transform.Find("DiscardStack");
                m_handPositions = new Transform[] { transform.Find("HandStart"), transform.Find("HandEnd") };
                m_cardTemplate = transform.Find("CardTemplate").gameObject;

                // Create cardUIs for draw stack
                Vector3 vPosition = m_drawStackPosition.position;
                foreach (Cards.Card card in cards.DrawCards)
                {
                    CardUI cu = CardUI.Create(card, m_cardTemplate);
                    cu.RectTransform.position = vPosition;
                    vPosition -= Vector3.one * 4;
                    m_cardUI[card] = cu;
                    cu.RectTransform.SetAsFirstSibling();
                }

                // move cards to hand
                m_hand = new Cards.Card[HAND_COUNT];
                for (int i = 0; i < HAND_COUNT; i++)
                {
                    Cards.Card next = m_cards.DrawCard();
                    CardUI cu = m_cardUI[next];
                    cu.RectTransform.SetAsLastSibling();
                    cu.Move(this[i], true, 3.0f + i * 0.4f);
                    m_hand[i] = next;
                    Button btn = cu.GetComponentInChildren<Button>(true);
                    btn.onClick.AddListener(() => { SelectCard(next); });
                }

                IsDoingTheShuffle = false;
            }
            catch (System.NotImplementedException)
            {
                Debug.Log("Cards not done?");
            }
        }

        private void SelectCard(Cards.Card card)
        {
            // wait for moves
            if (System.Array.FindIndex(m_hand, h => h != null && m_cardUI[h].IsMoving) >= 0)
            {
                return;
            }

            if (!IsDoingTheShuffle)
            {
                SelectedCard = SelectedCard == card ? null : card;
            }
        }

        private void Update()
        {
            if (IsDoingTheShuffle)
            {
                return;
            }

            // raycast against cave
            Ray mr = m_camera.ScreenPointToRay(Input.mousePosition);
            Plane ground = new Plane(Vector3.back, Vector3.zero);
            float fEnter;
            Vector2Int vMouseCoord = Vector2Int.zero;
            if (ground.Raycast(mr, out fEnter))
            {
                Vector3 vHit = mr.GetPoint(fEnter);
                vMouseCoord = new Vector2Int(Mathf.FloorToInt(vHit.x), Mathf.FloorToInt(vHit.y));
                if (!Cave.WithinBounds(vMouseCoord))
                {
                    return;
                }
            }

            if (SelectedCard != null)
            {
                // get card preview area
                List<(Vector2Int, Color32)> previewArea = SelectedCard.GetPreviewEffect(vMouseCoord);
                foreach ((Vector2Int vCoord, Color32 color) pt in previewArea)
                {
                    DrawTile(pt.vCoord, pt.color);
                }

                // fire spell?
                if (Input.GetMouseButtonDown(0))
                {
                    StartCoroutine(PlayCard(SelectedCard, vMouseCoord));
                }
            }
            else
            {
                DrawTile(vMouseCoord, new Color32(0, 0, 0, 32));
            }
        }

        private void DrawTile(Vector2Int vCoord, Color32 color)
        {
            if (!Cave.WithinBounds(vCoord))
            {
                return;
            }

            // has color material?
            if (!m_tileMaterials.ContainsKey(color))
            {
                Material m = new Material(sm_tile);
                m.color = color;
                m_tileMaterials[color] = m;
            }

            // draw tile
            Matrix4x4 mWorld = Matrix4x4.Translate(new Vector3(vCoord.x, vCoord.y, 0.001f));
            Graphics.DrawMesh(m_tileMesh, mWorld, m_tileMaterials[color], 0);
        }

        private void CreateTileMesh()
        {
            sm_tile = Resources.Load<Material>("Materials/Tile");

            m_tileMesh = new Mesh();
            m_tileMesh.name = "TileMesh";
            m_tileMesh.hideFlags = HideFlags.DontSave;
            m_tileMesh.subMeshCount = 1;

            m_tileMesh.vertices = new Vector3[]
            {
                new Vector3(0.0f, 0.0f, 0.0f),
                new Vector3(0.0f, 1.0f, 0.0f),
                new Vector3(1.0f, 1.0f, 0.0f),
                new Vector3(1.0f, 0.0f, 0.0f),
            };

            m_tileMesh.SetTriangles(new int[] { 0, 1, 2, 0, 2, 3 }, 0);
            m_tileMesh.RecalculateNormals();
            m_tileMesh.RecalculateBounds();
        }

        private IEnumerator PlayCard(Cards.Card card, Vector2Int vTargetCoord)
        {
            if (card == null || !ManaWidget.Instance.ConsumeMana(card.ManaCost))
            {
                yield break;
            }

            int iHandIndex = System.Array.IndexOf(m_hand, card);

            // perform spell!
            SelectedCard.Perform(vTargetCoord);

            // discard card
            m_cards.DiscardCard(SelectedCard);

            // UI cleanup
            CardUI cu = m_cardUI[SelectedCard];
            Button btn = cu.GetComponentInChildren<Button>(true);
            btn.onClick.RemoveAllListeners();
            m_hand[iHandIndex] = null;
            SelectedCard = null;

            // move to discard pile
            cu.Move(m_discardStackPosition.position + m_cards.DiscardCards.Count * Vector3.one * 4, false);

            // shuffle?
            if (m_cards.DrawCards.Count == 0)
            {
                yield return ShuffleCards();
            }

            // draw new card
            Cards.Card next = m_cards.DrawCard();
            cu = m_cardUI[next];
            cu.RectTransform.SetAsLastSibling();
            cu.Move(this[iHandIndex], true);
            m_hand[iHandIndex] = next;
            btn = cu.GetComponentInChildren<Button>(true);
            btn.onClick.AddListener(() => { SelectCard(next); });
        }

        private IEnumerator ShuffleCards()
        {
            IsDoingTheShuffle = true;
            SelectedCard = null;
            Vector3 vShuffleCenter = Vector3.Lerp(m_handPositions[0].position, m_handPositions[1].position, 0.5f) + Vector3.up * 128.0f;

            // get discard cards
            List<CardUI> shuffleCards = new List<CardUI>();
            foreach (Cards.Card card in m_cards.DiscardCards)
            {
                CardUI cu = m_cardUI[card];
                shuffleCards.Add(cu);
                cu.RectTransform.SetAsLastSibling();
            }

            // shuffle for a while
            for (float f = 0.0f; f < 3.0f; f += Time.deltaTime)
            {
                foreach (CardUI cu in shuffleCards)
                {
                    if(!cu.IsMoving)
                    {
                        cu.Move(vShuffleCenter + (Vector3)Random.insideUnitCircle * 50.0f, false);
                    }
                }
                yield return null;
            }

            // do ACTUAL shuffle
            m_cards.ShuffleDeck();

            // move to draw stack
            Vector3 vPosition = m_drawStackPosition.position;
            float fDelay = 0.0f;
            foreach (CardUI cu in shuffleCards)
            {
                cu.Move(vPosition, false, fDelay);
                vPosition -= Vector3.one * 4;
                fDelay += 0.15f;
                cu.RectTransform.SetAsFirstSibling();
            }

            // wait for move
            while (shuffleCards.FindIndex(cu => cu.IsMoving) >= 0)
            {
                yield return null;
            }

            IsDoingTheShuffle = false;
        }
    }
}