using UnityEngine;

namespace CityGenerator.Transport_Network_Elements.MeshUtilities {
    // based on [1]
    public struct OutlineShape {
        public readonly Vector2[] points;
        public readonly Vector2[] normals;
        public readonly float[] textureCoords;
        public readonly int[] edges;
    }

}

/*
References:
[1] J. Holmér, “A coder’s guide to spline-based procedural geometry,” 2015. 
    Available: https://docs.google.com/presentation/d/10XjxscVrm5LprOmG-VB2DltVyQ_QygD26N6XC2iap2A (Accessed 2025-10-14).

*/
