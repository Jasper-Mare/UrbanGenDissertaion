using Unity.Mathematics;
using UnityEngine;

namespace CityGenerator.MeshUtilities {
    static class MeshCreator {

        public static void CreatePlane(Mesh mesh, int xSize, int zSize)
            => CreatePlane(mesh, xSize, zSize, xSize + 1, zSize + 1);
        public static void CreatePlane(Mesh mesh, int xSize, int zSize, int numVertsX, int numVertsZ)
            => CreatePlane(mesh, xSize, zSize, numVertsX, numVertsZ, Vector3.zero);
        public static void CreatePlane(Mesh mesh, int xSize, int zSize, int numVertsX, int numVertsZ, Vector3 origin) {

            float uvScale = 1.0f / math.max(numVertsX - 1, numVertsZ - 1);
            float xScale = xSize / (float)numVertsX;
            float zScale = zSize / (float)numVertsZ;

            int vertCount = numVertsX * numVertsZ;
            int triIndexCount = (numVertsX - 1) * (numVertsZ - 1) * 6;

            Vector3[] vertices     = new Vector3[ vertCount ];
            int[] triangleIndices  = new int[ triIndexCount ];
            Vector3[] normals      = new Vector3[ vertCount ];
            Vector2[] uvs          = new Vector2[ vertCount ];
            Color[] colors         = new Color[vertices.Length];

            for (int i = 0, z = 0; z < numVertsZ; z++) {
                for (int x = 0; x < numVertsX; x++, i++) {
                    vertices[i] = new Vector3(x, 0, z) - origin;
                    normals[i] = Vector3.up;
                    uvs[i] = new Vector2((x * uvScale), (z * uvScale));

                    colors[i] = Color.black;
                }
            }

            for (int ti = 0, vi = 0, z = 0; z < numVertsZ - 1; z++, vi++) {
                for (int x = 0; x < numVertsX - 1; x++, ti += 6, vi++) {
                    triangleIndices[ti] = vi;
                    triangleIndices[ti + 3] = triangleIndices[ti + 2] = vi + 1;
                    triangleIndices[ti + 4] = triangleIndices[ti + 1] = vi + numVertsX;
                    triangleIndices[ti + 5] = vi + numVertsX + 1;
                }
            }

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangleIndices;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.colors = colors;

        }

        // based off [1]
        public static void ExtrudeMeshAlongBezier(Mesh mesh, OutlineShape outline, OrientedPoint[] path) {

            int vertsInShape = outline.points.Length;
            int segments = path.Length - 1;
            int edgeLoops = path.Length;
            int vertCount = vertsInShape * edgeLoops;
            int triCount = outline.edges.Length * segments;
            int triIndexCount = triCount * 3;
            float[] realPathLengths = new float[path.Length];

            // my code
            float pathLength = 0;
            realPathLengths[0] = 0;
            for (int i = 1; i < path.Length; i++) {
                OrientedPoint p1 = path[i - 1];
                OrientedPoint p2 = path[i];

                pathLength += (p1.position - p2.position).magnitude;
                realPathLengths[i] = pathLength;
            }
            pathLength /= outline.textureWidth;

            // tweaked from [1]
            int[] triangleIndices  = new int[ triIndexCount ];
            Vector3[] vertices     = new Vector3[ vertCount ];
            Vector3[] normals      = new Vector3[ vertCount ];
            Vector2[] uvs          = new Vector2[ vertCount ];

            // Generation code

            for (int i = 0; i < path.Length; i++) {
                int offset = i * vertsInShape;
                float v = realPathLengths[i] / outline.textureWidth;

                for (int j = 0; j < vertsInShape; j++) {
                    int id = offset + j;

                    vertices[id] = path[i].LocalToWorld(outline.points[j]);
                    normals[id] = path[i].LocalToWorldDirection(outline.normals[j]);
                    uvs[id] = new Vector2(outline.textureCoords[j], v);
                }
            }

            int ti = 0;
            for (int i = 0; i < segments; i++) {
                int offset = i * vertsInShape;
                for (int l = 0; l < outline.edges.Length; l += 2) {
                    int a = offset + outline.edges[l] + vertsInShape;
                    int b = offset + outline.edges[l];
                    int c = offset + outline.edges[l+1];
                    int d = offset + outline.edges[l+1] + vertsInShape;
                    triangleIndices[ti] = a; ti++;
                    triangleIndices[ti] = b; ti++;
                    triangleIndices[ti] = c; ti++;
                    triangleIndices[ti] = c; ti++;
                    triangleIndices[ti] = d; ti++;
                    triangleIndices[ti] = a; ti++;
                }
            }
            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangleIndices;
            mesh.normals = normals;
            mesh.uv = uvs;
        }
    }
}

/*
References:
[1] J. Holmér, “A coder’s guide to spline-based procedural geometry,” 2015. 
    Available: https://docs.google.com/presentation/d/10XjxscVrm5LprOmG-VB2DltVyQ_QygD26N6XC2iap2A (Accessed 2025-10-14).

*/
