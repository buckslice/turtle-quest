using UnityEngine;
using System.Collections.Generic;

// point in terrain that has a position and color
class TerrainPoint {
    public Vector3 pos;
    public Color32 col;
    public TerrainPoint(Vector3 pos, Color32 col) {
        this.pos = pos;
        this.col = col;
    }
}

// simple 2D int vector that can be stored in dictionary
class Vec2 {
    public int x;
    public int y;
    public Vec2(int x, int y) {
        this.x = x;
        this.y = y;
    }
    public override bool Equals(object o) {
        Vec2 v = (Vec2)o;
        return x == v.x && y == v.y;
    }
    public override int GetHashCode() {
        return (x << 16) | y;
    }
}

public class WorldGenerator : MonoBehaviour {
    public int chunkLoadRadius = 8;
    public int newChunksPerFrame = 1;
    [Tooltip("Tiles per chunk")]
    public int chunkSize = 16;
    [Tooltip("Scale of each tile in chunk")]
    public float tileScale = 1.0f;
    // private variables set on start to prevent changes during runtime
    int GRID_SIZE;
    float GRID_SCALE;
    float CHUNK_SCALE;

    public bool randomSeed = false;
    public int seed = 1000;
    System.Random rng;

    public Gradient grad;
    public Material mat;

    public GameObject rockPrefab;
    public GameObject kelpPrefab;
    public GameObject[] creatures;

    Rigidbody playerRigid;
    float playerStartHeight;

    Dictionary<Vec2, GameObject> chunkMap = new Dictionary<Vec2, GameObject>();
    List<Vec2> newChunks = new List<Vec2>();
    List<float> distances = new List<float>();
    List<Vec2> toRemove = new List<Vec2>();

    // Use this for initialization
    void Start() {
        GameObject playerGO = Camera.main.transform.root.gameObject;
        if (playerGO) {
            playerRigid = playerGO.GetComponent<Rigidbody>();
            playerStartHeight = playerRigid.position.y;
        }

        InitGenerator();
    }

    // Update is called once per frame
    void Update() {
        Vector3 p = playerRigid.position;
        Vec2 pc = WorldToChunk(p.x, p.z);

        if (Input.GetKeyDown(KeyCode.R)) {
            // clear all current chunks
            foreach (var key in chunkMap.Keys) {
                Destroy(chunkMap[key]);
            }
            chunkMap.Clear();

            InitGenerator();
            // move player up so doesnt fall through ground (could handle this better)
            playerRigid.position = new Vector3(p.x, playerStartHeight, p.z);
            playerRigid.isKinematic = false;
            playerRigid.velocity = Vector3.down * 0.1f;
        }

        float loadDist = chunkLoadRadius * CHUNK_SCALE;

        newChunks.Clear();
        distances.Clear();
        // generate chunks around the player
        for (int x = pc.x - chunkLoadRadius; x <= pc.x + chunkLoadRadius; ++x) {
            for (int y = pc.y - chunkLoadRadius; y <= pc.y + chunkLoadRadius; ++y) {
                Vec2 cc = new Vec2(x, y);
                if (chunkMap.ContainsKey(cc)) {
                    continue;
                }

                float cx = cc.x * CHUNK_SCALE;
                float cy = cc.y * CHUNK_SCALE;
                float sqrDist = (p.x - cx) * (p.x - cx) + (p.z - cy) * (p.z - cy);

                if (sqrDist > loadDist * loadDist) {
                    continue;
                }

                // insertion sort
                int len = distances.Count;
                if (len < newChunksPerFrame) {
                    newChunks.Add(cc);
                    distances.Add(sqrDist);
                } else {
                    for (int i = 0; i < len; ++i) {
                        if (sqrDist < distances[i]) {
                            newChunks.Insert(i, cc);
                            distances.Insert(i, sqrDist);
                            newChunks.RemoveAt(len);
                            distances.RemoveAt(len);
                            break;
                        }
                    }

                }
            }
        }

        // build new chunks and add to map
        for (int i = 0; i < newChunks.Count; ++i) {
            Vec2 cc = newChunks[i];
            chunkMap[cc] = BuildChunk(cc);
        }

        // remove chunks that are too far away
        toRemove.Clear();
        foreach (var cc in chunkMap.Keys) {
            float cx = cc.x * CHUNK_SCALE;
            float cy = cc.y * CHUNK_SCALE;
            float sqrDist = (p.x - cx) * (p.x - cx) + (p.z - cy) * (p.z - cy);
            if (sqrDist > loadDist * loadDist + 1.0f) {
                toRemove.Add(cc);
            }
        }
        foreach (Vec2 v in toRemove) {
            Destroy(chunkMap[v].gameObject);
            chunkMap.Remove(v);
        }

    }

