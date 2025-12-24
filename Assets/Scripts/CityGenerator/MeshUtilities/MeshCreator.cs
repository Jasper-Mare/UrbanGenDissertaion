using Unity.Mathematics;
using UnityEngine;

namespace CityGenerator.MeshUtilities {
    static class MeshCreator {

        public static void CreatePlane(Mesh mesh, int xSize, int zSize)
            => CreatePlane(mesh, xSize, zSize, xSize + 1, zSize + 1);
        public static void CreatePlane(Mesh mesh, int xSize, int zSize, int numVertsX, int numVertsZ)
            => CreatePlane(mesh, xSize, zSize, numVertsX, numVertsZ, Vector3.zero);
        public static void CreatePlane(Mesh mesh, int xSize, int zSize, int numVertsX, int numVertsZ, Vector3 origin) {

            float uvScale = 1.0f / math.max(numVertsX, numVertsZ);
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
    }
}
