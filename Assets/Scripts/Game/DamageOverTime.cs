using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public abstract class DamageOverTime
    {
        private float m_fTime = 0.0f;
        private float m_fTimeToLive = 1.0f;

        #region Properties

        public bool IsDone => m_fTimeToLive <= 0.0f;

        #endregion

        public DamageOverTime(float fDuration)
        {
            m_fTimeToLive = fDuration;
        }

        public void Update(Unit unit)
        {
            // This function should be called once per frame by the unit on which
            // the DamageOverTime effect is active.

            // apply effect once per second
            m_fTime += Time.deltaTime;
            if(m_fTime >= 1.0f)
            {
                m_fTime -= 1.0f;
                ApplyEffect(unit);
            }

            // reduce time to live
            m_fTimeToLive -= Time.deltaTime;
        }

        protected abstract void ApplyEffect(Unit unit);
    }
}