using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Profiling;

public class MeshGenerator
{
    
    private GridData _data;
    private World world;
    private Vector3 _position;
    private float _sizeMultiplier;
    public bool Valid;
    // TODO: Chunk data provider

    public MeshGenerator(Vector3 position, float sizeMultiplier,IMeshDataProvider meshDataProvider)
    {
        _sizeMultiplier = sizeMultiplier;
        _position = position;
        Profiler.BeginSample("Get grid data");
        _data = meshDataProvider.GetGridData(position, sizeMultiplier);
        Profiler.EndSample();
    }



    public void Initialize(World world, Vector3Int position, GridData data, float sizeMultiplier = 1f)
    {
        throw new NotImplementedException();  
    }

    public Mesh generateMesh()
    {
        Profiler.BeginSample("find edges");
        var activeEdges = findEdges();
        Profiler.EndSample();
        Profiler.BeginSample("find cells");
        var vertices = findActiveCells(activeEdges);
        Profiler.EndSample();
        Profiler.BeginSample("create mesh");
        var mesh = createMeshFlat(activeEdges, vertices);
        Profiler.EndSample();

        return mesh;
    }

    
    //TODO: getVAlue+IsActive

    private float getEdgeValue(Edge edge)
    {
        Vector3Int endPosition = edge.startPosition + Utility.EdgeDirToVector(edge.direction);
        var startChunkData = _data;
        var endChunkData = _data;
        var startPosition = edge.startPosition;
        var vertex1 = 0;
        var vertex2 = 0;
        if (startPosition.IsComponentWiseGreaterOrEqual(_data.Size))
        {
            var startChunk = getNextChunk(startPosition);
            if (startChunk!=null)
            {
                startChunkData = startChunk._data;
                startPosition = Utility.VectorMod(startPosition, _data.Size);
                vertex1 = startChunkData.volumes.Get3D(startPosition);
            }
        }
        else
        {
            vertex1 = startChunkData.volumes.Get3D(startPosition);
        }
        if (endPosition.IsComponentWiseGreaterOrEqual(_data.Size))
        {
            var endChunk = getNextChunk(endPosition);
            if (endChunk != null)
            {              
                endPosition = Utility.VectorMod(endPosition, _data.Size);
                endChunkData = endChunk._data;
                vertex2 = endChunkData.volumes.Get3D(endPosition);
            }
        }   
        else
        {
            vertex2 = endChunkData.volumes.Get3D(endPosition);
        }
        if (vertex1 < vertex2)
        {
            return (384 - vertex1 - vertex2)/255f;
        }
        else
        {
            return (128 - vertex1 - vertex2)/ 255f;
        }       
    }

    private MeshGenerator getNextChunk(Vector3Int endPosition)
    {
        Vector3Int chunkOffset = endPosition/_data.Size;
        MeshGenerator nextChunk;
        world.chunks.TryGetValue(Utility.VectorCeil(_position + chunkOffset), out nextChunk);
        return nextChunk;
    }

    private bool isEdgeActive(Vector3Int startPosition, Vector3Int endPosition)
    {
        ref var startChunkData = ref _data;
        ref var endChunkData = ref _data;
        if (startPosition.IsComponentWiseGreaterOrEqual(_data.Size))
        {
            var startChunk = getNextChunk(startPosition);
            if (startChunk != null)
            {
                startChunkData = startChunk._data;
                startPosition = Utility.VectorMod(startPosition, _data.Size);
            }
            else
            {
                return false;
            }
        }
        if (endPosition.IsComponentWiseGreaterOrEqual(_data.Size))
        {
            
            var endChunk = getNextChunk(endPosition);
            if (endChunk != null)
            {

                endPosition = Utility.VectorMod(endPosition, _data.Size);
                endChunkData = endChunk._data;
            }
            else
            {
                return startChunkData.volumes.Get3D(startPosition) >= 128;
            }
           
        }
        return (startChunkData.volumes.Get3D(startPosition) < 128 &&
                endChunkData.volumes.Get3D(endPosition) >= 128) ||
                (startChunkData.volumes.Get3D(startPosition) >= 128 &&
                endChunkData.volumes.Get3D(endPosition) < 128);
       
    }

    private List<Edge> findEdges()
    {
         var edges = new List<Edge>();
         //var cells = _data.Size+2;
         var cells = _data.Size-1;
         var directionsArray = (Edge.Directions[])Enum.GetValues(typeof(Edge.Directions));
         Vector3Int pos = Vector3Int.zero;
         for (int i = 0; i < directionsArray.Length; i++)
         {
             var direction = Utility.EdgeDirToVector(directionsArray[i]);
             for (pos.z = 0; pos.z < cells; pos.z++)
             {
                 for (pos.y = 0; pos.y < cells; pos.y++)
                 {
                     for (pos.x = 0; pos.x < cells; pos.x++)
                     {
                         if (isEdgeActive(pos, pos+direction))
                         {                           
                             Edge edge = new Edge(pos, directionsArray[i]);
                             edge.value = getEdgeValue(edge);                      
                             edges.Add(edge);                        
                         }                      
                     }
                 }
             }
         }
         return edges;              
    }

