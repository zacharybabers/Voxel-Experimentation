using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;

public class WorldInfo : MonoBehaviour
{
    public static Dictionary<Vector3, ChunkData> loadedChunkDictionary;
    public Queue<Vector3> chunksToLoad;
    public Queue<Vector3> meshQueue;
    public Stack<ChunkData> chunkPool;
    public Stack<TerrainChunk> terrainPool;
    
    [SerializeField] private int nonLoadedChunkSize = 9;
    [SerializeField] private int chunksPerFrame = 5;
    [SerializeField] private int worldLOD = 0;

    private static bool viewerNewChunkThisFrame;

    public const int chunkSize = 32;

    public static CulledMeshBuilder meshBuilder;
    public static TerrainGenerator terrainGenerator;
    public static byte[,,] emptyAtlas;

    [SerializeField] private Transform chunkParent;
    [SerializeField] private GameObject chunkPrefab;
  

    [SerializeField] private Transform targetTransform;
    private Vector3Int transformChunk;
    private Vector3Int lastTransformChunk;
    
    private float drawDistance;
    [SerializeField] private float[] lodDistances;

    private void Awake()
    {
        emptyAtlas = new byte[chunkSize,chunkSize,chunkSize];
        drawDistance = lodDistances[4];
        for(int i = 0; i < lodDistances.Length; i++)
        {
            lodDistances[i] = Mathf.Pow(lodDistances[i], 2f);
        }
        InitCubeTypes();
        meshBuilder = gameObject.AddComponent<CulledMeshBuilder>();
        meshBuilder.InitMeshBuilder();
        meshBuilder.mainLOD = worldLOD;
        terrainGenerator = gameObject.GetComponent<TerrainGenerator>();
        loadedChunkDictionary = new Dictionary<Vector3, ChunkData>();
        chunksToLoad = new Queue<Vector3>();
        meshQueue = new Queue<Vector3>();
        chunkPool = new Stack<ChunkData>();
        terrainPool = new Stack<TerrainChunk>();
        terrainGenerator.InitializeLookupTable();
        meshBuilder.uvLookup = terrainGenerator.uvLookup;
        //fill unused terrain chunks
        for (int i = 0; i < nonLoadedChunkSize; i++)
        {
            GameObject chunkObject = Instantiate(chunkPrefab, chunkParent);
            var terrainChunk = chunkObject.AddComponent<TerrainChunk>();
            terrainPool.Push(terrainChunk);
        }
        var position = targetTransform.position;
        transformChunk = new Vector3Int((int) position.x / chunkSize, (int) position.z / chunkSize, (int) position.y / chunkSize);
        lastTransformChunk = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);

