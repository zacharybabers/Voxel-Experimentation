using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseGenerator : MonoBehaviour
{
    public int width;
    public int height;
    public int seedNumber;
    public float noiseScale;
    public int noiseOctave;
    public float noisePersistence;
    public float noiseLacunarity;
    public Vector2 noiseOffset;

    private float[,] myNoiseMap;

   

    private float[,] heightMap()
    {
        return Noise.GenerateNoiseMap(width, height, seedNumber, noiseScale, noiseOctave, noisePersistence,
            noiseLacunarity, noiseOffset);
    }

    private void Start()
    {
        myNoiseMap = heightMap();
        foreach (float number in myNoiseMap)
        {
            print(number);
        }
    }
}