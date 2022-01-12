using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class TerrainChunk : MonoBehaviour
{
    public ChunkData chunkData;
    public Vector3 chunkCoord;
    public MeshFilter meshFilter;

    private void Awake()
    {
        meshFilter = gameObject.GetComponent<MeshFilter>();
    }
    
}



public class ChunkData
{
    public Vector3 chunkCoord;

    public bool isEmpty;
    
    private Mesh chunkMesh;
    
    public int[,,] voxelAtlas;

    public TerrainChunk terrainChunk;

    public ChunkData(Vector3 chunkCoord)
    {
        this.chunkCoord = chunkCoord;
        GetVoxelAtlas();
        isEmpty = IsEmpty();
    }

    public void BuildMesh()
    {
        if (!isEmpty)
        {
            var top = new Vector3(chunkCoord.x, chunkCoord.y, chunkCoord.z+1);
            var bottom = new Vector3(chunkCoord.x, chunkCoord.y, chunkCoord.z-1);
            var left = new Vector3(chunkCoord.x-1, chunkCoord.y, chunkCoord.z);
            var right = new Vector3(chunkCoord.x+1, chunkCoord.y, chunkCoord.z);
            var forward = new Vector3(chunkCoord.x, chunkCoord.y+1, chunkCoord.z);
            var backward = new Vector3(chunkCoord.x, chunkCoord.y-1, chunkCoord.z);

            ref var topChunk = ref WorldInfo.loadedChunkDictionary[top].voxelAtlas;
            ref var bottomChunk = ref WorldInfo.loadedChunkDictionary[bottom].voxelAtlas;
            ref var leftChunk = ref WorldInfo.loadedChunkDictionary[left].voxelAtlas;
            ref var rightChunk = ref WorldInfo.loadedChunkDictionary[right].voxelAtlas;
            ref var forwardChunk = ref WorldInfo.loadedChunkDictionary[forward].voxelAtlas;
            ref var backChunk = ref WorldInfo.loadedChunkDictionary[backward].voxelAtlas;
            
            chunkMesh = WorldInfo.meshBuilder.Build(this, ref topChunk, ref bottomChunk, ref leftChunk, ref rightChunk, ref forwardChunk, ref backChunk);
        }
        UpdatePositionAndMesh();
    }

    private void GetVoxelAtlas()
    {
        voxelAtlas = WorldInfo.terrainGenerator.GenerateChunkAtlas(chunkCoord);
    }

    private void UpdatePositionAndMesh()
    {
        if (terrainChunk == null)
        {
            return;
        }
        terrainChunk.transform.position = new Vector3(chunkCoord.x * WorldInfo.chunkSize, chunkCoord.z * WorldInfo.chunkSize, chunkCoord.y * WorldInfo.chunkSize);
        terrainChunk.meshFilter.mesh = chunkMesh;
        terrainChunk.chunkCoord = chunkCoord;
        terrainChunk.gameObject.name = "chunk (" + chunkCoord.x + ", " + chunkCoord.y + ", " + chunkCoord.z + ")";
    }

    public void AssignTerrainChunk(TerrainChunk terrainChunk)
    {
        terrainChunk.gameObject.SetActive(true);
        this.terrainChunk = terrainChunk;
        terrainChunk.chunkData = this;
        UpdatePositionAndMesh();
    }

    public void UnloadTerrainChunk()
    {
        if (terrainChunk != null)
        {
            this.terrainChunk.gameObject.SetActive(false);
            this.terrainChunk = null;
        }
    }

    public bool HasMesh()
    {
        return this.chunkMesh != null;
    }

    private bool IsEmpty()
    {
        
        for (int i = 0; i < WorldInfo.chunkSize; i++)
        {
            for (int j = 0; j < WorldInfo.chunkSize; j++)
            {
                for (int k = 0; k < WorldInfo.chunkSize; k++)
                {
                    if (voxelAtlas[i, j, k] != 0)
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }
    
    
}