    private Vertex tryGetOrAddVertex(Dictionary<Vector3Int, Vertex> vertices,Vector3Int vertexCell,Edge edge)
    {
        Vertex vertex;
        if(!vertices.TryGetValue(vertexCell, out vertex))
        {
            vertex = new Vertex();
            vertex.position = Vector3.zero;
            vertex.edges = 0;
            vertices.Add(vertexCell, vertex);
        }
        vertex.position += edge.startPosition + (Vector3)Utility.EdgeDirToVector(edge.direction) * Math.Abs(edge.value);
        vertex.edges++;
        return vertex;

    }

    private Mesh createMesh(List<Edge> activeEdges,Vertex[] vertices)
    {
        Mesh mesh = new Mesh();
        List<int> tris = new List<int>();
        for (int i = 0; i < activeEdges.Count; i++)
        {        
            var edge = activeEdges[i];
            
                //if (edge.direction!= Edge.Directions.Y) continue;
                var edgeEnd = edge.startPosition + Utility.EdgeDirToVector(edge.direction);
            if (edgeEnd.IsComponentWiseGreaterOrEqual(_data.Size + 1)) continue;
            if (edgeEnd.x == 0 || edgeEnd.y == 0 || edgeEnd.z == 0) continue;
            int[] order;
            if (edge.value <= 0 ^ edge.direction==Edge.Directions.Y)
                order = new int[]{ 0, 3, 2, 0, 1, 3 };    
            else
                order = new int[] { 3, 1, 0, 2, 3, 0 };
            tris.AddRange(Enumerable.Select(order, x => edge.vertices[x].index));
        } 
        mesh.vertices = vertices.Select(x => x.position).ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();
        return mesh;
    }

    private Mesh createMeshFlat(List<Edge> activeEdges, Vertex[] vertices)
    {
        Mesh mesh = new Mesh();
        List<int> tris = new List<int>();
        List<Vector3> verticesList = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        for (int i = 0; i < activeEdges.Count; i++)
        {
            var edge = activeEdges[i];

            //if (edge.direction!= Edge.Directions.Y) continue;
            var edgeEnd = edge.startPosition + Utility.EdgeDirToVector(edge.direction);
            if (edgeEnd.IsComponentWiseGreaterOrEqual(_data.Size + 1)) continue;
            if (edgeEnd.x == 0 || edgeEnd.y == 0 || edgeEnd.z == 0) continue;
            int[] order;
            
            if (edge.value <= 0 ^ edge.direction == Edge.Directions.Y)
                order = new int[] { 0, 3, 2, 0, 1, 3 };
            else
                order = new int[] { 2, 3, 0, 3, 1, 0};
            tris.AddRange(Enumerable.Select(order, x => verticesList.Count+x));
           /* Vector3 vec1 = edge.vertices[order[5]].position - edge.vertices[order[4]].position;
            Vector3 vec2 = edge.vertices[order[3]].position - edge.vertices[order[4]].position;
            normals.AddRange(Enumerable.Select(edge.vertices, x => Vector3.Cross(vec1,vec2).normalized));*/
            verticesList.AddRange(Enumerable.Select(edge.vertices, x => (x.position-Vector3.one*_data.Size/2) * _sizeMultiplier));
        }  
        mesh.vertices = verticesList.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();
        //mesh.normals = normals.ToArray();
        return mesh;
    }
    private Vertex[] findActiveCells(List<Edge> activeEdges)
    {
        int gridSize = _data.Size+1;
        var vertices = new Dictionary<Vector3Int, Vertex>();
        for (int i = 0; i < activeEdges.Count; i++)
        {
            Profiler.BeginSample("edge processing");
            var edge = activeEdges[i];
            var start = edge.startPosition - Vector3Int.one + Utility.EdgeDirToVector(edge.direction);
            var end = edge.startPosition;
            start.Clamp(Vector3Int.zero, Vector3Int.one * gridSize);
            end.Clamp(Vector3Int.zero,Vector3Int.one * gridSize);
            for (int z = start.z; z <= end.z; z++)
            {
                for (int y = start.y; y <= end.y; y++)
                {
                    for (int x = start.x; x <= end.x; x++)
                    {
                        var vertex = tryGetOrAddVertex(vertices, new Vector3Int(x, y, z), edge);
                        edge.AddVertex(vertex);
                    }
                }
            }
            Profiler.EndSample();

        }
        Profiler.BeginSample("normalization");
        var verticesList = vertices.Values.ToArray();
        for (int i = 0; i < verticesList.Length; i++)
        {
            verticesList[i].Normalize(i);
        }
        Profiler.EndSample();
        return verticesList;

    }

} 

