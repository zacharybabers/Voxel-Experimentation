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
    public MeshCollider meshCollider;

    private void Awake()
    {
        meshFilter = gameObject.GetComponent<MeshFilter>();
        meshCollider = gameObject.GetComponent<MeshCollider>();
    }
    
}



public class ChunkData
{
    public Vector3 chunkCoord;

    public bool isEmpty;
    
    public int currentLOD;
    
    private Mesh[] chunkMeshes;
    
    public int[,,] voxelAtlas;

    public TerrainChunk terrainChunk;

    public ChunkData(Vector3 chunkCoord)
    {
        this.chunkCoord = chunkCoord;
        GetVoxelAtlas();
        isEmpty = IsEmpty();
        chunkMeshes = new Mesh[5];
        currentLOD = -1;
    }

    public void Refresh(Vector3 chunkCoord)
    {
        this.chunkCoord = chunkCoord;
        GetVoxelAtlas();
        isEmpty = IsEmpty();
        ClearChunkMeshes();
        currentLOD = -1;
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
            /*
            if (WorldInfo.loadedChunkDictionary[top].currentLOD != this.currentLOD)
            {
                topChunk = ref WorldInfo.emptyAtlas;
            }
            */
            
            ref var bottomChunk = ref WorldInfo.loadedChunkDictionary[bottom].voxelAtlas;
            /*
            if (WorldInfo.loadedChunkDictionary[bottom].currentLOD != this.currentLOD)
            {
                bottomChunk = ref WorldInfo.emptyAtlas;
            }
            */
            
            ref var leftChunk = ref WorldInfo.loadedChunkDictionary[left].voxelAtlas;
            if (WorldInfo.loadedChunkDictionary[left].currentLOD != this.currentLOD)
            {
                leftChunk = ref WorldInfo.emptyAtlas;
            }
            
            ref var rightChunk = ref WorldInfo.loadedChunkDictionary[right].voxelAtlas;
            if (WorldInfo.loadedChunkDictionary[right].currentLOD != this.currentLOD)
            {
                rightChunk = ref WorldInfo.emptyAtlas;
            }
            
            ref var forwardChunk = ref WorldInfo.loadedChunkDictionary[forward].voxelAtlas;
            if (WorldInfo.loadedChunkDictionary[forward].currentLOD != this.currentLOD)
            {
                forwardChunk = ref WorldInfo.emptyAtlas;
            }
            
            ref var backChunk = ref WorldInfo.loadedChunkDictionary[backward].voxelAtlas;
            if (WorldInfo.loadedChunkDictionary[backward].currentLOD != this.currentLOD)
            {
                backChunk = ref WorldInfo.emptyAtlas;
            }
            
            chunkMeshes[currentLOD] = WorldInfo.meshBuilder.BuildLOD(currentLOD, this, ref topChunk, ref bottomChunk, ref leftChunk, ref rightChunk, ref forwardChunk, ref backChunk);
        }
        UpdatePositionAndMesh();
    }
    
    private void GetVoxelAtlas()
    {
        voxelAtlas = WorldInfo.terrainGenerator.GenerateChunkAtlas(chunkCoord);
    }

    public void UpdatePositionAndMesh()
    {
        if (terrainChunk == null)
        {
            return;
        }
        terrainChunk.transform.position = new Vector3(chunkCoord.x * WorldInfo.chunkSize, chunkCoord.z * WorldInfo.chunkSize, chunkCoord.y * WorldInfo.chunkSize);
        if (currentLOD != -1)
        {
            terrainChunk.meshFilter.mesh = chunkMeshes[currentLOD];
            terrainChunk.meshCollider.sharedMesh = chunkMeshes[currentLOD];
        }
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
        return this.chunkMeshes[currentLOD] != null;
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

    private void ClearChunkMeshes()
    {
        for (int i = 0; i < chunkMeshes.Length; i++)
        {
            chunkMeshes[i] = null;
        }
    }
    
    
}