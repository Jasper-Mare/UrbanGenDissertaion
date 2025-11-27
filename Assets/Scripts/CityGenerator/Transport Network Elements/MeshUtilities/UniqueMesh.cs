using UnityEngine;

namespace CityGenerator.TransportNetworkElements.MeshUtilities {

    // [1]
    public class UniqueMesh : MonoBehaviour {

        [HideInInspector] int ownerID; // To ensure they have a unique mesh
        MeshFilter _mf;
        MeshFilter mf { // Tries to find a mesh filter, adds one if it doesn't exist yet
            get {

                if (_mf == null) {
                    _mf = GetComponent<MeshFilter>();

                    if (_mf == null) {
                        _mf = gameObject.AddComponent<MeshFilter>();
                    }
                }

                return _mf;
            }
        }

        Mesh _mesh;
        protected Mesh mesh { // The mesh to edit
            get {
                bool isOwner = (ownerID == gameObject.GetInstanceID());
                if (mf.sharedMesh == null || !isOwner) {
                    mf.sharedMesh = _mesh = new Mesh();
                    ownerID = gameObject.GetInstanceID();
                    _mesh.name = "Mesh [" + ownerID + "]";
                }
                return _mesh;
            }
        }

    }
}

/*
References:
[1] J. Holmér, “A coder’s guide to spline-based procedural geometry,” 2015. 
    Available: https://docs.google.com/presentation/d/10XjxscVrm5LprOmG-VB2DltVyQ_QygD26N6XC2iap2A (Accessed 2025-10-14).



*/
