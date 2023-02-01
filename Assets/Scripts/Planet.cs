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
    LODOctree LODOctree;
    int counter = 0;
    void Awake()
    {
        LODOctree = GetComponent<LODOctree>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        LODOctree.SetLODCenter(Player.transform.position);
        Gizmos.color = Color.yellow;
        counter = 0;
        LODDFS(LODOctree.Root);
        Debug.Log(counter);

    }

    private void LODDFS(LODNode node)
    {
       
        if (node.isLeaf)
        {
            counter++;
            Gizmos.DrawWireCube(node.Position, Vector3.one * LODOctree.GetNodeSize(node));
            return;
        }
        for (int i = 0; i < 8; i++)
        {         
            LODDFS(node.Children[i]);      
        }
           
    }

}
