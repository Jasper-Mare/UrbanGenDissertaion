using CityGenerator.FlowFields;
using CityGenerator.MeshUtilities;
using CityGenerator.StreetGraph;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(UniqueMesh))]
public class StreetGraphTestScript : MonoBehaviour {

    [SerializeField]
    bool updateVisualisation = false;

    private Mesh mesh;
    private Renderer rend;
    private Material visMatInstance;
    private HyperStreamlineGenerator generator;

    void Start() {
        mesh = GetComponent<UniqueMesh>().Mesh;
        rend = GetComponent<Renderer>();
        rend.material = (visMatInstance = rend.material);
        MeshCreator.CreatePlane(mesh, 100, 100, 1, 1);

        updateVisualisation = true;
    }

    void Update() {

        if (updateVisualisation) {
            uint seed = (uint)System.DateTime.Now.Millisecond;


            TensorField field = TensorFieldGenerator.Generate(float2.zero, new float2(100, 100), new int2(100, 100), 4, seed);
            field.Visualise(visMatInstance);
            generator = new HyperStreamlineGenerator(field, 50, 1, 5, 0.1f, seed);
            StartCoroutine(generator.Run(this));

            updateVisualisation = false;
        }

        if (generator is not null) {
            foreach (HyperStreamline streamline in generator.majorStreamlines) {
                streamline.DebugRender();
            }
            foreach (HyperStreamline streamline in generator.minorStreamlines) {
                streamline.DebugRender();
            }
        }


    }

    void OnDestroy() {
        Destroy(visMatInstance);
    }

}

