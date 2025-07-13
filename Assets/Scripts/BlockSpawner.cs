using System.Collections.Generic;
using UnityEngine;

public class BlockSpawner : MonoBehaviour
{
    [Header("Block Settings")]
    public GameObject blockPrefab;

    [Header("Spawn Points (order matters)")]
    public List<Transform> spawnPoints; // Each should have BlockSpawnPoint.cs attached

    [Header("Tracking")]
    public List<Block> activeBlocks = new List<Block>();

    public void SpawnBlocks()
    {
        ClearExistingBlocks();

        for (int i = 0; i < spawnPoints.Count; i++)
        {
            Transform spawn = spawnPoints[i];
            BlockSpawnPoint spawnInfo = spawn.GetComponent<BlockSpawnPoint>();

            if (spawnInfo == null)
            {
                Debug.LogWarning($"Spawn point {spawn.name} is missing BlockSpawnPoint component!");
                continue;
            }

            GameObject blockObj = Instantiate(blockPrefab, spawn.position, Quaternion.identity);

            Block blockScript = blockObj.GetComponent<Block>();
            blockScript.SetColor(spawnInfo.blockColor);

            activeBlocks.Add(blockScript);
        }
    }

    public void ClearExistingBlocks()
    {
        foreach (var block in activeBlocks)
        {
            if (block != null) Destroy(block.gameObject);
        }
        activeBlocks.Clear();
    }
}
