using UnityEngine;
using System.Collections.Generic;

public static class MeshGenerator {

    struct TriangleIndices {
        public int v1;
        public int v2;
        public int v3;

        public TriangleIndices(int v1, int v2, int v3) {
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
        }
    }

    static readonly float t = (1f + Mathf.Sqrt(5f)) / 2f;
    static readonly Vector3[] goldenVectors = new Vector3[] {
        new Vector3(-1, +t, 0).normalized,
        new Vector3(+1, +t, 0).normalized,
        new Vector3(-1, - t, 0).normalized,
        new Vector3(+1, -t, 0).normalized,

        new Vector3(0, -1, +t).normalized,
        new Vector3(0, +1, +t).normalized,
        new Vector3(0, -1, -t).normalized,
        new Vector3(0, +1, -t).normalized,

        new Vector3(+t, 0, -1).normalized,
        new Vector3(+t, 0, +1).normalized,
        new Vector3(-t, 0, -1).normalized,
        new Vector3(-t, 0, +1).normalized
    };

    static int index = 0;
    static List<Vector3> vertices = new List<Vector3>();
    static List<int> triangles = new List<int>();
    static Dictionary<long, int> table = new Dictionary<long, int>();
    // generates or returns the vertices and indices of a unit icosphere
    public static MeshData GenerateIcosphere(int rec) {
        if (rec > 9) {
            Debug.Log("Setting recursion level to maximum of 9");
            rec = 9;
        }
        if (rec < 0) {
            Debug.Log("Setting recursion level to minimum of 0");
            rec = 0;
        }

        // initialize variables
        index = 0;
        table.Clear();
        vertices.Clear();
        triangles.Clear();
        List<TriangleIndices> faces = new List<TriangleIndices>();

        // 12 starting points
        for (int i = 0; i < 12; ++i) {
            vertices.Add(goldenVectors[i]);
            ++index;
        }

        // 20 faces
        faces.Add(new TriangleIndices(0, 11, 5));
        faces.Add(new TriangleIndices(0, 5, 1));
        faces.Add(new TriangleIndices(0, 1, 7));
        faces.Add(new TriangleIndices(0, 7, 10));
        faces.Add(new TriangleIndices(0, 10, 11));

        faces.Add(new TriangleIndices(1, 5, 9));
        faces.Add(new TriangleIndices(5, 11, 4));
        faces.Add(new TriangleIndices(11, 10, 2));
        faces.Add(new TriangleIndices(10, 7, 6));
        faces.Add(new TriangleIndices(7, 1, 8));

        faces.Add(new TriangleIndices(3, 9, 4));
        faces.Add(new TriangleIndices(3, 4, 2));
        faces.Add(new TriangleIndices(3, 2, 6));
        faces.Add(new TriangleIndices(3, 6, 8));
        faces.Add(new TriangleIndices(3, 8, 9));

        faces.Add(new TriangleIndices(4, 9, 5));
        faces.Add(new TriangleIndices(2, 4, 11));
        faces.Add(new TriangleIndices(6, 2, 10));
        faces.Add(new TriangleIndices(8, 6, 7));
        faces.Add(new TriangleIndices(9, 8, 1));

        for (int i = 0; i < rec; ++i) {
            List<TriangleIndices> faces2 = new List<TriangleIndices>();

            // speeds generation up to clear it each iteration since reduces hash collisions
            table.Clear();
            foreach (TriangleIndices tri in faces) {
                int a = GetMidpoint(tri.v1, tri.v2);
                int b = GetMidpoint(tri.v2, tri.v3);
                int c = GetMidpoint(tri.v3, tri.v1);

                faces2.Add(new TriangleIndices(tri.v1, a, c));
                faces2.Add(new TriangleIndices(a, tri.v2, b));
                faces2.Add(new TriangleIndices(c, b, tri.v3));
                // make new triangle upside down and backwards to simplify neighbor calculations
                // this way the triangle is either flipped or not flipped versus having 3 different rotations
                faces2.Add(new TriangleIndices(b, c, a));
            }
            faces = faces2;
        }

        for (int i = 0; i < faces.Count; ++i) {
            triangles.Add(faces[i].v1);
            triangles.Add(faces[i].v2);
            triangles.Add(faces[i].v3);
        }
        //foreach (TriangleIndices tri in faces) {
        //    triangles.Add(tri.v1);
        //    triangles.Add(tri.v2);
        //    triangles.Add(tri.v3);
        //}

        return new MeshData(vertices, triangles);
    }

    private static int GetMidpoint(int p1, int p2) {
        //generate key from pair of indices
        bool firstIsSmaller = p1 < p2;

        // hash 2 ints into long
        long smallerIndex = firstIsSmaller ? p1 : p2;
        long greaterIndex = firstIsSmaller ? p2 : p1;
        long key = (smallerIndex << 32) + greaterIndex;

        int ret;
        if (table.TryGetValue(key, out ret)) {
            // once we find vertex take it out of dictionary to reduce hash collisions
            table.Remove(key);
            return ret;
        }

        vertices.Add(((vertices[p1] + vertices[p2]) * 0.5f).normalized);
        table[key] = index;
        return index++;
    }

