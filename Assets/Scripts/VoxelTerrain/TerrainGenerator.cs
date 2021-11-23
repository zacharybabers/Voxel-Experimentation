using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
   
    [SerializeField] private Biome biome;
    [SerializeField] private int numTextures;
    private Dictionary<Biome, List<UVSet>> uvLookup;

    private void Awake()
    {
        if (numTextures % 2 != 0)
        {
            Debug.Log("Uneven amount of textures!");  //give a warning if there is an uneven amount of textures (uv coords will be repeating and therefore less accurate)
        }
        //for each biome in biome list, generate a uv lookup table
    }

    public int[,,] GenerateChunkAtlas(Vector3 chunkCoord)
    {
        return biome.GenerateChunkAtlas(chunkCoord);
    }
}

public struct UVSet
{
    public Vector2 topUVs;
    public Vector2 sideUVs;
    public Vector2 bottomUvs;
}

