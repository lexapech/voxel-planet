using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;

public class World : MonoBehaviour
{
    [SerializeField]
    private GameObject chunkPrototype;
    [SerializeField]
    private GameObject playerPrototype;
    [SerializeField]
    private int chunkSize = 32;
    private GridData[,,] worldData;
    private List<Player> playerList;
    private int chunksCount = 2;
    public Vector3 gravityCenter { private set; get; }
    public Dictionary<Vector3Int, MeshGenerator> chunks { private set; get; }
    // Start is called before the first frame update
    void Start()
    {
        playerList = new List<Player>();
        gravityCenter = Vector3.one * chunksCount * chunkSize / 2;
        generateWorld();
        createPlayer(new Vector3(chunksCount * chunkSize / 2, chunksCount * chunkSize + 5, chunksCount * chunkSize / 2));
    }

    private void createPlayer(Vector3 position)
    {
        var player = Instantiate(playerPrototype,transform);
        player.name = "Player";
        player.transform.position = position;
        var gravityDown = (gravityCenter - player.transform.position).normalized;
        player.transform.rotation.SetFromToRotation(-player.transform.up, gravityDown);
        player.SetActive(true);
        var playerComponent = player.GetComponent<Player>();
        playerComponent.world = this;
        playerList.Add(playerComponent);
    }

    private void generateWorld()
    {
        worldData = new GridData[chunksCount, chunksCount, chunksCount];
        chunks = new Dictionary<Vector3Int, MeshGenerator>();
        for (int z = 0; z < chunksCount; z++)
        {
            for (int y = 0; y < chunksCount; y++)
            {
                for (int x = 0; x < chunksCount; x++)
                {
                    worldData[x, y, z] = initializeGrid(new Vector3Int(x, y, z));
                }
            }
        }
        for (int z = 0; z < chunksCount; z++)
        {
            for (int y = 0; y < chunksCount; y++)
            {
                for (int x = 0; x < chunksCount; x++)
                {
                    var position = new Vector3Int(x, y, z);
                    var chunk = Instantiate(chunkPrototype, transform);
                    chunk.name = string.Format("Chunk_{0}_{1}_{2}", x, y, z);
                    chunk.transform.position = position * chunkSize;
                    chunk.SetActive(true);
                    var chunkMeshGenerator = chunk.GetComponent<MeshGenerator>();
                    chunkMeshGenerator.Initialize(this, position, worldData[x, y, z]);
                    chunks.Add(position, chunkMeshGenerator);
                }
            }
        }
        foreach (var chunk in chunks)
        {
            chunk.Value.generateMesh();
        }
    }

    private GridData initializeGrid(Vector3Int pos)
    {
        Vector3 center = Vector3.one * chunksCount * chunkSize/2;
        float radius = (chunksCount * chunkSize / 2) *0.9f;
        var grid = new GridData(chunkSize);
        for (int z = 0; z < chunkSize; z++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                for (int x = 0; x < chunkSize; x++)
                {
                    Vector3 currentPos=(pos * chunkSize + new Vector3(x, y, z));
                    var temp = (currentPos - center).normalized;
                    var a1 = Mathf.Atan2(temp.z, temp.x);
                    var a2 = Mathf.Atan2(temp.y, temp.x);
                    float sphereDist = radius -(currentPos - center).magnitude - Mathf.PerlinNoise(a1*1f, a2 * 1f) * 10 - Mathf.PerlinNoise(a1 * 3f, a2 * 3f) * 5;
                    grid.volumes[x, y, z] = (byte)(Mathf.Clamp01(sphereDist) * 255);
                    //grid.volumes[x, y, z] = (byte)(Mathf.Clamp01(Mathf.PerlinNoise(currentPos.x, currentPos.z) * 25 + (Mathf.PerlinNoise(currentPos.x*3, currentPos.z*3)*10-5) + 5 - y) * 255);

                }
            }
        }
        return grid;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
