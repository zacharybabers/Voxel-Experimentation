using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class CubeType : MonoBehaviour
{
    [SerializeField] private string name = "Block";
    [SerializeField] private int blockID;
    [SerializeField] private Vector2Int topTexture;
    [SerializeField] private Vector2Int sideTexture;
    [SerializeField] private Vector2Int bottomTexture;

    int newID = 0;

    public int GetBlockID()
    {
        return blockID;
    }

    public void SetNewID(int id)
    {
        newID = id;
    }

    public int GetNewID()
    {
        return newID;
    }

    public Vector2Int GetTopTextureCoord()
    {
        return topTexture;
    }
    
    public Vector2Int GetBottomTextureCoord()
    {
        return bottomTexture;
    }
    
    public Vector2Int GetSideTextureCoord()
    {
        return sideTexture;
    }
}
