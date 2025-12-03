using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DSA
{
    public class BinaryTree<T>
    {
        public class Node
        {
            public T        m_value;
            public Node     m_left;
            public Node     m_right;
        }

        public Node         m_root;

        #region Properties

        public int Depth => CalculateDepth(m_root, 0);

        #endregion

        public static int CalculateDepth(Node node, int iDepth)
        {
            if (node == null)
            {
                return iDepth;
            }

            return Mathf.Max(CalculateDepth(node.m_left, iDepth + 1),
                             CalculateDepth(node.m_right, iDepth + 1));
        }

        public static bool IsNodeBalanced(Node node)
        {
            return Mathf.Abs(CalculateDepth(node.m_left, 0) - CalculateDepth(node.m_right, 0)) <= 1;
        }

        public static int GetBalance(Node node)
        {
            if (node == null)
            {
                return 0;
            }

            return CalculateDepth(node.m_left, 0) - CalculateDepth(node.m_right, 0);
        }
    }
}