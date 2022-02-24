using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class Biome : MonoBehaviour
{
    
    [SerializeField] private string name = "biome";
    
    public int topBlock;
    public int innerBlock;
    
    [SerializeField] private List<FastNoiseUnity> additiveFastNoises;
    [SerializeField] private List<float> weights;
    [SerializeField] private List<FastNoiseUnity> multiplicativeFastNoises;
    [SerializeField] private int globalScale = 32;
    [SerializeField] private int topLayerHeight = 5;
    [SerializeField] private FastNoiseUnity innerFastNoise;
    [SerializeField] private int innerBlockBaseLevel = 0;
    [SerializeField] private int innerScale = 16;
    [SerializeField] private bool innerLayerAdditive;
    

    private void Awake()
    {
        if (weights.Count != additiveFastNoises.Count)
        {
            Debug.Log("Different number of additive fast noises and weights in this Biome.");
        }
        
    }

    public void UpdateCubeTypes()
    {
        var cubeTypes = FindObjectsOfType<CubeType>();

        bool innerChanged = false;
        bool topChanged = false;
        
        foreach (var cubeType in cubeTypes)
        {
            if (innerBlock == cubeType.GetBlockID() && !innerChanged)
            {
                innerBlock = cubeType.GetNewID();
                innerChanged = true;
            }

            if (topBlock == cubeType.GetBlockID() && !topChanged)
            {
                topBlock = cubeType.GetNewID();
                topChanged = true;
            }
        }
    }

    private int[,] GenerateHeightMap(Vector3 chunkCoord) //generate 'top block'/surface heightmap
    {
        int[,] initMap = new int[WorldInfo.chunkSize, WorldInfo.chunkSize];

        for (int blockX = 0; blockX < WorldInfo.chunkSize; blockX++)
        {
            for (int blockY = 0; blockY < WorldInfo.chunkSize; blockY++)
            {
                float result = 0;
                
                for (int i = 0; i < additiveFastNoises.Count; i++)
                {
                    float initNoise = additiveFastNoises[i].fastNoise.GetNoise(chunkCoord.x * WorldInfo.chunkSize + blockX, chunkCoord.y * WorldInfo.chunkSize + blockY); //Get initial noise value at coord (Domain: [-1,1])
                    initNoise = (initNoise + 1f) * 0.5f; //Shift value's domain to [0,1]
                    result += initNoise * weights[i]; //multiply noise by its weight and add it to result
                }

                result *= globalScale;

                for (int i = 0; i < multiplicativeFastNoises.Count; i++)
                {
                    float positiveNoise = multiplicativeFastNoises[i].fastNoise.GetNoise(chunkCoord.x * WorldInfo.chunkSize + blockX, chunkCoord.y * WorldInfo.chunkSize + blockY) + 1f;
                    result *= positiveNoise;
                }
                initMap[blockX, blockY] = (int) result;
            }
        }

        return initMap;
    }
    
    //private int GenerateInnerHeightMap    //generate 'inner block'/stone heightmap
    private int[,] GenerateInnerHeightMap(Vector3 chunkCoord)
    {
        int[,] initMap = new int[WorldInfo.chunkSize, WorldInfo.chunkSize];
        for (int blockX = 0; blockX < WorldInfo.chunkSize; blockX++)
        {
            for (int blockY = 0; blockY < WorldInfo.chunkSize; blockY++)
            {
                float result = innerFastNoise.fastNoise.GetNoise(chunkCoord.x * WorldInfo.chunkSize + blockX,chunkCoord.y * WorldInfo.chunkSize + blockY) * innerScale + innerBlockBaseLevel;
                initMap[blockX, blockY] = (int) result;
            }
        }

        return initMap;
    }
    
    //private int[,,] Process3DNoise //go through 3d noise functions w given thresholds and turn blocks to air for caves, or do reverse for overhangs and things

    public byte[,,] GenerateChunkAtlas(Vector3 chunkCoord)
    {
        var heightMap = GenerateHeightMap(chunkCoord);
        var innerHeightMap = GenerateInnerHeightMap(chunkCoord);

        var chunkAtlas = new byte[WorldInfo.chunkSize, WorldInfo.chunkSize, WorldInfo.chunkSize];

        for (int i = 0; i < WorldInfo.chunkSize; i++)
        {
            for (int j = 0; j < WorldInfo.chunkSize; j++)
            {
                for (int k = 0; k < WorldInfo.chunkSize; k++)
                {
                    var thisGlobalHeight = (int) chunkCoord.z * WorldInfo.chunkSize + k;

                    if (thisGlobalHeight <= heightMap[i, j] - topLayerHeight)
                    {
                        chunkAtlas[i, j, k] = (byte)innerBlock;
                    }
                    else if (thisGlobalHeight <= heightMap[i, j])
                    {
                        chunkAtlas[i, j, k] = (byte)topBlock;
                    }

                    if (thisGlobalHeight <= innerHeightMap[i, j])
                    {
                        if (innerLayerAdditive)
                        {
                            chunkAtlas[i, j, k] = (byte) innerBlock;
                        }
                        else if (chunkAtlas[i, j, k] == topBlock)
                        {
                            chunkAtlas[i, j, k] = (byte)innerBlock;
                        }
                    }
                }
            }
        }

        return chunkAtlas;
    }


}
