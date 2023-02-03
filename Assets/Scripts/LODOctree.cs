using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class LODOctree : MonoBehaviour
{
    [SerializeField, Range(1, 20)]
    public int Depth = 5;
    [SerializeField, Range(16, 128)]
    public int ChunkSize = 32;
    [SerializeField, Range(1, 32)]
    public float LODFactor = 1f;
    public LODNode Root { private set; get;}

    static readonly Vector3[] NODE_OFFSETS =
    {
         new Vector3(-1, -1, -1),
         new Vector3(-1, -1, 1),
         new Vector3(-1, 1, -1),
         new Vector3(-1, 1, 1),
         new Vector3(1, -1, -1),
         new Vector3(1, -1, 1),
         new Vector3(1, 1, -1),
         new Vector3(1, 1, 1)
    };

    public class LODNode
    {
        public int Depth { private set; get; }
        public Vector3 Position { private set; get; }
        public LODNode[] Children { private set; get; }

        public bool isLeaf;

        private MeshGenerator _meshGenerator;

        public LODNode(int depth, Vector3 position)
        {
            Depth=depth;
            Position = position;
            Children = new LODNode[8];
        }
        
    }
    public LODOctree()
    {
        Root = new LODNode(0, Vector3.zero);
    }

    public void SetLODCenter(Vector3 center)
    {
        setLODCenter(Root, center);
    }
    private void setLODCenter(LODNode node, Vector3 center)
    {            
        if(node.Depth==Depth)
        {
            setIsLeaf(node, true);
            // Stop at max depth
        }
        else if(isNear(center,node))
        {
            // Near - expand tree
            setIsLeaf(node, false);
            for (int i = 0; i < 8; i++)
            {
                if (node.Children[i] == null)
                {
                    node.Children[i] = new LODNode(node.Depth + 1, getChildPosition(node, i));
                }
                setLODCenter(node.Children[i], center);
            }
            
        }
        else
        {
            setIsLeaf(node, true);
            // Too far - no recursion
        }
    }

    private void setIsLeaf(LODNode node, bool isLeaf)
    {
        if(node.isLeaf != isLeaf)
        {
            node.isLeaf = isLeaf;
            // if marking as leaf - add to create queue
            // otherwise - add to destroy queue
        }
       
    }

    private Vector3 getChildPosition(LODNode node, int index)
    {
        return node.Position + NODE_OFFSETS[index] * GetNodeSize(node) / 4;
    }

    private bool isNear(Vector3 targetPosition, LODNode node)
    {
        return getDistanceToNode(targetPosition, node) < GetNodeSize(node) * LODFactor;
    }
    public float GetNodeSize(LODNode node)
    {
        return Mathf.Pow(2, Depth - node.Depth) * ChunkSize;
    }
    private float getDistanceToNode(Vector3 targetPosition, LODNode node)
    {
        Vector3 distance = Utility.VectorAbs(node.Position - targetPosition);
        var minDistance = Mathf.Max(distance.x, distance.y, distance.z);
        var nodeSize = Mathf.Pow(2, Depth - node.Depth) * ChunkSize;
        return minDistance - nodeSize * 0.5f;
    }

    public LODNode FindContaningNode(Vector3 point)
    {
        var worldSize = GetNodeSize(Root) / 2;
        if (Utility.VectorAbs(point).IsComponentWiseGreaterOrEqual(worldSize)) return null;
        var currentNode = Root;
        while (!currentNode.isLeaf)
        {
            var childIndex = getOctantIndex(point - currentNode.Position);
            currentNode = currentNode.Children[childIndex];
        }
        return currentNode;
    }

    private int getOctantIndex(Vector3 position)
    {
        int x = position.x >= 0 ? 4 : 0;
        int y = position.y >= 0 ? 2 : 0;
        int z = position.z >= 0 ? 1 : 0;
        return x + y + z;
    }

}
