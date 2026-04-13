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

    System.Collections.Generic.List<GameObject> debugs = new();

    OutlineShape outline = new OutlineShape(
        new Vector2[] {
            new(1, 0.1f), new(-1, 0.1f), // top edge
            new(-1, 0.1f), new(-1, -0.3f), new(-0.75f, -0.5f), // left edge
            new(-0.75f, -0.5f), new(0.75f, -0.5f), // bottom edge
            new(0.75f, -0.5f), new(1, -0.3f), new(1, 0.1f), // right edge
        },
        new Vector2[] {
            new(0, 1), new(0, 1), // top edge
            new(-1, 0), new(-0.89f, -0.45f), new(-0.45f, -0.89f), // left edge
            new(0, -1), new(0, -1), // bottom edge
            new(0.45f, -0.89f), new(0.89f, -0.45f), new(1, 0), // right edge
        },
        new float[] {
            0.0f, 0.5f, // top edge (road)
            0.5f, 0.57f, 0.62f, // left edge (concrete)
            0.62f, 0.88f, // bottom edge (concrete)
            0.88f, 0.93f, 1.0f // right edge (concrete)
        },
        new int[] {
            0,1, // top
            2,3, 3,4, // left
            5,6, // bottom
            7,8, 8,9 // right
        }
        );

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        updateMesh();
    }

    void updateMesh() {
        if (MeshObject == null || splinePoints == null || splinePoints.Length != 4) {
            return;
        }

        BezierCurve curve = new BezierCurve( splinePoints.Select(t => t.position).ToArray() );
        OrientedPoint[] points = curve.SampleOrientedPoints(sampleStep);

        // delete old debugs
        foreach (GameObject objDebug in debugs) {
            Destroy(objDebug);
        }
        debugs.Clear();

        // debug oriented points
        foreach (OrientedPoint pt in points) {
            GameObject objDebug = new GameObject($"debug{pt.GetHashCode()}");
            objDebug.transform.parent = this.transform;
            var debug = objDebug.AddComponent<OrientedPointDebug>();
            debug.point = pt;
            debugs.Add(objDebug);
        }

        MeshCreator.ExtrudeMeshAlongOrientedPath(MeshObject.Mesh, outline, points);

    }

    // Update is called once per frame
    void Update() {
        if (doUpdate) {
            updateMesh();
            doUpdate = false;
        }
    }
}
