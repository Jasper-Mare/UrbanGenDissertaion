using UnityEngine;

namespace CityGenerator.MeshUtilities {
    // my code based on [1]
    [CreateAssetMenu(fileName = "NewOutline", menuName = "CityGenerator/OutlineShape", order = 2)]
    public class OutlineShape : ScriptableObject {
        public Vector2[] points;
        public Vector2[] normals;
        public float[] textureCoords;
        public int[] edges;
        public float textureWidth;

        public OutlineShape(Vector2[] points, Vector2[] pointNormals, float[] textureUs, int[] edgePairs) {
            Setup(points, pointNormals, textureUs, edgePairs);
        }

        public void Setup(Vector2[] points, Vector2[] pointNormals, float[] textureUs, int[] edgePairs) {
            this.points = points;
            normals = pointNormals;
            textureCoords = textureUs;
            edges = edgePairs;
            textureWidth = 0;

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
