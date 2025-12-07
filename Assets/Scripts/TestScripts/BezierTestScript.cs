using CityGenerator.MeshUtilities;
using System.Linq;
using UnityEngine;

public class TestScript : MonoBehaviour {
    [SerializeField]
    UniqueMesh MeshObject;

    [SerializeField]
    Transform[] splinePoints;

    [SerializeField]
    float sampleStep = 0.1f;

    [SerializeField]
    bool doUpdate = false;

    OutlineShape outline = new OutlineShape(
        new Vector2[] { new(1, 0), new(-1, 0) },
        new Vector2[] { new(0, 1), new(0, 1) },
        new float[] { 0.0f, 1.0f  },
        new int[] { 0, 1  }
    );

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        Debug.Log("hello world");
        updateMesh();
    }

    void updateMesh() {
        if (MeshObject == null || splinePoints == null || splinePoints.Length != 4) {
            return;
        }

        BezierCurve curve = new BezierCurve( splinePoints.Select(t => t.position).ToArray() );
        OrientedPoint[] points = curve.SampleOrientedPoints(sampleStep);

        BezierMesher.ExtrudeMesh(MeshObject.Mesh, outline, points);

    }

    // Update is called once per frame
    void Update() {
        if (doUpdate) {
            updateMesh();
            doUpdate = false;
        }
    }
}
