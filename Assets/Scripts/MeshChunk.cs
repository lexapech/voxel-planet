using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class MeshChunk : MonoBehaviour
{
    private MeshFilter _meshFilter;
    private MeshCollider _meshCollider;
    private float _size;
    private MeshGenerator _meshGenerator;
    private IMeshDataProvider _meshDataProvider;

    public void Init(Transform parent, Vector3 position, float size, IMeshDataProvider meshDataProvider)
    {
        transform.parent = parent;
        transform.localPosition = position;
        _size = size;
        _meshDataProvider = meshDataProvider;
        _meshFilter = GetComponent<MeshFilter>();
        _meshCollider = GetComponent<MeshCollider>();
        _meshGenerator = new MeshGenerator(position,size,_meshDataProvider);
        gameObject.SetActive(false);
    }
    public void GenerateMesh()
    {
        var timer = System.Diagnostics.Stopwatch.StartNew();
        Profiler.BeginSample("generate mesh");
        var mesh = _meshGenerator.generateMesh();
        Profiler.EndSample();
        timer.Stop();
        Debug.Log("Chunk generation finished in " + timer.ElapsedMilliseconds + " ms");
        _meshFilter.sharedMesh = mesh;
        _meshCollider.sharedMesh = mesh;
        gameObject.SetActive(true);
    }
}
