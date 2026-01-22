using CityGenerator.MeshUtilities;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(UniqueMesh))]
public class StreetGraphTestScript : MonoBehaviour {

    [SerializeField]
    bool updateVisualisation = false;

    void Start() {

    }

    void Update() {

        if (updateVisualisation) {


            updateVisualisation = false;
        }

    }

}

