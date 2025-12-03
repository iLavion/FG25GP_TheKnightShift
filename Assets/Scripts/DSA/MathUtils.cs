using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DSA
{
    public static class MathUtils 
    {
        public static float EaseIn(float f)
        {
            return f * f;
        }

        public static float EaseOut(float f)
        {
            return 1.0f - EaseIn(1.0f - f);
        }

        public static float SmoothStep(float f)
        {
            return f * f * (3.0f - 2.0f * f);
        }
    }
}