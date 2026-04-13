using CityGenerator;
using CityGenerator.MeshUtilities;
using CityGenerator.Templates;
using System.Collections;
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
    private Generator gen;

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
    [SerializeField]
    float bridgeMaximumSteepness = 1;
    [SerializeField]
    float bridgeHeight = 3f;
    [SerializeField]
    float intersectionMinimumRadius = 3f;

    float2 position = float2.zero;

    private GameObject CityRoot;

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

        template.maximumSteepness = bridgeMaximumSteepness;
        template.minimumIntersectionRadius = intersectionMinimumRadius;
        template.bridgingHeight = bridgeHeight;

        /*

        O=========O
        |         |
        O         O
         \       /
          O-----O

        */
        template.outline = new OutlineShape(
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
        template.roadMaterial = templateMaterial;

        shouldRegenerate = true;
    }

    void Update() {

        if (shouldRegenerate) {
            StartCoroutine(RunGenerator());

            shouldRegenerate = false;
        }

        if (gen is not null) {
            gen.DebugDraw();
        }

    }

    IEnumerator RunGenerator() {
        if (CityRoot is not null) {
            Destroy(CityRoot);
            CityRoot = null;
        }

        uint seed = (generatorSeed == 0)
            ? (uint)System.DateTime.Now.Millisecond
            : generatorSeed;

        gen = new Generator(mesh, visMatInstance, position, size,
            numberOfTensors, numIterations, decayConstant,
            maxLength, minSeperation, lookAheadDist, seedDensity,
            bridgeProportion, template, seed
        );

        yield return StartCoroutine(gen.Run(this, true));
        CityRoot = gen.CityRoot;
    }

    void OnDestroy() {
        Destroy(visMatInstance);
    }

    // draw debug stuff
    void OnDrawGizmos() {
        if (gen is not null) {
            gen.DrawDebugGizmos();
        }
    }
}
