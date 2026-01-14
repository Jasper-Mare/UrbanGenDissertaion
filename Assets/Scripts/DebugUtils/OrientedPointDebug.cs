using CityGenerator.MeshUtilities;
using UnityEngine;

public class OrientedPointDebug : MonoBehaviour {
    public OrientedPoint point;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        transform.position = point.position;
        transform.rotation = point.rotation;
    }

    // Update is called once per frame
    void Update() {
        point.position = transform.position;
        point.rotation = transform.rotation;

        Debug.DrawRay(point.position, point.LocalToWorldDirection(Vector3.right), Color.orange);
        Debug.DrawRay(point.position, Vector3.up);
    }
}
