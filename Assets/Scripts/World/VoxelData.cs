using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VoxelData
{
    public static readonly int ChunkWidth = 16;
    public static readonly int ChunkHeight = 256;
    public static readonly int WorldSizeInChunks = 100;
    public static readonly int TextureAtlasSizeInBlocks = 4;
    public static readonly int ViewDistanceInChunks = 5;

    public static int octaves = 1;
    public static float persistence = 1;
    public static float lacunarity = 1;
    public static int seed = 100;   
    public static float scale = 0.1f;

    public static int WorldSizeInVoxels
    {
        get { return WorldSizeInChunks * ChunkWidth;  }
    }
    public static float NormalizedBlockTextureSize
    {
        get { return 1f / (float)TextureAtlasSizeInBlocks; }
    }

    public static readonly Vector3[] vertices =
    {
        new Vector3(0,0,0),
        new Vector3(1,0,0),
        new Vector3(1,1,0),
        new Vector3(0,1,0),
        new Vector3(0,0,1),
        new Vector3(1,0,1),
        new Vector3(1,1,1),
        new Vector3(0,1,1),
    };
    public static readonly Vector3[] faceChecks =
    {
        new Vector3(0,0,-1),
        new Vector3(0,0,1),
        new Vector3(0,1,0),
        new Vector3(0,-1,0),
        new Vector3(-1,0,0),
        new Vector3(1,0,0),
    };
    public static readonly int[,] tris =
    {
        {0,3,1,2},  //back
        {5,6,4,7},  //front
        {3,7,2,6},  //top
        {1,5,0,4},  //bottom
        {4,7,0,3},  //left
        {1,2,5,6}   //right
    }; 
    public static readonly Vector2[] uvs =
    {
        new Vector2(0,0),
        new Vector2(0,1),
        new Vector2(1,0),
        new Vector2(1,1)
    };

    
}
