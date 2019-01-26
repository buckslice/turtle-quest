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
    int size;
    float scale;
    float chunkScale;

    public bool flatShaded = false;

    public bool randomSeed = false;
    public int seed = 1000;
    System.Random rng;

    public Gradient grad;
    public Material mat;
    public Material fadeMat;

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

        float loadDist = chunkLoadRadius * chunkScale;

        newChunks.Clear();
        distances.Clear();
        // generate chunks around the player
        for (int x = pc.x - chunkLoadRadius; x <= pc.x + chunkLoadRadius; ++x) {
            for (int y = pc.y - chunkLoadRadius; y <= pc.y + chunkLoadRadius; ++y) {
                Vec2 cc = new Vec2(x, y);
                if (chunkMap.ContainsKey(cc)) {
                    continue;
                }

                float cx = cc.x * chunkScale;
                float cy = cc.y * chunkScale;
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
            float cx = cc.x * chunkScale;
            float cy = cc.y * chunkScale;
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
        size = chunkSize;
        scale = tileScale;
        chunkScale = size * scale;
    }

    Vec2 WorldToChunk(float x, float z) {
        int px = (int)((x - chunkScale / 2.0f * (x > 0.0f ? -1.0f : 1.0f)) / chunkScale);
        int pz = (int)((z - chunkScale / 2.0f * (z > 0.0f ? -1.0f : 1.0f)) / chunkScale);
        return new Vec2(px, pz);
    }

    GameObject BuildChunk(Vec2 coord) {
        Vector3[] verts = new Vector3[(size + 1) * (size + 1)];
        int[] tris = new int[size * size * 6];
        Color32[] colors = new Color32[verts.Length];
        //Vector2[] uvs = new Vector2[verts.Length];

        // generate vertices and colors
        for (int i = 0, y = 0; y <= size; ++y) {
            for (int x = 0; x <= size; ++x, ++i) {
                float xf = (x + coord.x * size - size / 2.0f) * scale;
                float yf = (y + coord.y * size - size / 2.0f) * scale;
                TerrainPoint tp = GeneratePoint(xf, yf);
                verts[i] = tp.pos;
                colors[i] = tp.col;
            }
        }
        // generate triangles
        bool mode = true;
        for (int t = 0, i = 0; i < (size + 1) * size; ++i) {
            if (i % (size + 1) == size) {
                continue;
            }
            if (mode) { // swap edge diagonal
                tris[t++] = i;
                tris[t++] = i + size + 1;
                tris[t++] = i + size + 2;
                tris[t++] = i + size + 2;
                tris[t++] = i + 1;
                tris[t++] = i;
            } else {
                tris[t++] = i;
                tris[t++] = i + size + 1;
                tris[t++] = i + 1;
                tris[t++] = i + 1;
                tris[t++] = i + size + 1;
                tris[t++] = i + size + 2;
            }
            mode = !mode;
        }

        Mesh m = new Mesh();
        m.vertices = verts;
        m.triangles = tris;
        m.colors32 = colors;

        GameObject go = new GameObject(string.Format("Chunk ({0},{1})", coord.x, coord.y));
        go.transform.parent = transform;

        // build mesh collider from the shared mesh
        go.AddComponent<MeshCollider>().sharedMesh = m;

        // if flatshading there can be no vertex sharing
        // so split vertices and color array
        if (flatShaded) {
            Vector3[] newVerts = new Vector3[tris.Length];
            Color32[] newColors = new Color32[tris.Length];

            for (int i = 0; i < tris.Length; ++i) {
                int t = tris[i];
                newVerts[i] = verts[t];
                newColors[i] = colors[t];
                tris[i] = i;
            }

            // average colors
            int tc = 3; // change to 6 if you want to average color per square
            for (int i = 0; i < tris.Length; i += tc) {
                Color sum = new Color();
                for (int j = 0; j < tc; ++j) {
                    sum += newColors[i + j];
                }
                sum /= tc;
                for (int j = 0; j < tc; ++j) {
                    newColors[i + j] = sum;
                }
            }

            m = new Mesh();
            m.vertices = newVerts;
            m.triangles = tris;
            m.colors32 = newColors;
        }

        m.RecalculateBounds();
        m.RecalculateNormals();

        go.AddComponent<MeshFilter>().mesh = m;
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.material = mat;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;

        return go;
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
        total = total * 0.4f;
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
