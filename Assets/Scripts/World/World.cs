using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public Transform player;
    public Vector3 spawnPosition;

    public BiomeAttributes biome;
    public Material material;
    public BlockType[] blockTypes;

    public int seed;

    public Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];
    

    List<ChunkCoord> activeChunks = new List<ChunkCoord>();

    public ChunkCoord playerChunkCoord;
    public ChunkCoord playerLastChunkCoord;

    List<ChunkCoord> chunksToCreate = new List<ChunkCoord>();

    private bool isCreatingChunks;

    public GameObject debugScreen;

    public List<ChunkCoord> loadedChunks = new List<ChunkCoord>();

    private void Start()
    {
        Random.InitState(seed);

        spawnPosition = new Vector3((VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2, 100f, (VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2);
        //WorldLoader.readFile(this);
        GenerateWorld();
        playerLastChunkCoord = GetChunkCoordFromVector3(player.position);
    }

    private void Update()
    {
        playerChunkCoord = GetChunkCoordFromVector3(player.position);
        if (!playerChunkCoord.Equals(playerLastChunkCoord))
        {
            CheckViewDistance();
            playerLastChunkCoord = playerChunkCoord;
        }

        if(chunksToCreate.Count>0 && !isCreatingChunks)
        {
            StartCoroutine("CreateChunks");
        }

        if(Input.GetKeyDown(KeyCode.F3)) {
            debugScreen.SetActive(!debugScreen.activeSelf);
        }
        
    }

    public Chunk getChunk(int i, int j)
    {
        return chunks[i, j];
    }

    void GenerateWorld()
    {
        

        for (int x = (VoxelData.WorldSizeInChunks/2) - VoxelData.ViewDistanceInChunks; x < (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceInChunks; x++)
        {
            for (int z = (VoxelData.WorldSizeInChunks / 2) - VoxelData.ViewDistanceInChunks; z < (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceInChunks; z++)
            {
                if(chunks[x,z] !=null)
                {
                    chunks[x, z].Init();
                    activeChunks.Add(new ChunkCoord(x, z));
                    loadedChunks.Remove(new ChunkCoord(x, z));
                } else
                {
                    chunks[x, z] = new Chunk(new ChunkCoord(x, z), this, true);
                    activeChunks.Add(new ChunkCoord(x, z));
                }
                
            }
        }

        player.position = spawnPosition;
    }

    IEnumerator CreateChunks()
    {
        isCreatingChunks = true;
        while(chunksToCreate.Count>0)
        {
            chunks[chunksToCreate[0].x, chunksToCreate[0].z].Init();
            chunksToCreate.RemoveAt(0);
            yield return null;
        }

        isCreatingChunks = false;

    }

    ChunkCoord GetChunkCoordFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x) / VoxelData.ChunkWidth;
        int z = Mathf.FloorToInt(pos.z) / VoxelData.ChunkWidth;
        return new ChunkCoord(x, z);
    }

    void CheckViewDistance()
    {
        ChunkCoord coord = GetChunkCoordFromVector3(player.position);
        List<ChunkCoord> previouslyActiveCunks = new List<ChunkCoord>(activeChunks);

        for (int x = coord.x - VoxelData.ViewDistanceInChunks;  x < coord.x + VoxelData.ViewDistanceInChunks;  x++)
        {
            for (int z = coord.z - VoxelData.ViewDistanceInChunks; z < coord.z + VoxelData.ViewDistanceInChunks; z++)
            {
                if(isChunkInWorld(new ChunkCoord(x,z)))
                {
                    if(chunks[x, z] == null) {
                        chunks[x, z] = new Chunk(new ChunkCoord(x, z),this, false);
                        chunksToCreate.Add(new ChunkCoord(x, z));
                    } else if(!chunks[x,z].isActive)
                    {
                        chunks[x, z].isActive = true;
                    }
                    /*
                    if(chunks[x,z]!=null)
                    { 
                        
                        if(!chunks[x,z].isVoxelMapPopulated)
                        {
                            chunks[x, z].Init();
                        }
                        
                        if (loadedChunks.Contains(new ChunkCoord(x, z)))
                        {
                            chunks[x, z].UpdateChunk();
                            Debug.Log("contains");
                            loadedChunks.Remove(new ChunkCoord(x, z));
                        }
                        activeChunks.Add(new ChunkCoord(x, z));
                    }*/
                    
                }
                for (int i = 0; i < previouslyActiveCunks.Count; i++)
                {
                    if(previouslyActiveCunks[i].Equals(new ChunkCoord(x,z)))
                    {
                        previouslyActiveCunks.RemoveAt(i);
                    }
                }
            }
        }

        foreach(ChunkCoord c in previouslyActiveCunks)
        {
            chunks[c.x, c.z].isActive = false;
            //Debug.Log("xd");
        }
    }
    public byte GetVoxel(Vector3 pos)
    {
        int yPos = Mathf.FloorToInt(pos.y);
        // IMMUTABLE PASS


        if (!isVoxelInWorld(pos))
        {
            return 0;
        }

        if(yPos == 0) 
        {
            return 4; //Return bedrock
        }

        //Basic terrain pass
        int terrainHeigth = Mathf.FloorToInt(biome.terrainHeigth * Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.terrainScale)) + biome.solidGroundHeight;
        byte voxelValue = 0;
        if (yPos == terrainHeigth)
        {
            voxelValue = 3; //GRASS
        } else if (yPos < terrainHeigth && yPos > terrainHeigth - 4) {
            voxelValue = 2; //DIRT  
        }
        else if (yPos < terrainHeigth)
        {
            voxelValue = 1; //STONE 
        } else
        {
            return 0;
        }

        //Second pass: lodes
        if(voxelValue == 1)
        {
            foreach(Lode lode in biome.lodes)
            {
                if(yPos > lode.minHeight && yPos < lode.maxHeight)
                {
                    if(Noise.Get3DPerlin(pos, lode.noiseOffset, lode.scale, lode.threshold))
                    {
                        voxelValue = lode.blockID;
                    }
                }
            }
        }
        return voxelValue;


        //third pass: trees


    }
    public Chunk GetChunkFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x) / VoxelData.ChunkWidth;
        int z = Mathf.FloorToInt(pos.z) / VoxelData.ChunkWidth;
        return chunks[x, z];
    }

    bool isChunkInWorld(ChunkCoord coord)
    {
        return (coord.x > 0 && coord.x < VoxelData.WorldSizeInChunks - 1 && (coord.z > 0 && coord.z < VoxelData.WorldSizeInChunks - 1));
    }
    bool isVoxelInWorld(Vector3 coord)
    {
        return (coord.x >= 0 && coord.x < VoxelData.WorldSizeInVoxels && coord.y >= 0 && coord.y < VoxelData.ChunkHeight && (coord.z >= 0 && coord.z < VoxelData.WorldSizeInVoxels));
    }

    public bool CheckForVoxel(Vector3 pos)
    {
        ChunkCoord thisChunk = new ChunkCoord(pos);

        if(!isChunkInWorld(thisChunk) || pos.y < 0 || pos.y > VoxelData.ChunkHeight)
        {
            return false;
        }

        if(chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].isVoxelMapPopulated)
        {
            return blockTypes[chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlobalVector3(pos)].isSolid;
        }

        return blockTypes[GetVoxel(pos)].isSolid;

    }

    public Vector3 GetNearestNonSolidBlock(Vector3 pos)
    {
        Vector3 newPos = Vector3.zero;
        int minRange = 0, maxRange = 0;
        int xCheck;
        int yCheck;
        int zCheck;
        while (newPos==Vector3.zero)
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    for (int z = -1; z <= 1; z++)
                    {
                        xCheck = x<0 ? x+minRange : x+maxRange;
                        yCheck = y<0 ? y+minRange : y+maxRange;
                        zCheck = z<0 ? z+minRange : z+maxRange;
                        if (CheckForVoxel(new Vector3(xCheck+pos.x, yCheck+pos.y, zCheck+pos.z)))
                        {
                            newPos = new Vector3(xCheck, yCheck, zCheck);
                            return newPos;
                        }
                    }
                }
            }
            minRange--;
            maxRange++;
        }

        return Vector3.zero;
    }
}


[System.Serializable]
public class BlockType
{
    public string blockName;
    public bool isSolid;



    [Header("Texture Values")]
    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;


    //back
    //front
    //top
    //bottom
    //left
    //right
    public int GetTextureID(byte faceIndex)
    {
        switch(faceIndex)
        {
            case 0:
                return backFaceTexture;
            case 1:    
                return frontFaceTexture;
            case 2:    
                return topFaceTexture;
            case 3:    
                return bottomFaceTexture;
            case 4:
                return leftFaceTexture;
            case 5:
                return rightFaceTexture;
            default:
                Debug.Log("Error in GetTextureID: invalid face index");
                return 0;
        }
    } 


    

}
