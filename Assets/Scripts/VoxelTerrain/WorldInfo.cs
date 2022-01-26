using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class WorldInfo : MonoBehaviour
{
    public static Dictionary<Vector3, ChunkData> loadedChunkDictionary;
    public Queue<Vector3> chunksToLoad;
    public Queue<Vector3> meshQueue;
    public List<ChunkData> chunkPool;
    public List<TerrainChunk> unusedTerrainChunks;

    [SerializeField] private int chunkPoolSize = 9;
    [SerializeField] private int nonLoadedChunkSize = 9;
    [SerializeField] private int chunksPerFrame = 5;
    [SerializeField] private int worldLOD = 0;

    private static bool viewerNewChunkThisFrame;
    public const int chunkSize = 32;

    public static CulledMeshBuilder meshBuilder;
    public static TerrainGenerator terrainGenerator;
    public static int[,,] emptyAtlas;

    [SerializeField] private Transform chunkParent;
    [SerializeField] private GameObject chunkPrefab;
  

    [SerializeField] private Transform targetTransform;
    private Vector3Int transformChunk;
    private Vector3Int lastTransformChunk;
    
    private float drawDistance;
    [SerializeField] private float[] lodDistances;

    private void Awake()
    {
        emptyAtlas = new int[chunkSize,chunkSize,chunkSize];
        drawDistance = lodDistances[4];
        InitCubeTypes();
        meshBuilder = gameObject.AddComponent<CulledMeshBuilder>();
        meshBuilder.InitMeshBuilder();
        meshBuilder.mainLOD = worldLOD;
        terrainGenerator = gameObject.GetComponent<TerrainGenerator>();
        loadedChunkDictionary = new Dictionary<Vector3, ChunkData>();
        chunksToLoad = new Queue<Vector3>();
        meshQueue = new Queue<Vector3>();
        chunkPool = new List<ChunkData>();
        unusedTerrainChunks = new List<TerrainChunk>();
        terrainGenerator.InitializeLookupTable();
        meshBuilder.uvLookup = terrainGenerator.uvLookup;
        //fill unused terrain chunks
        for (int i = 0; i < nonLoadedChunkSize; i++)
        {
            GameObject chunkObject = Instantiate(chunkPrefab, chunkParent);
            var terrainChunk = chunkObject.AddComponent<TerrainChunk>();
            unusedTerrainChunks.Add(terrainChunk);
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
                var viewDistSquared = Mathf.Pow(lodDistances[i], 2);
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
            UpdateLODNeeded();
            FindInitialChunksToLoad();
            UnloadChunks();
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
                var viewDistSquared = Mathf.Pow(lodDistances[i], 2);
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
                unusedTerrainChunks.Add(chunkData.terrainChunk);
                chunkData.UnloadTerrainChunk();
                loadedChunkDictionary.Remove(chunkData.chunkCoord);
                
                chunkPool.Add(chunkData);
                if (chunkPool.Count > chunkPoolSize)
                {
                    chunkPool.RemoveAt(0);
                }
            }
        }
        //take out of range chunks out of chunkstoload
        
    } 

    private void RefreshOneChunk()
    {
        var chunkToLoad = chunksToLoad.Dequeue();
        
        bool gotChunk = false;

        /*for (int i = chunkPool.Count - 1; i >= 0; i--)
        {
            bool finished = false;
            if (chunkPool[i].chunkCoord.Equals(chunkToLoad))
            {
                loadedChunkDictionary.Add(chunkToLoad, chunkPool[i]);
                if (!chunkPool[i].HasMesh())
                {
                    MeshChunk(chunkToLoad);
                }
                chunkPool.RemoveAt(i);
                gotChunk = true;
                finished = true;
            }

            if (finished)
            {
                break;
            }
        }*/
       
        
        if (!gotChunk)
        {
            LoadChunk(chunkToLoad);
        }

        CreateTerrainChunk(loadedChunkDictionary[chunkToLoad]);
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
            loadedChunkDictionary.Add(chunkCoord, new ChunkData(chunkCoord));
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
        if (unusedTerrainChunks.Count == 0)
        {
            CreateTerrainChunk(chunkData);
        }
        else
        {
            AssignTerrainChunk(chunkData);
        }
    }

    private void AssignTerrainChunk(ChunkData chunkData)
    {
        if (!chunkData.isEmpty)
        {
            Debug.Log("the terrain chunk is: " + (unusedTerrainChunks[unusedTerrainChunks.Count - 1] == null));
            chunkData.AssignTerrainChunk(unusedTerrainChunks[unusedTerrainChunks.Count - 1]);
            unusedTerrainChunks.RemoveAt(unusedTerrainChunks.Count - 1);
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

