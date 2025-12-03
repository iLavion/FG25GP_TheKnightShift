using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace DSA
{
    public static class QuickSort
    {
        public static int Partition(int[] array, int iStart, int iEnd)
        {
            // select the pivot value
            int iPivotValue = array[(iStart + iEnd) / 2];
            int iLeft = iStart;
            int iRight = iEnd;

            while (iLeft <= iRight)
            {
                // move left index until a value greater or equal to the pivot is found
                while (array[iLeft] < iPivotValue)
                {
                    iLeft++;
                }

                // move right index until a value less or equal to the pivot is found
                while (array[iRight] > iPivotValue)
                {
                    iRight--;
                }

                // should we swap?
                if (iLeft <= iRight)
                {
                    // ... otherwise swap
                    int iTemp = array[iLeft];
                    array[iLeft] = array[iRight];
                    array[iRight] = iTemp;

                    iLeft++;
                    iRight--;
                }
            }

            return iLeft;
        }

        public static void Sort(int[] array)
        {
            Sort(array, 0, array.Length - 1);
        }

        static void Sort(int[] array, int iStart, int iEnd)
        {
            if (iStart < iEnd)
            {
                // Partition the array
                int iPivotIndex = Partition(array, iStart, iEnd);

                // Send left side off to QuickSort
                Sort(array, iStart, iPivotIndex - 1);

                // Send right side off to QuickSort
                Sort(array, iPivotIndex, iEnd);        // <-- iPivotIndex + 1 :(
            }
        }
    }
}