    // generate random offset vectors based on seed
    Vector3 offset1;
    Vector3 offset2;
    void InitGenerator() {
        // initialize seed
        if (randomSeed) {
            seed = Random.Range(-100000, 100000);
        }
        rng = new System.Random(seed);
        offset1 = Noise.NextRandomOffset(rng);
        offset2 = Noise.NextRandomOffset(rng);

        // init variables here
        GRID_SIZE = chunkSize;
        GRID_SCALE = tileScale;
        CHUNK_SCALE = GRID_SIZE * GRID_SCALE;
    }

    Vec2 WorldToChunk(float x, float z) {
        int px = (int)((x - CHUNK_SCALE / 2.0f * (x > 0.0f ? -1.0f : 1.0f)) / CHUNK_SCALE);
        int pz = (int)((z - CHUNK_SCALE / 2.0f * (z > 0.0f ? -1.0f : 1.0f)) / CHUNK_SCALE);
        return new Vec2(px, pz);
    }

    GameObject BuildChunk(Vec2 coord) {
        Vector3[] verts = new Vector3[(GRID_SIZE + 3) * (GRID_SIZE + 3)];
        int[] tris = new int[(GRID_SIZE + 2) * (GRID_SIZE + 2) * 6];
        Color32[] colors = new Color32[verts.Length];
        //Vector2[] uvs = new Vector2[verts.Length];
        GameObject go = new GameObject(string.Format("Chunk ({0},{1})", coord.x, coord.y));

        // generate vertices and colors
        List<Vector3> kelpPoints = new List<Vector3>();

        for (int i = 0, y = -1; y <= GRID_SIZE + 1; ++y) {
            for (int x = -1; x <= GRID_SIZE + 1; ++x, ++i) {
                float xf = (x + coord.x * GRID_SIZE - GRID_SIZE / 2.0f) * GRID_SCALE;
                float yf = (y + coord.y * GRID_SIZE - GRID_SIZE / 2.0f) * GRID_SCALE;
                TerrainPoint tp = GeneratePoint(xf, yf);
                verts[i] = tp.pos;
                colors[i] = tp.col;

                if (Noise.Billow(new Vector3(xf, yf, 0), 3, 0.02f) > 0.0f) {
                    kelpPoints.Add(tp.pos);
                }

                if (Random.value < 0.015f) {
                    GameObject pre = Instantiate(rockPrefab, tp.pos, Random.rotation, go.transform);

                    MeshData data = MeshGenerator.GenerateIcosphere(1);
                    for (int j = 0; j < data.vertices.Length; ++j) {
                        data.vertices[j] *= 1.0f + Noise.Ridged(data.vertices[j], 3, Random.Range(0.3f, 0.8f)) * 0.3f;
                    }
                    pre.GetComponent<MeshFilter>().sharedMesh = data.CreateMesh();

                    pre.transform.localScale = Vector3.one * (1 + Random.value * 2);
                }

                if (Random.value < 0.005f) {
                    GameObject creature = Instantiate(creatures[Random.Range(0, creatures.Length)], tp.pos + Vector3.up * (2.0f + Random.value * 10.0f), Quaternion.identity, go.transform);
                }
            }
        }

        Mesh kelps = MeshGenerator.GenerateStrips(kelpPoints);
        GameObject kelpGo = Instantiate(kelpPrefab, Vector3.zero, Quaternion.identity, go.transform);
        kelpGo.GetComponent<MeshFilter>().sharedMesh = kelps;

        // generate triangles
        GenerateTriangles(tris, GRID_SIZE + 2);

        // generate the normals
        Vector3[] normals = new Vector3[verts.Length];
        for (int i = 0; i < tris.Length / 3; ++i) {
            int a = tris[i * 3];
            int b = tris[i * 3 + 1];
            int c = tris[i * 3 + 2];

            Vector3 va = verts[a];
            Vector3 vb = verts[b];
            Vector3 vc = verts[c];

            Vector3 norm = Vector3.Cross(vb - va, vc - va);
            normals[a] += norm;
            normals[b] += norm;
            normals[c] += norm;
        }
        for (int i = 0; i < normals.Length; ++i) {
            normals[i].Normalize();
        }

        // now slice off extra vert layer
        Vector3[] vs = new Vector3[(GRID_SIZE + 1) * (GRID_SIZE + 1)];
        Color32[] cs = new Color32[vs.Length];
        Vector3[] ns = new Vector3[vs.Length];
        int[] ts = new int[GRID_SIZE * GRID_SIZE * 6];
        for (int y = 0; y < GRID_SIZE + 1; ++y) {
            for (int x = 0; x < GRID_SIZE + 1; ++x) {
                int oi = x + y * (GRID_SIZE + 1);
                int ci = (x + 1) + (y + 1) * (GRID_SIZE + 3);
                vs[oi] = verts[ci];
                cs[oi] = colors[ci];
                ns[oi] = normals[ci];
            }
        }

        GenerateTriangles(ts, GRID_SIZE);

        Mesh m = new Mesh();
        m.vertices = vs;
        m.triangles = ts;
        m.colors32 = cs;
        m.normals = ns;

        go.transform.parent = transform;

        // build mesh collider from the shared mesh
        go.AddComponent<MeshCollider>().sharedMesh = m;

        m.RecalculateBounds();
        //m.RecalculateNormals();

        go.AddComponent<MeshFilter>().mesh = m;
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.material = mat;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;

        return go;
    }

