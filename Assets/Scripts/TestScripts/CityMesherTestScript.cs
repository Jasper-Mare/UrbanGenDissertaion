using CityGenerator.FlowFields;
using CityGenerator.MeshUtilities;
using CityGenerator.StreetGraph;
using CityGenerator.Templates;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(UniqueMesh))]
public class CityMesherTestScript : MonoBehaviour {

    [SerializeField]
    bool shouldRegenerate = false;
    [SerializeField]
    uint generatorSeed = 0;

    private Mesh mesh;
    private Renderer rend;
    private Material visMatInstance;
    private HyperStreamlineGenerator streamlineGenerator;
    private CityMeshGenerator meshGenerator;

    [Header("Tensorfield Generator Properties")]
    [SerializeField]
    float2 size = new float2(100, 100);
    [SerializeField]
    int2 numberOfTensors = new int2(100, 100);
    [SerializeField]
    int numIterations = 4;
    [SerializeField]
    float decayConstant = 0.01f;


    [Header("Streamline Generator Properties")]
    [SerializeField]
    float maxLength = 50;
    [SerializeField]
    float minSeperation = 1;
    [SerializeField]
    float lookAheadDist = 5;
    [SerializeField]
    float seedDensityCount = 1f;
    [SerializeField]
    float seedDensityLength = 1f;
    [SerializeField]
    [Range(0, 1)]
    float bridgeProportion = 0.1f;

    [Header("Mesh Generator Properties")]
    [SerializeField]
    Material templateMaterial;

    float2 position = float2.zero;

    NetworkElementTemplate template = new NetworkElementTemplate();

    float seedDensity {
        get {
            return seedDensityCount / seedDensityLength;
        }
    }

    void Start() {
        mesh = GetComponent<UniqueMesh>().Mesh;
        rend = GetComponent<Renderer>();
        rend.material = (visMatInstance = rend.material);

        Vector3 pos3d = transform.position;
        position = new float2(pos3d.x, pos3d.z);

        template.maximumSteepness = 1f;
        template.minimumIntersectionRadius = 3f;
        template.bridgingHeight = 3f;
        template.outline = new OutlineShape(
            new Vector2[] { new(1, 0), new(-1, 0) },
            new Vector2[] { new(0, 1), new(0, 1) },
            new float[] { 0.0f, 1.0f },
            new int[] { 0, 1 }
        );
        template.roadMaterial = templateMaterial;

        shouldRegenerate = true;
    }

    void Update() {

        if (shouldRegenerate) {
            StartCoroutine(Generate());

            shouldRegenerate = false;
        }

        if (streamlineGenerator is not null) {
            foreach (HyperStreamline streamline in streamlineGenerator.majorStreamlines) {
                streamline.DebugRender();
            }
            foreach (HyperStreamline streamline in streamlineGenerator.minorStreamlines) {
                streamline.DebugRender();
            }
            foreach (HyperStreamlineIntersection intersection in streamlineGenerator.intersections) {
                intersection.DebugRender();
            }
            foreach (Bridge bridge in streamlineGenerator.bridges) {
                bridge.DebugRender();
            }

        }


    }

    IEnumerator Generate() {
        uint seed = (generatorSeed == 0)
            ? (uint)System.DateTime.Now.Millisecond
            : generatorSeed;

        MeshCreator.CreatePlane(mesh, size.x, size.y, 1, 1);
        TensorField field = TensorFieldGenerator.Generate(position, size, numberOfTensors, numIterations, decayConstant, seed);
        field.Visualise(visMatInstance);
        yield return null;

        streamlineGenerator = new HyperStreamlineGenerator(field, maxLength, minSeperation, lookAheadDist, seedDensity, bridgeProportion, seed, template);
        yield return StartCoroutine(streamlineGenerator.Run(this));
        yield return null;

        List<HyperStreamline> streamlines = new List<HyperStreamline>();
        streamlines.AddRange(streamlineGenerator.majorStreamlines);
        streamlines.AddRange(streamlineGenerator.minorStreamlines);
        List<HyperStreamlineIntersection> intersections = streamlineGenerator.intersections;
        List<Bridge> bridges = streamlineGenerator.bridges;

        meshGenerator = new CityMeshGenerator(seed, template, streamlines, intersections, bridges);
        yield return StartCoroutine(meshGenerator.Run(this));
    }

    void OnDestroy() {
        Destroy(visMatInstance);
    }

}

