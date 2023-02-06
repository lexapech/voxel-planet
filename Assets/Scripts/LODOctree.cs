using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UIElements;

public class LODOctree : MonoBehaviour
{
    [SerializeField, Range(1, 20)]
    public int Depth = 5;
    private int _chunkSize;
    [SerializeField, Range(1, 32)]
    public float LODFactor = 1f;

    private IChunkProvider _chunkProvider;
    private IMeshDataProvider _meshDataProvider;
    public LODNode Root { private set; get;}

    private List<MeshChunk> chunksRequiringActivation = new List<MeshChunk>();
    private List<MeshChunk> chunksRequiringDestruction = new List<MeshChunk>();

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

        public MeshChunk MeshChunk;

        public LODNode(int depth, Vector3 position)
        {
            Depth = depth;
            Position = position;
            Children = new LODNode[8];
        }
        
    }
    public LODOctree()
    {
        
        Root = new LODNode(0, Vector3.zero);
    }
    public void Init(IChunkProvider chunkProvider,IMeshDataProvider meshDataProvider,int chunkSize)
    {
        _chunkSize = chunkSize;
        _chunkProvider = chunkProvider;
        _meshDataProvider = meshDataProvider;
    }

    public void SetLODCenter(Vector3 center)
    {
        setLODCenter(Root, center);
    }
    private void setLODCenter(LODNode node, Vector3 center)
    {            
        if(node.Depth == Depth)
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
            if (node.isLeaf)
            {
                // If marking as leaf - add to create queue
                node.MeshChunk = createMeshChunk(node.Position, GetNodeSize(node) / _chunkSize);
                deleteChildNodes(node);
            }
            else if(node.MeshChunk!=null)
            {
                // otherwise - add to destroy queue

                chunksRequiringDestruction.Add(node.MeshChunk);
                node.MeshChunk = null;
                //ChunkProvider.Destroy(ref node.MeshChunk);
            }
        }
       
    }
    private void deleteChildNodes(LODNode node)
    {
        for(int i = 0; i < 8; i++)
        {
            if (node.Children[i]!=null)
            {
                deleteChildNodes(node.Children[i]);
                if (node.Children[i].MeshChunk)
                {
                    chunksRequiringDestruction.Add(node.Children[i].MeshChunk);
                }                   
                node.Children[i] = null;
            }
        }
    }

    private MeshChunk createMeshChunk(Vector3 position, float size)
    {
        MeshChunk meshChunk = _chunkProvider.Instantiate();
        meshChunk.Init(transform, position, size, _meshDataProvider);

        chunksRequiringActivation.Add(meshChunk);

        return meshChunk;
    }

    public void ApplyMeshTransition()
    {
        for (int i = 0; i < chunksRequiringDestruction.Count; i++)
        {
            _chunkProvider.Destroy(chunksRequiringDestruction[i]);
        }
        for (int i = 0; i < chunksRequiringActivation.Count; i++)
        {
            chunksRequiringActivation[i].GenerateMesh();
        }
        chunksRequiringDestruction.Clear();
        chunksRequiringActivation.Clear();


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
        return Mathf.Pow(2, Depth - node.Depth) * _chunkSize;
    }
    private float getDistanceToNode(Vector3 targetPosition, LODNode node)
    {
        Vector3 distance = Utility.VectorAbs(node.Position - targetPosition);
        var minDistance = Mathf.Max(distance.x, distance.y, distance.z);
        var nodeSize = Mathf.Pow(2, Depth - node.Depth) * _chunkSize;
        return minDistance - nodeSize * 0.5f;
    }

    public LODNode FindContainingNode(Vector3 point)
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
