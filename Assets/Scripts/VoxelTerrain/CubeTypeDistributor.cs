using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CubeTypeDistributor : MonoBehaviour
{
    
    public void UpdateIDs()
    {
        var cubeTypes = FindObjectsOfType<CubeType>();
        var cubeList = cubeTypes.ToList();
        List<int> cubeIDs = new List<int>();
        foreach (var cubeType in cubeList)
        {
            cubeIDs.Add(cubeType.GetBlockID());
        }
        cubeIDs.Sort();
        cubeIDs.Insert(0, 0);
        foreach (var cubeType in cubeList)
        {
            cubeType.SetNewID(cubeIDs.IndexOf(cubeType.GetBlockID()));
        }
    }
}
