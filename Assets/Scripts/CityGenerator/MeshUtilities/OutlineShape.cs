using UnityEngine;

namespace CityGenerator.MeshUtilities {
    // my code based on [1]
    public struct OutlineShape {
        public readonly Vector2[] points;
        public readonly Vector2[] normals;
        public readonly float[] textureCoords;
        public readonly int[] edges;
        public readonly float textureWidth;

        public OutlineShape(Vector2[] points, Vector2[] pointNormals, float[] textureUs, int[] edgePairs) {
            this.points = points;
            normals = pointNormals;
            textureCoords = textureUs;
            edges = edgePairs;
            this.textureWidth = 0;

            for (int i = 0; i < edgePairs.Length; i += 2) {
                int p1 = edgePairs[i];
                int p2 = edgePairs[i + 1];

                textureWidth += (points[p1] - points[p2]).magnitude;
            }

        }
    }

}

/*
References:
[1] J. Holmér, “A coder’s guide to spline-based procedural geometry,” 2015. 
    Available: https://docs.google.com/presentation/d/10XjxscVrm5LprOmG-VB2DltVyQ_QygD26N6XC2iap2A (Accessed 2025-10-14).

*/
