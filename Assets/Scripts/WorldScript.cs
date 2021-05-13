using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class WorldScript : MonoBehaviour
{
    [SerializeField]GameObject Player;

    private PlayerController playerController;
    public Material material;
    public BlockType[] blockTypes;

    public bool isSeedRandom;
    public float seed;
    public float Xscale = 1;
    public float Zscale = 1;
    public float Xnoise = 0.15f;
    public float Znoise = 0.15f;
    public float Yscale = 2;
    public float perlinWaves = 5;
    public int GroundLevel = 58;

    public int WorldRenderPositionX = 0;
    public int WorldRenderPositionY = 0;

    List<Tuple<Chunk, int, int>> WorldChunks = new List<Tuple<Chunk, int, int>>();
    public Queue<Chunk> ChunksToHaveMeshAdded = new Queue<Chunk>();
    private Queue<Tuple<int, int, int>> QueuedChunks = new Queue<Tuple<int, int, int>>();
    static int MaxChunks = 2;
    public Chunk currentChunk;
    void Start()
    {
        playerController = Player.GetComponent<PlayerController>();

        if(isSeedRandom)seed = (int)System.DateTime.Now.Ticks;
        InitializeMap(Voxel.RenderDistance);
        //UpdateActiveChunks();
    }

    // Update is called once per frame
    int i = 0;
    void Update()
    {
        int PlayerPositionX = Mathf.FloorToInt(Player.transform.position.x) / 16;
        int PlayerPositionY = Mathf.FloorToInt(Player.transform.position.z) / 16;
        if (PlayerPositionX != WorldRenderPositionX || PlayerPositionY != WorldRenderPositionY)
        {
            WorldRenderPositionX = PlayerPositionX;
            WorldRenderPositionY = PlayerPositionY;
            MoveRender(WorldRenderPositionX, WorldRenderPositionY);
        }
        
        if (i == 20)
        {
            DeactivateChunks();
            i = 0;
        }
        else i++;
    }
    private void LateUpdate()
    {
        //UpdateActiveChunks();
        QueueChunkCreation();
        AddMeshes();
    }
    void InitializeMap(int RenderDistance)
    {
        Chunk newChunck = new Chunk(this, WorldRenderPositionX, WorldRenderPositionY , RenderDistance);
        WorldChunks.Add(new Tuple<Chunk, int, int>(newChunck, WorldRenderPositionX, WorldRenderPositionY));
        currentChunk = newChunck;
    }
    public void AddToQueueCreation(int WorldPosX,int WorldPosY,int RenderDistance)
    {
        if (FindChunk(WorldPosX, WorldPosY) == null && !QueuedChunks.Contains(new Tuple<int, int, int>(WorldPosX, WorldPosY, RenderDistance))) QueuedChunks.Enqueue(new Tuple<int, int , int>(WorldPosX, WorldPosY , RenderDistance));
    }
    public void QueueChunkCreation()
    {
        for (int i = 0; i < MaxChunks; i++)
        {
            if (QueuedChunks.Count > 0)
            {

                Chunk newChunck = new Chunk(this, QueuedChunks.Peek().Item1, QueuedChunks.Peek().Item2, QueuedChunks.Peek().Item3);
                WorldChunks.Add(new Tuple<Chunk, int, int>(newChunck, QueuedChunks.Peek().Item1, QueuedChunks.Peek().Item2));
                QueuedChunks.Dequeue();
            }
        }
    }
    public void AddMeshes()
    {
        for (int i = 0; i < MaxChunks; i++)
        {
            if (ChunksToHaveMeshAdded.Count > 0)
            {

                if(!ChunksToHaveMeshAdded.Peek().threadLocked) ChunksToHaveMeshAdded.Peek().AddMesh();
                ChunksToHaveMeshAdded.Dequeue();
            }
        }
    }
    public Chunk CreateNewChunk(int WorldX,int WorldY, int RenderDistance)
    {
        Chunk newChunck = new Chunk(this, WorldX, WorldY, RenderDistance);
        WorldChunks.Add(new Tuple<Chunk, int, int>(newChunck, WorldX, WorldY));
        return newChunck;
    }
    public Chunk FindChunk(int WorldPosX,int WorldPosY)
    {
        foreach (Tuple<Chunk, int, int> chunk in WorldChunks)
        {
            if (chunk.Item2 == WorldPosX && chunk.Item3 == WorldPosY) return chunk.Item1;
        }
        return null;
    }
    void UpdateActiveChunks()
    {
        foreach (Tuple<Chunk,int,int> chunks in WorldChunks)
        {
            if (chunks.Item1.IsActive) chunks.Item1.UpdateChunk();
        }
    }
    void MoveRender(int PosX,int PosY)
    {
        currentChunk = FindChunk(PosX, PosY);
        currentChunk.AttemptExpansion(Voxel.RenderDistance);
        TagOffExpansion();
    }
    void DeactivateChunks()
    {
        foreach (Tuple<Chunk, int, int> chunks in WorldChunks)
        {
            if (chunks.Item1.IsActive && (Mathf.Abs(WorldRenderPositionX-chunks.Item2)>Voxel.RenderDistance || Mathf.Abs(WorldRenderPositionY-chunks.Item3)>Voxel.RenderDistance)) chunks.Item1.IsActive = false;
        }
    }
    void TagOffExpansion()
    {
        foreach (Tuple<Chunk, int, int> chunks in WorldChunks)
        {
            chunks.Item1.TagOffExpansion();
        }
    }
    public bool CheckForVoxel(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x);
        int y = Mathf.FloorToInt(position.y);
        int z = Mathf.FloorToInt(position.z);
        Chunk chunk = FindChunk(x / Voxel.chunkSize, z / Voxel.chunkSize);
        if (chunk == null) return false;
        return blockTypes[chunk.GetVoxel(new Vector3(x % Voxel.chunkSize, y, z % Voxel.chunkSize))].isSolid;
    }
    public int GetHighestBlock(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x);
        int y = Mathf.FloorToInt(position.y);
        int z = Mathf.FloorToInt(position.z);
        Chunk chunk = FindChunk(x / Voxel.chunkSize, z / Voxel.chunkSize);
        int start = (y - 1 < Voxel.chunkHeight) ? y : Voxel.chunkHeight - 1;
        if (chunk == null) return start;
        for (int i = start; i >= 0; i--)
        {
            if (blockTypes[chunk.GetVoxel(new Vector3(x % Voxel.chunkSize, i, z % Voxel.chunkSize))].isSolid) 
            {
                Debug.Log(i);
                return i; 
            }
        }
        return start;
    }
}
[System.Serializable]
public class BlockType
{
    public string blockname;
    public bool isSolid;
    public int[] sideTextures = new int[6];
}
