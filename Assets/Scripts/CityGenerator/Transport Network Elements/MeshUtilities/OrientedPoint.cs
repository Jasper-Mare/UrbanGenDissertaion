using UnityEngine;

namespace CityGenerator.Transport_Network_Elements.MeshUtilities {
    // from [1]
    public struct OrientedPoint {

        public Vector3 position;
        public Quaternion rotation;

        public OrientedPoint(Vector3 position, Quaternion rotation) {
            this.position = position;
            this.rotation = rotation;
        }

        public Vector3 LocalToWorld(Vector3 point) {
            return position + rotation * point;
        }

        public Vector3 WorldToLocal(Vector3 point) {
            return Quaternion.Inverse(rotation) * (point - position);
        }

        public Vector3 LocalToWorldDirection(Vector3 dir) {
            return rotation * dir;
        }

    }

}

/*
References:
[1] J. Holmér, “A coder’s guide to spline-based procedural geometry,” 2015. 
    Available: https://docs.google.com/presentation/d/10XjxscVrm5LprOmG-VB2DltVyQ_QygD26N6XC2iap2A (Accessed 2025-10-14).

*/
