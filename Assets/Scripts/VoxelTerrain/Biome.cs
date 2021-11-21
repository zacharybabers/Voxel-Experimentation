using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Biome : MonoBehaviour
{
    [SerializeField] private List<FastNoiseUnity> additiveFastNoises;
    [SerializeField] private List<float> weights;
    [SerializeField] private List<FastNoiseUnity> multiplicativeFastNoises;
    [SerializeField] private int groundLevel = 0;
    [SerializeField] private int globalScale = 32;

    private void Awake()
    {
        if (weights.Count != additiveFastNoises.Count)
        {
            Debug.Log("Different number of additive fast noises and weights in this Biome.");
        }
    }

    private int[,] GenerateHeightMap(Vector3 chunkCoord)
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

    public int[,,] GenerateChunkAtlas(Vector3 chunkCoord)
    {
        var heightMap = GenerateHeightMap(chunkCoord);

        var chunkAtlas = new int[WorldInfo.chunkSize, WorldInfo.chunkSize, WorldInfo.chunkSize];

        for (int i = 0; i < WorldInfo.chunkSize; i++)
        {
            for (int j = 0; j < WorldInfo.chunkSize; j++)
            {
                for (int k = 0; k < WorldInfo.chunkSize; k++)
                {
                    if (chunkCoord.z * WorldInfo.chunkSize + k <= heightMap[i, j])
                    {
                        chunkAtlas[i, j, k] = 1;
                    }
                }
            }
        }

        return chunkAtlas;
    }


}