    public static MeshData GenerateSquarePyramid() {
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();

        Vector3 a = new Vector3(0.0f, 0.0f, 1.0f);
        Vector3 b = new Vector3(1.0f, 1.0f, -1.0f);
        Vector3 c = new Vector3(1.0f, -1.0f, -1.0f);
        Vector3 d = new Vector3(-1.0f, -1.0f, -1.0f);
        Vector3 e = new Vector3(-1.0f, 1.0f, -1.0f);

        verts.Add(a);
        verts.Add(b);
        verts.Add(e);

        verts.Add(a);
        verts.Add(c);
        verts.Add(b);

        verts.Add(a);
        verts.Add(d);
        verts.Add(c);

        verts.Add(a);
        verts.Add(e);
        verts.Add(d);

        verts.Add(b);
        verts.Add(c);
        verts.Add(d);

        verts.Add(d);
        verts.Add(e);
        verts.Add(b);

        for (int i = 0; i < 18; i++) {
            tris.Add(i);
        }

        return new MeshData(verts, tris);
    }

    public static MeshData GenerateTriangularPyramid() {
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();

        float baseEdgeLength = 1.0f;
        float height = 2.0f;

        float hw = baseEdgeLength / 2.0f;
        float fx = (float)0.57735026919 * hw;   // distance from center of edge to center of equilateral
        float fy = (float)1.15470053838 * hw;   // distance from any point to center of equilateral
        Vector3 w = new Vector3();

        Vector3 l = w - Vector3.right * hw - Vector3.forward * fx;
        Vector3 r = w + Vector3.right * hw - Vector3.forward * fx;
        Vector3 u = w + Vector3.up * height;
        Vector3 f = w + Vector3.forward * fy;

        verts.Add(l);
        verts.Add(u);
        verts.Add(r);

        verts.Add(f);
        verts.Add(u);
        verts.Add(l);

        verts.Add(r);
        verts.Add(u);
        verts.Add(f);

        verts.Add(l);
        verts.Add(r);
        verts.Add(f);

        for (int i = 0; i < 12; i++) {
            tris.Add(i);
        }

        return new MeshData(verts, tris);
    }

    public static Mesh GenerateStrips(List<Vector3> points) {
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        int tri = 0;
        for (int p = 0; p < points.Count; ++p) {
            int pieces = Random.Range(10, 20);
            for (int i = 0; i < pieces + 1; ++i) {
                Quaternion q = Random.rotation;
                float ww = 1.0f - i / pieces;
                Vector3 aa = new Vector3(Random.Range(-0.8f, -0.4f) * ww, 0, 0);
                Vector3 bb = new Vector3(Random.Range(0.4f, 0.8f) * ww, 0, 0);
                Vector3 aq = q * aa;
                Vector3 bq = q * bb;
                aq.y = i;
                bq.y = i;
                verts.Add(points[p] + aq);
                verts.Add(points[p] + bq);
                uvs.Add(new Vector2(0, (float)i / pieces));
                uvs.Add(new Vector2(1, (float)i / pieces));

                //verts.Add(points[p] + new Vector3(Random.Range(-0.5f, -0.2f), i, Random.Range(-0.5f, -0.2f)));
                //verts.Add(points[p] + new Vector3(Random.Range(0.2f, 0.5f), i, Random.Range(0.2f, 0.5f)));
            }
            for (int i = 0; i < pieces; ++i) {
                tris.Add(tri);
                tris.Add(tri + 2);
                tris.Add(tri + 1);
                tris.Add(tri + 1);
                tris.Add(tri + 2);
                tris.Add(tri + 3);
                tri += 2;
            }
            tri += 2;
        }
        Mesh m = new Mesh();
        m.vertices = verts.ToArray();
        m.uv = uvs.ToArray();
        m.triangles = tris.ToArray();

        return m;

    }

}

public class MeshData {
    public Vector3[] vertices;
    public int[] triangles;
    int triangleIndex;

    public MeshData(List<Vector3> vertices, List<int> triangles) {
        this.vertices = vertices.ToArray();
        this.triangles = triangles.ToArray();
    }

    public Mesh CreateMesh() {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    public void ConvertToFladShaded() {
        if (vertices.Length == triangles.Length) {
            Debug.LogWarning("Trying to convert a mesh that already has no sharing!");
            return;
        }

        Vector3[] newVerts = new Vector3[triangles.Length];
        for (int i = 0; i < triangles.Length; ++i) {
            newVerts[i] = vertices[triangles[i]];
            triangles[i] = i;
        }

        vertices = newVerts;
    }
}