using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelHeightMapGenerator : MonoBehaviour
{
    [SerializeField] private NoiseGenerator noiseGenerator;

    private void GenerateHeightMap(){
        var initNoiseMap = noiseGenerator.heightMap();
        var voxelHeightMap = new int[initNoiseMap.GetLength(0),initNoiseMap.GetLength(1)];
        string visrep = "";

        for(int x = 0; x < initNoiseMap.GetLength(0); x++){
            visrep += "\n";
            for(int y = 0; y < initNoiseMap.GetLength(1); y++){
                float temp = initNoiseMap [x,y];
                temp *= 50;
                voxelHeightMap[x,y] = Mathf.RoundToInt(temp);
                visrep+= voxelHeightMap[x,y] +" ";
            }
        } 
        print(visrep);
    }

    void Start(){
        GenerateHeightMap();
    }
}
