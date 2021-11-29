using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [SerializeField] private List<Biome> biomes;
    [SerializeField] private int numTextures;
    private Dictionary<Biome, List<UVSet>> uvLookup;

    private void Awake()
    {
        if (numTextures % 2 != 0)
        {
            Debug.Log("Uneven amount of textures!");  //give a warning if there is an uneven amount of textures (uv coords will be repeating and therefore less accurate)
        }
        
        uvLookup = new Dictionary<Biome, List<UVSet>>();
        foreach (var biome in biomes)
        {
            uvLookup.Add(biome, GenerateLookupTable(biome));
        }
    }

    public List<UVSet> GenerateLookupTable(Biome biome)
    {
        List<UVSet> initList = new List<UVSet>();
        for (int i = 0; i < biome.blockTextureList.Count; i++)
        {
            UVSet uvSet = new UVSet();

            int adjustedIndex = i * 3;
            //topUVs
            var topUVs = new QuadUVS();
            topUVs.topLeft = new Vector2((float) adjustedIndex / numTextures, 1f);
            topUVs.bottomRight = new Vector2( adjustedIndex + 1f/ numTextures, 0f);
            uvSet.topUVs = topUVs;
            //sideUVs
            var sideUVs = new QuadUVS();
            sideUVs.topLeft = new Vector2( adjustedIndex + 1f/ numTextures, 1f);
            sideUVs.bottomRight = new Vector2( adjustedIndex + 2f/ numTextures, 0f);
            uvSet.sideUVs = sideUVs;
            //bottomUVs
            var bottomUVs = new QuadUVS();
            bottomUVs.topLeft = new Vector2(adjustedIndex + 2f / numTextures, 1f);
            bottomUVs.bottomRight = new Vector2(adjustedIndex + 3f / numTextures, 0f);
            uvSet.bottomUvs = bottomUVs;
            
            initList.Add(uvSet);
        }

        return initList;
    }

    public int[,,] GenerateChunkAtlas(Vector3 chunkCoord)
    {
        return biomes[0].GenerateChunkAtlas(chunkCoord);
    }
}

public struct UVSet
{
    public QuadUVS topUVs;
    public QuadUVS sideUVs;
    public QuadUVS bottomUvs;
}

public struct QuadUVS
{
    public Vector2 topLeft;
    public Vector2 bottomRight;
}

