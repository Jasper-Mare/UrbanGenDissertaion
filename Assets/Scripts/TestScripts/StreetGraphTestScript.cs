using CityGenerator.FlowFields;
using CityGenerator.MeshUtilities;
using CityGenerator.StreetGraph;
using CityGenerator.Templates;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(UniqueMesh))]
public class StreetGraphTestScript : MonoBehaviour {

    [SerializeField]
    bool shouldRegenerate = false;
    [SerializeField]
    uint generatorSeed = 0;

    private Mesh mesh;
    private Renderer rend;
    private Material visMatInstance;
    private HyperStreamlineGenerator generator;
    private BridgeDesignator bridger;

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

        template.maximumSteepness = 0.1f;
        template.minimumIntersectionRadius = 3f;
        template.bridgingHeight = 3f;

        shouldRegenerate = true;
    }

    void Update() {

        if (shouldRegenerate) {
            StartCoroutine(Generate());

            shouldRegenerate = false;
        }

        if (generator is not null) {
            foreach (HyperStreamline streamline in generator.majorStreamlines) {
                streamline.DebugRender();
            }
            foreach (HyperStreamline streamline in generator.minorStreamlines) {
                streamline.DebugRender();
            }
        }

        if (bridger is not null) {
            foreach (HyperStreamlineIntersection intersection in bridger.intersections) {
                intersection.DebugRender();
            }
            foreach (Bridge bridge in bridger.bridges) {
                bridge.DebugRender();
            }

        }


    }

    IEnumerator Generate() {
        uint seed = (generatorSeed == 0)
            ? (uint)System.DateTime.Now.Millisecond
            : generatorSeed;

        Debug.Log("Running TensorFieldGenerator");
        MeshCreator.CreatePlane(mesh, size.x, size.y, 1, 1);
        TensorField field = TensorFieldGenerator.Generate(position, size, numberOfTensors, numIterations, decayConstant, seed);
        field.Visualise(visMatInstance);

        Debug.Log("Running HyperStreamlineGenerator");
        generator = new HyperStreamlineGenerator(field, maxLength, minSeperation, lookAheadDist, seedDensity, seed);
        yield return StartCoroutine(generator.Run(this, true));
        yield return null;

        Debug.Log("Running BridgeDesignator");
        bridger = new BridgeDesignator(generator.majorStreamlines, generator.minorStreamlines, bridgeProportion, template, seed);
        yield return StartCoroutine(bridger.Run(this, true));
        yield return null;
    }

    void OnDestroy() {
        Destroy(visMatInstance);
    }

}