    void GenerateTriangles(int[] tris, int s) {
        for (int y = 0; y < s; ++y) {
            for (int x = 0; x < s; ++x) {
                int i = (x + y * s) * 6;
                int a = x + y * (s + 1);
                int b = x + (y + 1) * (s + 1);
                // swap every other one for diamond pattern
                if ((x + y) % 2 == 0) {
                    tris[i] = a;
                    tris[i + 1] = b;
                    tris[i + 2] = b + 1;
                    tris[i + 3] = b + 1;
                    tris[i + 4] = a + 1;
                    tris[i + 5] = a;
                } else {
                    tris[i] = b;
                    tris[i + 1] = b + 1;
                    tris[i + 2] = a + 1;
                    tris[i + 3] = a + 1;
                    tris[i + 4] = a;
                    tris[i + 5] = b;
                }
            }
        }
    }

    // main generation function called on to generate each point in the mesh
    // experiment with noise here!
    TerrainPoint GeneratePoint(float x, float y) {
        Vector3 position = new Vector3(x, 0.0f, y);

        float mountains = Noise.Ridged(position + offset1, 6, 0.0075f);
        mountains *= .5f;
        mountains += .5f;
        //float hills = Noise.Fractal(position + offset1, 5, 0.005f);
        float hills = Noise.Billow(position + offset1, 5, 0.005f);
        hills *= .25f;
        hills -= .25f;

        float blendNoise = Noise.Fractal(position + offset2, 5, 0.003f);
        float total = Noise.Blend(mountains, hills, blendNoise, -0.5f, 0.5f);

        total = (total + 1) / 2.0f;
        //Color c = grad.Evaluate(total);
        total = total * 0.8f;
        //float colnoise = (Noise.Billow(position, 4, 0.02f) + 1.0f) / 2.0f;
        //Color c = new Color(0.2f, 0.1f, colnoise);
        //c = new Color(.6f, .6f, .1f + colnoise / 3.0f);
        Color c = new Color(1, 1, 1);
        return new TerrainPoint(new Vector3(x, total * 60.0f, y), c);

        //float xf = x * 0.04f + offset1.x;
        //float yf = y * 0.04f + offset1.y;
        //WorleySample ws = Noise.Worley3(xf, yf, 0, 2, DistanceFunction.EUCLIDIAN);
        //float h = 1.0f - (float)ws.F[0];

        //Vector3 pos = new Vector3(x, h * 25.0f, y);
        //return new TerrainPoint(pos, grad.Evaluate(h));
    }

}
