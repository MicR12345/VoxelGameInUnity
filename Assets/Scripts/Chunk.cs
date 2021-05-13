using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

public class Chunk
{
    GameObject chunkObject;
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    MeshCollider meshCollider;

    WorldScript world;

    int vertexIndex = 0;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    public bool threadLocked = false;

    int[,,] voxelMap = new int[Voxel.chunkSize, Voxel.chunkHeight, Voxel.chunkSize];
    int[,] heightMap = new int[Voxel.chunkSize, Voxel.chunkSize];

    private int WorldPositionX;
    private int WorldPositionZ;

    public bool wasRecentlyExpanded = false;

    private Chunk[] neighbours = new Chunk[4];
    private Vector2 WorldPosition
    {
        get { return new Vector2(WorldPositionX, WorldPositionZ); }
    }
    public Vector3 position
    {
        get { return chunkObject.transform.position; }
    }
    public void FindNeighbours()
    {
        neighbours[0] = world.FindChunk(WorldPositionX + 1, WorldPositionZ);
        neighbours[1] = world.FindChunk(WorldPositionX - 1, WorldPositionZ);
        neighbours[2] = world.FindChunk(WorldPositionX, WorldPositionZ + 1);
        neighbours[3] = world.FindChunk(WorldPositionX, WorldPositionZ - 1);
    }
    public Chunk NeighbourXpositive
    {
        get { return neighbours[0]; }
    }
    public Chunk NeighbourXnegative
    {
        get { return neighbours[1]; }
    }
    public Chunk NeighbourZpositive
    {
        get { return neighbours[2]; }
    }
    public Chunk NeighbourZnegative
    {
        get { return neighbours[3]; }
    }
    public bool IsActive
    {
        get { return chunkObject.activeSelf; }
        set { chunkObject.SetActive(value); if(value)UpdateChunk(); }
    }
    public int GetVoxel(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x);
        int y = Mathf.FloorToInt(position.y);
        int z = Mathf.FloorToInt(position.z);
        if (IsVoxelInChunk(x, y, z)) return voxelMap[x, y, z];
        else
        {
            if (x >= Voxel.chunkSize)
            {
                if (neighbours[0] == null) return 0;
                else return neighbours[0].GetVoxel(new Vector3(x % Voxel.chunkSize, y, z));
            }
            if (x < 0)
            {
                if (neighbours[1] == null) return 0;
                else return neighbours[1].GetVoxel(new Vector3(Voxel.chunkSize + x - 1, y, z));
            }
            if (z >= Voxel.chunkSize)
            {
                if (neighbours[2] == null) return 0;
                else return neighbours[2].GetVoxel(new Vector3(x, y, z%Voxel.chunkSize));
            }
            if (z < 0)
            {
                if (neighbours[3] == null) return 0;
                else return neighbours[3].GetVoxel(new Vector3(x, y, Voxel.chunkSize + z - 1));
            }
        }
        return 0;
    }
    public Chunk(WorldScript world,int WorldPositionX,int WorldPositionZ)
    {
        this.world = world;
        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshRenderer.material = world.material;
        chunkObject.transform.SetParent(world.transform);

        meshCollider = chunkObject.AddComponent<MeshCollider>();
        meshCollider.convex = true;
        this.WorldPositionX = WorldPositionX;
        this.WorldPositionZ = WorldPositionZ;

        chunkObject.name = "Chunk" + WorldPositionX + "," + WorldPositionZ;

        GenerateVoxelMap();
    }
    public Chunk(WorldScript world, int WorldPositionX, int WorldPositionZ,int RenderDistance)
    {
        this.world = world;
        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshRenderer.material = world.material;
        chunkObject.transform.SetParent(world.transform);

        meshCollider = chunkObject.AddComponent<MeshCollider>();
        meshCollider.convex = true;
        this.WorldPositionX = WorldPositionX;
        this.WorldPositionZ = WorldPositionZ;

        chunkObject.name = "Chunk" + WorldPositionX + "," + WorldPositionZ;

        AttemptExpansion(RenderDistance);

        FindNeighbours();
        GenerateVoxelMap();
        UpdateChunk();
    }
    public void AttemptExpansion(int RenderDistance)
    {
        if (RenderDistance > 0)
        {
            Chunk Xpositive = (neighbours[0] == null) ? world.FindChunk(WorldPositionX + 1, WorldPositionZ) : neighbours[0];
            Chunk Xnegative = (neighbours[1] == null) ? world.FindChunk(WorldPositionX - 1, WorldPositionZ) : neighbours[1];
            Chunk Zpositive = (neighbours[2] == null) ? world.FindChunk(WorldPositionX, WorldPositionZ + 1) : neighbours[2];
            Chunk Znegative = (neighbours[3] == null) ? world.FindChunk(WorldPositionX, WorldPositionZ - 1) : neighbours[3];

            if (Xpositive == null)world.AddToQueueCreation(WorldPositionX + 1, WorldPositionZ, RenderDistance - 1);
            else
            {
                neighbours[0] = Xpositive;
                if (!neighbours[0].wasRecentlyExpanded)
                {
                    neighbours[0].wasRecentlyExpanded = true;
                    neighbours[0].IsActive = true;
                    Xpositive.AttemptExpansion(RenderDistance - 1);
                }
            }
            if (Xnegative == null)world.AddToQueueCreation(WorldPositionX - 1, WorldPositionZ, RenderDistance - 1);
            else
            {
                neighbours[1] = Xnegative;
                if (!neighbours[1].wasRecentlyExpanded)
                {
                    neighbours[1].wasRecentlyExpanded = true;
                    neighbours[1].IsActive = true;
                    Xnegative.AttemptExpansion(RenderDistance - 1);
                }
            }
            if (Zpositive == null)world.AddToQueueCreation(WorldPositionX, WorldPositionZ + 1, RenderDistance - 1);
            else
            {
                neighbours[2] = Zpositive;
                if (!neighbours[2].wasRecentlyExpanded)
                {
                    neighbours[2].wasRecentlyExpanded = true;
                    neighbours[2].IsActive = true;
                    Zpositive.AttemptExpansion(RenderDistance - 1);
                }
            }
            if (Znegative == null)world.AddToQueueCreation(WorldPositionX, WorldPositionZ - 1, RenderDistance - 1);
            else
            {
                neighbours[3] = Znegative;
                if (!neighbours[3].wasRecentlyExpanded)
                {
                    neighbours[3].wasRecentlyExpanded = true;
                    neighbours[3].IsActive = true;
                    Znegative.AttemptExpansion(RenderDistance - 1);
                }
            }
        }
    }
    public void TagOffExpansion()
    {
        wasRecentlyExpanded = false;
    }
    public void UpdateChunk()
    {

        Thread myThread = new Thread(new ThreadStart(_updateChunk));
        myThread.Start();

    }
    public void _updateChunk()
    {
        threadLocked = true;
        CreateChunkMesh();
        threadLocked = false;
        lock (world.ChunksToHaveMeshAdded)
        {
            world.ChunksToHaveMeshAdded.Enqueue(this);
        }
    }
    public void AddMesh()
    {
        CreateMesh();
    }
    void CreateChunkMesh()
    {
        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
        for (int y = 0; y < Voxel.chunkHeight; y++)
        {
            for (int x = 0; x < Voxel.chunkSize; x++)
            {
                for (int z = 0; z < Voxel.chunkSize; z++)
                {
                    AddVoxelDataToChunk(new Vector3((float)x, (float)y, (float)z), voxelMap[x, y, z]);
                }
            }
        }
    }
    bool IsVoxelInChunk(int x,int y,int z)
    {
        if (x < 0 || x >= Voxel.chunkSize || y < 0 || y >= Voxel.chunkHeight || z < 0 || z >= Voxel.chunkSize) return false;
        else return true;
    }
    bool IsVoxelSolid(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x);
        int y = Mathf.FloorToInt(position.y);
        int z = Mathf.FloorToInt(position.z);
        if (!IsVoxelInChunk(x, y, z)) {
            return world.blockTypes[GetVoxel(position)].isSolid; 
        }
        return world.blockTypes[voxelMap[x, y, z]].isSolid;
    }
    void GenerateVoxelMap()
    {
        CreateHeightMapFromSeed();
        for (int y = 0; y < Voxel.chunkHeight; y++)
        {
            for (int x = 0; x < Voxel.chunkSize; x++)
            {
                for (int z = 0; z < Voxel.chunkSize; z++)
                {
                    if(y <= heightMap[x, z]-4) voxelMap[x, y, z] = 2;
                    else if(y>heightMap[x,z]-4 & y<heightMap[x,z])voxelMap[x, y, z] = 1;
                    else if(y>= heightMap[x, z]) voxelMap[x, y, z] = 0;
                }
            }
        }
    }
    void AddVoxelDataToChunk(Vector3 position,int blockId)
    {
        if (IsVoxelSolid(position))
        {
            for (int j = 0; j < 6; j++)
            {
                if (!IsVoxelSolid(position + Voxel.face[j]))
                {
                    for (int i = 0; i < 6; i++)
                    {
                        int z = i;
                        if (i == 3) z = 2;
                        if (i == 4) z = 1;
                        if (i == 5) z = 3;
                        int triangleIndex = Voxel.voxelTri[j, z];
                        vertices.Add(Voxel.voxelVerts[triangleIndex] + position + new Vector3((float)WorldPositionX * Voxel.chunkSize,0, (float)WorldPositionZ * Voxel.chunkSize));
                        triangles.Add(vertexIndex++);
                        
                    }
                    AddTexture(world.blockTypes[blockId].sideTextures[j]);
                }
            }
        }
    }
    void CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }
    void AddTexture(int textureId)
    {
        float y = textureId / Voxel.AtlasSize;
        float x = textureId - (y * Voxel.AtlasSize);

        x *= Voxel.NormalizedBlockTextureSize;
        y *= Voxel.NormalizedBlockTextureSize;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y+Voxel.NormalizedBlockTextureSize));
        uvs.Add(new Vector2(x + Voxel.NormalizedBlockTextureSize, y));
        uvs.Add(new Vector2(x + Voxel.NormalizedBlockTextureSize, y));
        uvs.Add(new Vector2(x, y + Voxel.NormalizedBlockTextureSize));
        uvs.Add(new Vector2(x + Voxel.NormalizedBlockTextureSize, y + Voxel.NormalizedBlockTextureSize));


    }
    void CreateHeightMapFromSeed()
    {
        for (int x = 0; x < Voxel.chunkSize; x++)
        {
            for (int z = 0; z < Voxel.chunkSize; z++)
            {
                heightMap[x, z] = Mathf.FloorToInt(GetBlockHeight(x + (Voxel.chunkSize * WorldPositionX), z + (Voxel.chunkSize * WorldPositionZ))) + world.GroundLevel;
            }
        }
    }
    float GetBlockHeight(int Xcoordinate,int Zcoordinate)
    {
        float sampleX = Xcoordinate / world.Xscale;
        float sampleZ = Zcoordinate / world.Zscale;
        float perlinNoise = 0;
        for (int i = 0; i < world.perlinWaves; i++)
        {
            perlinNoise = world.Yscale * Mathf.PerlinNoise(sampleX * world.Xnoise + (float)world.seed, sampleZ * world.Znoise + (float)world.seed);
        }
        perlinNoise = perlinNoise / world.perlinWaves;
        float height = world.Yscale * perlinNoise;
        return height;
    }
}
