using CityGenerator.FlowFields;
using CityGenerator.MeshUtilities;
using CityGenerator.StreetGraph;
using CityGenerator.Templates;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using IEnumerator = System.Collections.IEnumerator;

namespace CityGenerator {
    class Generator {
        public GameObject CityRoot;
        private HyperStreamlineGenerator streamlineGenerator;
        private CityMeshGenerator meshGenerator;
        private BridgeDesignator bridger;

        Mesh mesh;
        Material visualiserMaterial;
        float2 position;
        float2 size;
        int2 numberOfTensors;
        int numIterations;
        float decayConstant;

        float maxLength;
        float minSeperation;
        float lookAheadDist;
        float seedDensity;
        float bridgeProportion;
        NetworkElementTemplate template;

        uint seed;

        public Generator(
            Mesh mesh,
            Material visualiserMaterial,
            float2 position,
            float2 size,
            int2 numberOfTensors,
            int numIterations,
            float decayConstant,
            float maxLength,
            float minSeperation,
            float lookAheadDist,
            float seedDensity,
            float bridgeProportion,
            NetworkElementTemplate template,
            uint seed
        ) {
            this.mesh = mesh;
            this.visualiserMaterial = visualiserMaterial;

            this.position = position;
            this.size = size;
            this.numberOfTensors = numberOfTensors;
            this.numIterations = numIterations;
            this.decayConstant = decayConstant;

            this.maxLength = maxLength;
            this.minSeperation = minSeperation;
            this.lookAheadDist = lookAheadDist;
            this.seedDensity = seedDensity;
            this.bridgeProportion = bridgeProportion;
            this.template = template;

            this.seed = seed;



            CityRoot = null;
        }

        public IEnumerator Run(MonoBehaviour runner, bool showDebug) {

            TensorField field = TensorFieldGenerator.Generate(position, size, numberOfTensors, numIterations, decayConstant, seed);
            if (showDebug) {
                MeshCreator.CreatePlane(mesh, size.x, size.y, 1, 1);
                field.Visualise(visualiserMaterial);
            }
            yield return null;

            streamlineGenerator = new HyperStreamlineGenerator(field, maxLength, minSeperation, lookAheadDist, seedDensity, seed);
            yield return runner.StartCoroutine(streamlineGenerator.Run(runner));
            yield return null;

            List<HyperStreamline> streamlines = new List<HyperStreamline>();
            streamlines.AddRange(streamlineGenerator.majorStreamlines);
            streamlines.AddRange(streamlineGenerator.minorStreamlines);

            bridger = new BridgeDesignator(streamlineGenerator.majorStreamlines, streamlineGenerator.minorStreamlines, bridgeProportion, template, seed);
            yield return runner.StartCoroutine(bridger.Run(runner));
            yield return null;

            List<HyperStreamlineIntersection> intersections = bridger.intersections;
            List<Bridge> bridges = bridger.bridges;

            meshGenerator = new CityMeshGenerator(seed, template, streamlines, intersections, bridges);
            yield return runner.StartCoroutine(meshGenerator.Run(runner));
            CityRoot = meshGenerator.CityRoot;
        }

        public void DebugDraw() {
            if (streamlineGenerator is not null) {
                foreach (HyperStreamline streamline in streamlineGenerator.majorStreamlines) {
                    streamline.DebugRender();
                }
                foreach (HyperStreamline streamline in streamlineGenerator.minorStreamlines) {
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

        public void DrawDebugGizmos() {
            if (meshGenerator is null) {
                return;
            }

            foreach (var debug in meshGenerator.DebugInfo3D) {
                // skip everything outside a 100m radius
                if ((SceneView.lastActiveSceneView.camera.transform.position - debug.Item1).magnitude > 100) {
                    continue;
                }
                Handles.Label(debug.Item1, debug.Item2);
            }

        }

    }
}
