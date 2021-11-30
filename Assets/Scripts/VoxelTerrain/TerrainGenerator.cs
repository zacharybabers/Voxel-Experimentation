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
    public Dictionary<int, UVSet> uvLookup;

    public void InitializeLookupTable()
    {
        if (numTextures % 2 != 0 || numTextures == 0)
        {
            Debug.Log("Uneven amount of textures!");  //give a warning if there is an uneven amount of textures (uv coords will be repeating and therefore less accurate)
        }
        
        uvLookup = new Dictionary<int, UVSet>();
        var initList = GenerateLookupTable();
        for (int i = 0; i < initList.Count; i++)
        {
            uvLookup.Add(i + 1, initList[i]);
        }
    }

    public List<UVSet> GenerateLookupTable()
    {
        List<UVSet> initList = new List<UVSet>();
        for (int i = 0; i < numTextures; i++)
        {
            UVSet uvSet = new UVSet();

            int adjustedIndex = i * 3;
            int totalTextures = numTextures * 3;
            //topUVs
            var topUVs = new QuadUVs();
            topUVs.topLeft = new Vector2((float) adjustedIndex / totalTextures, 1f);
            topUVs.bottomRight = new Vector2( adjustedIndex + 1f/ totalTextures, 0f);
            topUVs.topRight = new Vector2( adjustedIndex + 1f/ totalTextures, 1f);
            topUVs.bottomLeft = new Vector2((float) adjustedIndex / totalTextures, 0f);
            uvSet.topUVs = topUVs;
            //sideUVs
            var sideUVs = new QuadUVs();
            sideUVs.topLeft = new Vector2( adjustedIndex + 1f/ totalTextures, 1f);
            sideUVs.bottomRight = new Vector2( adjustedIndex + 2f/ totalTextures, 0f);
            sideUVs.topRight = new Vector2( adjustedIndex + 2f/ totalTextures, 1f);
            sideUVs.bottomLeft = new Vector2( adjustedIndex + 1f/ totalTextures, 0f);
            uvSet.sideUVs = sideUVs;
            //bottomUVs
            var bottomUVs = new QuadUVs();
            bottomUVs.topLeft = new Vector2(adjustedIndex + 2f / totalTextures, 1f);
            bottomUVs.bottomRight = new Vector2(adjustedIndex + 3f / totalTextures, 0f);
            bottomUVs.bottomLeft = new Vector2(adjustedIndex + 2f / totalTextures, 0f);
            bottomUVs.topRight = new Vector2(adjustedIndex + 3f / totalTextures, 1f);
            uvSet.bottomUVs = bottomUVs;
            
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
    public QuadUVs topUVs;
    public QuadUVs sideUVs;
    public QuadUVs bottomUVs;

    public UVSet(QuadUVs topUVs, QuadUVs sideUVs, QuadUVs bottomUVs)
    {
        this.bottomUVs = bottomUVs;
        this.topUVs = topUVs;
        this.sideUVs = sideUVs;
    }
}

public struct QuadUVs
{
    public Vector2 topLeft;
    public Vector2 bottomRight;
    public Vector2 bottomLeft;
    public Vector2 topRight;

    public QuadUVs(Vector2 tL, Vector2 bR, Vector2 bL, Vector2 tR)
    {
        this.topLeft = tL;
        this.bottomRight = bR;
        this.bottomLeft = bL;
        this.topRight = tR;
    }
}

