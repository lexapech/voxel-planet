using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    private MeshFilter meshFilter; 
    private MeshCollider meshCollider;
    private GridData data;
    private World world;
    private Vector3Int position;
    private float _sizeMultiplier;
    public bool Valid;

    // Start is called before the first frame update
    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        Valid = false;
        //generateMesh();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Initialize(World world,Vector3Int position,GridData data,float sizeMultiplier = 2f)
    {
        _sizeMultiplier = sizeMultiplier;
        this.world = world;
        this.position = position;
        this.data = data;
    }

    public void generateMesh()
    {       
        if(!meshFilter) meshFilter = GetComponent<MeshFilter>();
        if (!meshCollider) meshCollider = GetComponent<MeshCollider>();
        var activeEdges = findEdges();
        var vertices = findActiveCells(activeEdges);
        Mesh mesh = createMeshFlat(activeEdges, vertices);
        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;
        Valid = true;
    }

    
    //TODO: getVAlue+IsActive

    private float getEdgeValue(Edge edge)
    {
        Vector3Int endPosition = edge.startPosition + Utility.EdgeDirToVector(edge.direction);
        var startChunkData = data;
        var endChunkData = data;
        var startPosition = edge.startPosition;
        var vertex1 = 0;
        var vertex2 = 0;
        if (startPosition.IsComponentWiseGreaterOrEqual(data.Size))
        {
            var startChunk = getNextChunk(startPosition);
            if (startChunk)
            {
                startChunkData = startChunk.data;
                startPosition = Utility.VectorMod(startPosition, data.Size);
                vertex1 = startChunkData.volumes.Get3D(startPosition);
            }
        }
        else
        {
            vertex1 = startChunkData.volumes.Get3D(startPosition);
        }
        if (endPosition.IsComponentWiseGreaterOrEqual(data.Size))
        {
            var endChunk = getNextChunk(endPosition);
            if (endChunk)
            {              
                endPosition = Utility.VectorMod(endPosition, data.Size);
                endChunkData = endChunk.data;
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
        Vector3Int chunkOffset = endPosition/data.Size;
        MeshGenerator nextChunk;
        world.chunks.TryGetValue(position + chunkOffset, out nextChunk);
        return nextChunk;
    }


    private bool isEdgeActive(Edge edge)
    {   
        Vector3Int endPosition = edge.startPosition + Utility.EdgeDirToVector(edge.direction);
        var startChunkData = data;
        var endChunkData = data;
        var startPosition = edge.startPosition;
        if (startPosition.IsComponentWiseGreaterOrEqual(data.Size))
        {
            var startChunk = getNextChunk(startPosition);
            if (startChunk)
            {
                startChunkData = startChunk.data;
                startPosition = Utility.VectorMod(startPosition, data.Size);
            }
            else
            {
                return false;
            }
        }
        if (endPosition.IsComponentWiseGreaterOrEqual(data.Size))
        {
            var endChunk = getNextChunk(endPosition);
            if (endChunk)
            {

                endPosition = Utility.VectorMod(endPosition, data.Size);
                endChunkData = endChunk.data;
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
        var cells = data.Size+2;
        var directionsArray = (Edge.Directions[])Enum.GetValues(typeof(Edge.Directions));
        for (int z = 0; z < cells; z++)
        {
            for (int y = 0; y < cells; y++)
            {
                for (int x = 0; x < cells; x++)
                {                   
                    Edge edge;
                    for (int i=0;i< directionsArray.Length;i++)
                    {                      
                        edge = new Edge(new Vector3Int(x, y, z), directionsArray[i]);
                        if (isEdgeActive(edge))
                        {
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
            if (edgeEnd.IsComponentWiseGreaterOrEqual(data.Size + 1)) continue;
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
            if (edgeEnd.IsComponentWiseGreaterOrEqual(data.Size + 1)) continue;
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
            verticesList.AddRange(Enumerable.Select(edge.vertices, x => (x.position-Vector3.one*data.Size/2) * _sizeMultiplier));
        }  
        mesh.vertices = verticesList.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();
        //mesh.normals = normals.ToArray();
        return mesh;
    }
    private Vertex[] findActiveCells(List<Edge> activeEdges)
    {
        int gridSize = data.Size+1;
        var vertices = new Dictionary<Vector3Int, Vertex>();
        for (int i = 0; i < activeEdges.Count; i++)
        {
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
                        edge.vertices.Add(vertex);
                    }
                }
            }

        }
        var verticesList = vertices.Values.ToArray();
        for (int i = 0; i < verticesList.Length; i++)
        {
            verticesList[i].Normalize(i);
        }
        return verticesList;

    }

} 

