using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static LODOctree;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class Planet : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    GameObject Player;
    [SerializeField, Range(16, 128)]
    public int ChunkSize = 32;
    LODOctree LODOctree;
    void Start()
    {
        var chunkPool = GetComponent<ChunkPool>();
        LODOctree = GetComponent<LODOctree>();
        LODOctree.Init(chunkPool, new PlainsMeshDataProvider(ChunkSize), ChunkSize); ;
        LODOctree.SetLODCenter(Player.transform.position);
        LODOctree.ApplyMeshTransition();
    }

    // Update is called once per frame
    void Update()
    {
        LODOctree.SetLODCenter(Player.transform.position);
        LODOctree.ApplyMeshTransition();
    }
}
