using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Burning : DamageOverTime
    {
        private readonly int m_damagePerTick;

        public Burning(float fDuration, int damagePerTick = 3) : base(fDuration)
        {
            m_damagePerTick = damagePerTick;
        }

        protected override void ApplyEffect(Unit unit)
        {
            if (unit == null) return;
            unit.TakeDamage(m_damagePerTick);
        }
    }
}
