using CityGenerator;
using CityGenerator.MeshUtilities;
using CityGenerator.Templates;
using System.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(UniqueMesh))]
public class CityMesherTestScript : MonoBehaviour {
    bool shouldRegenerate;
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
    float minSeparation = 1;
    //[SerializeField]
    float lookAheadDist = 5;
    [SerializeField]
    float seedDensityCount = 1f;
    [SerializeField]
    float seedDensityLength = 1f;

    [Header("Bridge Designator Properties")]
    [SerializeField]
    [Range(0, 1)]
    float bridgeProportion = 0.1f;
    [SerializeField]
    NetworkElementTemplate template;

    [Header("Mesh Generator Properties")]
    [SerializeField]
    float groundHeight = 0f;

    float2 position = float2.zero;

    private GameObject CityRoot;

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

        shouldRegenerate = false;
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
            maxLength, minSeparation, lookAheadDist, seedDensity,
            bridgeProportion, template, groundHeight, seed
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

    // Based off [1]
    [CustomEditor(typeof(CityMesherTestScript))]
    class CityMesherTestScriptEditor : Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            GUILayout.Space(10);

            if (GUILayout.Button("Generate")) {
                CityMesherTestScript testScript = target.GetComponent<CityMesherTestScript>();
                testScript.shouldRegenerate = true;
            }

            GUILayout.Space(10);

        }
    }

}

/* References:
[1] https://discussions.unity.com/t/create-a-button-in-the-inspector/22432/3
*/
