using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ManaWidget : MonoBehaviour
    {
        private int                 m_iMana;
        private Image               m_manaImage;
        private Text                m_manaCount;

        private static ManaWidget   sm_instance;

        const int MAX_MANA = 100;

        #region Properties

        public int Mana => m_iMana;

        public float ManaAmountTarget => Mathf.Clamp01(m_iMana / (float)MAX_MANA);

        public static ManaWidget Instance => sm_instance;

        #endregion

        private void Awake()
        {
            m_iMana = MAX_MANA / 2;
            sm_instance = this;

            m_manaImage = transform.Find("Mana").GetComponent<Image>();
            m_manaImage.fillAmount = 0.5f;

            m_manaCount = transform.Find("ManaCount").GetComponent<Text>();
            UpdateText();

            StartCoroutine(ManaRegeneration());
        }

        private void Update()
        {
            // smooth update of mana bowl
            m_manaImage.fillAmount = Mathf.Clamp01(m_manaImage.fillAmount + (ManaAmountTarget - m_manaImage.fillAmount) * Time.deltaTime * 0.5f);
        }

        public bool ConsumeMana(int iAmount)
        {
            if (iAmount <= m_iMana)
            {
                m_iMana -= iAmount;
                UpdateText();
                return true;
            }

            return false;
        }

        private void UpdateText()
        {
            m_manaCount.text = m_iMana + " / " + MAX_MANA;
        }

        IEnumerator ManaRegeneration()
        {
            yield return new WaitForSeconds(1.0f);
            while (true)
            {
                m_iMana++;
                UpdateText();
                yield return new WaitForSeconds(1.5f);
            }
        }
    }
}