        InitializeWorld();
    }

    private void Update()
    {
        CheckSameChunk();
        UpdateChunks();
    }

    private void InitCubeTypes()
    {
        var ctd = gameObject.AddComponent<CubeTypeDistributor>();
        ctd.UpdateIDs();

        var biomes = FindObjectsOfType<Biome>();
        foreach (var biome in biomes)
        {
            biome.UpdateCubeTypes();
        }
    }

    private void CheckSameChunk()
    {
        var position = targetTransform.position;
        transformChunk = new Vector3Int((int) position.x / chunkSize, (int) position.z / chunkSize, (int) position.y / chunkSize);
        if (transformChunk.Equals(lastTransformChunk))
        {
            viewerNewChunkThisFrame = false;
        }
        else
        {
            viewerNewChunkThisFrame = true;
        }

        lastTransformChunk = transformChunk;

    }

    private void FindInitialChunksToLoad()
    {
        
        chunksToLoad.Clear();
        
        var viewDistSquared = drawDistance * drawDistance;
        
        var currentChunkX = transformChunk.x;
        var currentChunkY = transformChunk.y;
        var currentChunkZ = transformChunk.z;

        var chunksInLinearDist = (int) drawDistance / chunkSize;

        //loop through x,y,z... if dist(player,(x,y,z)) < radius, do our check for coords being in dictionary, if not, add to chunksToLoad... this method would allow implementation of steps as well
        for (int i = currentChunkX - chunksInLinearDist; i <= currentChunkX + chunksInLinearDist; i++) //edit this loop in order to create steps, for example, per frame we only go through one x and repeat every set number of frames ( this would probably be done for y generally but no real difference )
        {
            for (int j = currentChunkY - chunksInLinearDist; j <= currentChunkY + chunksInLinearDist; j++)
            {
                for (int k = currentChunkZ - chunksInLinearDist; k <= currentChunkZ + chunksInLinearDist; k++)
                {
                    if (!loadedChunkDictionary.ContainsKey(new Vector3(i, j, k)) && !chunksToLoad.Contains(new Vector3(i, j, k)))
                    {
                        var distSquared = Mathf.Pow((float) (i - currentChunkX) * chunkSize, 2) + Mathf.Pow((float) (j - currentChunkY) * chunkSize, 2) + Mathf.Pow((float) (k - currentChunkZ) * chunkSize, 2);
                    
                        if (distSquared < viewDistSquared && k > -4)
                        {
                            chunksToLoad.Enqueue(new Vector3(i, j, k));
                        }
                    }
                }
                
            }
        }
        
        var myChunk = new Vector3(currentChunkX, currentChunkY, 0);

        if (chunksToLoad.Contains(myChunk) && chunksToLoad.Peek() != myChunk)
        {
            List<Vector3> tempList = new List<Vector3>();
            tempList.Add(myChunk);
            tempList.AddRange(chunksToLoad);
            var index = 0;
            for (var i = 1; i < tempList.Count; i++)
            {
                if (tempList[i].Equals(myChunk))
                {
                    index = i;
                }
            }

            tempList.RemoveAt(index);
            chunksToLoad = new Queue<Vector3>(tempList);
        }
        
        
    }

    private void InitializeWorld()
    {
        FindInitialChunksToLoad();


        //initialize chunk objects
        int initialNumChunks = chunksToLoad.Count;

        for (int i = 0; i < initialNumChunks; i++)
        {
            var chunkLoading = chunksToLoad.Dequeue(); 
            LoadChunk(chunkLoading);
        }

        var currentChunkX = transformChunk.x;
        var currentChunkY = transformChunk.y;
        var currentChunkZ = transformChunk.z;
        
        foreach (var coordinate in loadedChunkDictionary.Keys)
        {
            for (int i = 0; i < 5; i++)
            {
                var viewDistSquared = lodDistances[i];
                var distSquared = Mathf.Pow((float) (coordinate.x - currentChunkX) * chunkSize, 2) +
                                  Mathf.Pow((float) (coordinate.y - currentChunkY) * chunkSize, 2) +
                                  Mathf.Pow((float) (coordinate.z - currentChunkZ) * chunkSize, 2);
                if (distSquared < viewDistSquared)
                {
                    loadedChunkDictionary[coordinate].currentLOD = i;
                    break;
                }
                
            }
            
            MeshChunk(coordinate);
            CreateTerrainChunk(loadedChunkDictionary[coordinate]);
        }
    }

    private void UpdateChunks()
    {
        if (viewerNewChunkThisFrame)
        {
            // UpdateLODNeeded();
            // FindInitialChunksToLoad();

            var timer = Time.realtimeSinceStartup;
            
            chunksToLoad.Clear();

            var chunksInLinearDist = (int) drawDistance / chunkSize;
            var arrayLength = (int) Mathf.Pow(2 * chunksInLinearDist, 3);

            var distArray = new NativeArray<sbyte>(arrayLength, Allocator.TempJob);
            var coordsArray = new NativeArray<float3>(arrayLength, Allocator.TempJob);
            var viewDistArray = new NativeArray<float>(lodDistances.Length, Allocator.TempJob);
            for (int i = 0; i < lodDistances.Length; i++)
            {
                viewDistArray[i] = lodDistances[i];
            }

            DistanceChecker distanceChecker = new DistanceChecker();
            distanceChecker.dists = distArray;
            distanceChecker.coords = coordsArray;
            distanceChecker.viewDistsSquared = viewDistArray;
            distanceChecker.chunksInLinearDist = chunksInLinearDist;
            distanceChecker.playerChunk = (Vector3) transformChunk;

            JobHandle distanceCheckerHandle = distanceChecker.Schedule(arrayLength, 20);
            
            distanceCheckerHandle.Complete();

            for (int i = 0; i < arrayLength; i++)
            {
                var lodLevel = distArray[i];
                
                if (lodLevel != -1)
                {
                    Vector3 coordinate = coordsArray[i];
                    if (loadedChunkDictionary.ContainsKey(coordinate))
                    {
                        UpdateMeshQueue(coordinate, lodLevel);
                    }
                    else if(coordinate.z > -2 && !chunksToLoad.Contains(coordinate))
                    {
                        chunksToLoad.Enqueue(coordinate);
                    }
                }
            }
            
            distanceChecker.dists.Dispose();
            distanceChecker.coords.Dispose();
            viewDistArray.Dispose();
            
            UnloadChunks();
            
            //Debug.Log("time taken: " + (Time.realtimeSinceStartup - timer) + ". chunkstoload count: " + chunksToLoad.Count);

        }
        else
        {
            for (int i = 0; i < chunksPerFrame; i++)
            {
                if (chunksToLoad.Count > 0)
                {
                    RefreshOneChunk();
                    MeshOneChunk();
                }

                
            }
        }
    }

    private void UpdateLODNeeded()
    {

        var currentChunkX = transformChunk.x;
        var currentChunkY = transformChunk.y;
        var currentChunkZ = transformChunk.z;
        
        foreach (var coordinate in loadedChunkDictionary.Keys)
        {
            for (int i = 0; i < 5; i++)
            {
                var viewDistSquared = lodDistances[i];
                var distSquared = Mathf.Pow((float) (coordinate.x - currentChunkX) * chunkSize, 2) +
                                  Mathf.Pow((float) (coordinate.y - currentChunkY) * chunkSize, 2) +
                                  Mathf.Pow((float) (coordinate.z - currentChunkZ) * chunkSize, 2);
                if (distSquared < viewDistSquared)
                {
                    UpdateMeshQueue(coordinate, i);
                    break;
                }
                
            }
        }
        
    }

    private void UpdateMeshQueue(Vector3 coordinate, int lod)
    {
        //if they don't have a mesh, they're not empty, and they're meshable, add to mesh queue
        var data = loadedChunkDictionary[coordinate];

        if (data.currentLOD == lod)
        {
            return;
        }
        data.currentLOD = lod;

        if (data.HasMesh())
        {
            data.UpdatePositionAndMesh();
            return;
        }
            
        if (!data.HasMesh() && !data.isEmpty && IsMeshable(coordinate))
        { 
            meshQueue.Enqueue(coordinate);
        }
        
    }
    

    private void UnloadChunks()
    {

        var viewDistSquared = drawDistance * drawDistance;

        
        var currentChunkX = transformChunk.x;
        var currentChunkY = transformChunk.y;
        var currentChunkZ = transformChunk.z;

        var chunkDatas = new ChunkData[loadedChunkDictionary.Count];
        loadedChunkDictionary.Values.CopyTo(chunkDatas, 0);

        for (int chunkNum = 0; chunkNum < loadedChunkDictionary.Count; chunkNum++)
        {
            //calculate view distance, check which chunks are outside of it, then add them to pool and unload them
            var distSquared = Mathf.Pow((float) (chunkDatas[chunkNum].chunkCoord.x - currentChunkX) * chunkSize, 2) +
                              Mathf.Pow((float) (chunkDatas[chunkNum].chunkCoord.y - currentChunkY) * chunkSize, 2) +
                              Mathf.Pow((float) (chunkDatas[chunkNum].chunkCoord.z - currentChunkZ) * chunkSize, 2);
            if (distSquared >= viewDistSquared)
            {
                var chunkData = chunkDatas[chunkNum];
                terrainPool.Push(chunkData.terrainChunk);
                chunkData.UnloadTerrainChunk();
                loadedChunkDictionary.Remove(chunkData.chunkCoord);
                
                chunkPool.Push(chunkData);
            }
        }
        //take out of range chunks out of chunkstoload
        
      
        
    } 

    private void RefreshOneChunk()
    {
        var chunkToLoad = chunksToLoad.Dequeue();
        LoadChunk(chunkToLoad);
        AssignOrCreate(loadedChunkDictionary[chunkToLoad]);
    }

    private void MeshOneChunk()
    {
        if (meshQueue.Count == 0)
        {
            return;
        }
        MeshChunk(meshQueue.Dequeue());
    }

    private void LoadChunk(Vector3 chunkCoord)
    {
        if (!loadedChunkDictionary.ContainsKey(chunkCoord))
        {
            ChunkData chunkData;
            if (chunkPool.Count != 0 && chunkPool.Peek() != null)
            {
                chunkData = chunkPool.Pop();
                chunkData.Refresh(chunkCoord);
            }
            else
            {
                chunkData = new ChunkData(chunkCoord);
            }
            
            loadedChunkDictionary.Add(chunkCoord, chunkData);
        }
    }

    private bool MeshChunk(Vector3 chunkCoord)
    {
        if (!IsMeshable(chunkCoord))
        {
            return false;
        }
        loadedChunkDictionary[chunkCoord].BuildMesh();
        return true;
    }

    private void AssignOrCreate(ChunkData chunkData)
    {
        if (terrainPool.Count != 0 && terrainPool.Peek() != null)
        {
            AssignTerrainChunk(chunkData);
        }
        else
        {
            CreateTerrainChunk(chunkData);
        }
    }

    private void AssignTerrainChunk(ChunkData chunkData)
    {
        if (!chunkData.isEmpty)
        {
            chunkData.AssignTerrainChunk(terrainPool.Pop());
        }
    }

    private void CreateTerrainChunk(ChunkData chunkData)
    {
        if (!chunkData.isEmpty)
        {
            GameObject chunkObject = Instantiate(chunkPrefab, chunkParent);
            var terrainChunk = chunkObject.AddComponent<TerrainChunk>();
            chunkData.AssignTerrainChunk(terrainChunk);
        }
    }

    private bool IsMeshable(Vector3 chunkCoord)
    {
        
        if (!loadedChunkDictionary.ContainsKey(chunkCoord) || loadedChunkDictionary[chunkCoord].isEmpty)
        {
            return false;
        }
        //check right
        if (!loadedChunkDictionary.ContainsKey(new Vector3(chunkCoord.x + 1, chunkCoord.y, chunkCoord.z)))
        {
            return false;
        }
        //check left
        if (!loadedChunkDictionary.ContainsKey(new Vector3(chunkCoord.x - 1, chunkCoord.y, chunkCoord.z)))
        {
            return false;
        }
        //check above
        if (!loadedChunkDictionary.ContainsKey(new Vector3(chunkCoord.x, chunkCoord.y, chunkCoord.z + 1)))
        {
            return false;
        }
        //check below
        if (!loadedChunkDictionary.ContainsKey(new Vector3(chunkCoord.x, chunkCoord.y, chunkCoord.z - 1)))
        {
            return false;
        }
        //check forward
        if (!loadedChunkDictionary.ContainsKey(new Vector3(chunkCoord.x, chunkCoord.y + 1, chunkCoord.z)))
        {
            return false;
        }
        //check backward
        if (!loadedChunkDictionary.ContainsKey(new Vector3(chunkCoord.x, chunkCoord.y - 1, chunkCoord.z)))
        {
            return false;
        }
        
        return true;

    }
    
    //todo add support for partially reinitializing world if the current chunk is not adjacent to last frames chunk... aka teleportation (currently this glitches things out)
}

