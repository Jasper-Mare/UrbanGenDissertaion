using UnityEngine;

namespace CityGenerator.MeshUtilities {

    public static class BezierMesher {

        // based off [1]
        public static void ExtrudeMesh(Mesh mesh, OutlineShape outline, OrientedPoint[] path) {

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
