using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace DSA
{
    public class BinarySearchTree : BinaryTree<int>
    {
        #region Properties

        #endregion

        public virtual void Add(int elem)
        {
            m_root = Insert(m_root, elem);
        }

        protected virtual Node Insert(Node node, int elem)
        {
            // create new node
            if (node == null)
            {
                return new Node { m_value = elem };
            }

            // insert into left or right subtree
            if (elem <= node.m_value)
            {
                node.m_left = Insert(node.m_left, elem);
            }
            else
            {
                node.m_right = Insert(node.m_right, elem);
            }

            // return node
            return node;
        }

        public virtual void Remove(int elem)
        {
            m_root = Remove(m_root, elem);
        }

        protected virtual Node Remove(Node node, int elem)
        {
            if (node == null)
            {
                return null;
            }

            if (elem < node.m_value)
            {
                node.m_left = Remove(node.m_left, elem);
            }
            else if (elem > node.m_value)
            {
                node.m_right = Remove(node.m_right, elem);
            }
            else
            {
                if (node.m_left == null &&
                    node.m_right == null)
                {
                    // no kids? just delete node
                    return null;
                }
                else if (node.m_left != null &&
                         node.m_right != null)
                {
                    // got both kids?... complicated
                    // find node to replace self
                    Node successor = GetSuccessor(node);

                    // copy value from successor
                    node.m_value = successor.m_value;

                    // remove successor
                    node.m_right = Remove(node.m_right, node.m_value);
                }
                else if (node.m_left != null)
                {
                    // let the left kid replace node
                    return node.m_left;
                }
                else if (node.m_right != null)
                {
                    // let the right kid replace node
                    return node.m_right;
                }
            }

            return node;
        }

        protected Node GetSuccessor(Node node)
        {
            node = node.m_right;
            while (node != null && node.m_left != null)
            {
                node = node.m_left;
            }

            return node;
        }
    }
}