[BurstCompile(CompileSynchronously = true)]
public struct DistanceChecker : IJobParallelFor
{
    [NativeDisableParallelForRestriction] public NativeArray<SByte> dists;
    [NativeDisableParallelForRestriction] public NativeArray<float3> coords;

    [NativeDisableParallelForRestriction] public NativeArray<float> viewDistsSquared;

    public float3 playerChunk;
    public int chunksInLinearDist;
    
    public void Execute(int index)
    {
        int doubleDist = 2 * chunksInLinearDist;
        
        int3 intCoord = Operations.GetIndex3(doubleDist, doubleDist, doubleDist, index);
        float3 baseCoord = new float3((float) intCoord.x, (float) intCoord.y, (float) intCoord.z);
        baseCoord.x += playerChunk.x - chunksInLinearDist;
        baseCoord.y += playerChunk.y - chunksInLinearDist;
        baseCoord.z += playerChunk.z - chunksInLinearDist;

        coords[index] = baseCoord;
        
        //check if basecoord within distance of playerchunk

        var distSquared = Mathf.Pow((baseCoord.x - playerChunk.x) * 32f, 2f) +
                          Mathf.Pow((baseCoord.y - playerChunk.y) * 32f, 2f) +
                          Mathf.Pow((baseCoord.z - playerChunk.z) * 32f, 2f);

        dists[index] = -1;

        for (int i = 0; i < 5; i++)
        {
            if (distSquared < viewDistsSquared[i])
            {
                dists[index] = (sbyte) i;
                break;
            }
        }
    }
}

