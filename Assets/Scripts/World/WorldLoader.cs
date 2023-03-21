using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class WorldLoader : MonoBehaviour
{
    public string fileName;
    //string path;
    public static World worldScript;

    // Start is called before the first frame update

    public static void saveWorld()
    {
        

        //Directory.CreateDirectory(Application.streamingAssetsPath + "/saves/");
        Directory.CreateDirectory(Application.dataPath + "/Resources/");

        //string path = Application.streamingAssetsPath + "/saves/" + "test4.txt";
        string path = Application.dataPath + "/Resources/" + "test5.txt";
        //string json = worldScript.getChunk(45, 45).SaveToString();

        //string json = JsonHelper.ToJson<Chunk>(worldScript.chunks);

        //StreamWriter writer = new StreamWriter(path);
        //writer.WriteLine("{");
        //string chunkName = worldScript.getChunk(45,45).coord.x.ToString();


        string json = "";
        
        for (int i = 0; i < VoxelData.WorldSizeInChunks; i++)
        {
            for (int j = 0; j < VoxelData.WorldSizeInChunks; j++)
            {
                if(worldScript.chunks[i,j]!=null)
                {
                    string chunkName = "Chunk " + worldScript.getChunk(i, j).coord.x + " " + worldScript.getChunk(i, j).coord.z + ",\n";
                    json += chunkName;

                    for (int x = 0; x < VoxelData.ChunkWidth; x++)
                    {
                        for (int y = 0; y < VoxelData.ChunkHeight; y++)
                        {
                            for (int z = 0; z < VoxelData.ChunkWidth; z++)
                            {
                                if (worldScript.getChunk(i,j).voxelMap[x, y, z] != 0)
                                {
                                    json += x + " " + y + " " + z + " " + worldScript.getChunk(i,j).voxelMap[x, y, z] + ",\n";
                                }

                            }
                        }
                    }
                }
                
            }
        }
        
        File.WriteAllText(path, json);
        Debug.Log("World saved");
    }


    public static void readFile(World world)
    {

        //string path = Application.streamingAssetsPath +  "/" + fileName;
        worldScript = world;
        string textFile = Resources.Load<TextAsset>("test5").ToString();

        string[] lines = textFile.Split(',');

        Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];

        int xChunk = -1;
        int zChunk = -1;
        byte[,,] voxelMap = new byte[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];
        for(int i = 0; i < lines.Length-1; i++)
        {
            if(lines[i].Contains("Chunk"))
            {
                //Save voxelMap of previous chunk
                if(i!=0)
                {
                    worldScript.chunks[xChunk, zChunk].setVoxelmap(voxelMap);
                }
                
                //Starts reading new chunk
                string[] chunkdata = lines[i].Split(' ');
                xChunk = Int32.Parse(chunkdata[1]);
                zChunk = Int32.Parse(chunkdata[2]);
                worldScript.chunks[xChunk, zChunk] = new Chunk(new ChunkCoord(xChunk,zChunk), worldScript, false);
                worldScript.loadedChunks.Add(new ChunkCoord(xChunk, zChunk));
                voxelMap = new byte[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];
            } else
            {
                string[] blockData = lines[i].Split(' '); // 0-> x, 1-> y, 2-> z, 3->BLOCK_ID
                int x = Int32.Parse(blockData[0]);
                int y = Int32.Parse(blockData[1]);
                int z = Int32.Parse(blockData[2]);
                byte blockId = Byte.Parse(blockData[3]);
                voxelMap[x, y, z] = blockId;
            }
        }

        //Save voxelmap in last chunk
        if(xChunk != -1 && zChunk!= -1)
        {
            worldScript.chunks[xChunk, zChunk].setVoxelmap(voxelMap);

        }
        
    }

}