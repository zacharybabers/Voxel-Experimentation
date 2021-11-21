using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
   
    [SerializeField] private Biome biome;
   
    
    
    
    public int[,,] GenerateChunkAtlas(Vector3 chunkCoord)
    {
        return biome.GenerateChunkAtlas(chunkCoord);
    }
}

