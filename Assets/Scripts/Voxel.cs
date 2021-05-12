using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public static class Voxel
{
    public static readonly int chunkSize = 16;
    public static readonly int chunkHeight = 256;

    public static readonly int AtlasSize = 4;

    public static readonly int RenderDistance = 41;

    public static float NormalizedBlockTextureSize
    {
        get { return 1f / (float)AtlasSize; }
    }
    public static readonly Vector3[] voxelVerts = new Vector3[]
    {
    new Vector3(0.0f,0.0f,0.0f),
    new Vector3(1.0f,0.0f,0.0f),
    new Vector3(1.0f,1.0f,0.0f),
    new Vector3(0.0f,1.0f,0.0f),
    new Vector3(0.0f,0.0f,1.0f),
    new Vector3(1.0f,0.0f,1.0f),
    new Vector3(1.0f,1.0f,1.0f),
    new Vector3(0.0f,1.0f,1.0f),
};
    public static readonly int[,] voxelTri = new int[,]
    {
        {0,3,1,2 },//back
        {5,6,4,7 },//front
        {4,7,0,3 },//left
        {1,2,5,6 },//right
        {3,7,2,6 },//top
        {1,5,0,4 }//bottom
    };
    public static readonly Vector3[] face = new Vector3[]
    {
    new Vector3(0.0f,0.0f,-1.0f),
    new Vector3(0.0f,0.0f,1.0f),
    new Vector3(-1.0f,0.0f,0.0f),
    new Vector3(1.0f,0.0f,0.0f),
    new Vector3(0.0f,1.0f,0.0f),
    new Vector3(0.0f,-1.0f,0.0f),
    };
}
