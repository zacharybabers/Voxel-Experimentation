using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [SerializeField] private List<Biome> biomes;
    [SerializeField] private int textureCols;
    [SerializeField] private int textureRows;
    [SerializeField] private float xTexInset = .01f;
    [SerializeField] private float yTexInset = .01f;
    public Dictionary<int, UVSet> uvLookup;

    public void InitializeLookupTable()
    {
        uvLookup = new Dictionary<int, UVSet>();
        Dictionary<int, CubeType> cubeTypes = new Dictionary<int, CubeType>();
        var cubes = FindObjectsOfType<CubeType>();
        foreach (var cubeType in cubes)
        {
            cubeTypes.Add(cubeType.GetBlockID(), cubeType);
        }

        foreach (var pair in cubeTypes)
        {
            QuadUVs topUVs = GetIntCoordUVs(pair.Value.GetTopTextureCoord());
            QuadUVs sideUVs = GetIntCoordUVs(pair.Value.GetSideTextureCoord());
            QuadUVs bottomUVs = GetIntCoordUVs(pair.Value.GetBottomTextureCoord());
            
            UVSet cubeUVs = new UVSet(topUVs, sideUVs, bottomUVs);
            uvLookup.Add(pair.Key, cubeUVs);
        }
        
        
    }

    private QuadUVs GetIntCoordUVs(Vector2Int coord)
    {
        Vector2 coordf = new Vector2((float) coord.x, (float) coord.y);
        float rowsf = textureRows;
        float colsf = textureCols;
        
        var top =  (coordf.y + 1f) / rowsf; 
        var bottom = coordf.y / rowsf;
        var left =  coordf.x  / colsf;
        var right = (coordf.x + 1f) / colsf;

        right -= xTexInset;
        left += xTexInset;

        top -= yTexInset;
        bottom += yTexInset;
        
        return new QuadUVs(new Vector2(left, top), new Vector2(right, bottom), new Vector2(left, bottom), new Vector2(right, top));
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

