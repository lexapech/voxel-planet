using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkPool : MonoBehaviour, IChunkProvider
{
    [SerializeField]
    public GameObject ChunkPrefab;
    public void Destroy(MeshChunk chunk)
    {
        Destroy(chunk.gameObject);
    }

    public MeshChunk Instantiate()
    {
        var chunkGameObject = Instantiate(ChunkPrefab);
        var meshChunk = chunkGameObject.GetComponent<MeshChunk>();
        return meshChunk;
    }
}
