using CityGenerator.StreetGraph;
using CityGenerator.Templates;
using System.Collections.Generic;
using Unity.Mathematics;
using Debug = UnityEngine.Debug;
using GameObject = UnityEngine.GameObject;
using IEnumerator = System.Collections.IEnumerator;
using MonoBehaviour = UnityEngine.MonoBehaviour;

namespace CityGenerator.MeshUtilities {
    class CityMeshGenerator {

        Random rng;
        NetworkElementTemplate streetTemplate;

        List<HyperStreamline> inStreamlines;
        List<HyperStreamlineIntersection> inIntersections;
        List<Bridge> inBridges;

        GameObject CityRoot;

        public CityMeshGenerator(uint randomSeed, NetworkElementTemplate template, List<HyperStreamline> streamlines, List<HyperStreamlineIntersection> intersections, List<Bridge> bridges) {
            rng = new Random(randomSeed);
            streetTemplate = template;

            // these come from the previous generation stage
            this.inStreamlines = streamlines;
            this.inIntersections = intersections;
            this.inBridges = bridges;

            CityRoot = new GameObject("City");
        }

        public IEnumerator Run(MonoBehaviour runner) {

            yield return null;
            Debug.Log("Started building roads");
            yield return runner.StartCoroutine(MakeRoads());
            Debug.Log("Done building roads");

        }

        public IEnumerator MakeRoads() {
            foreach (HyperStreamline streamline in inStreamlines) {
                // discard all lines made of only one point or less
                if (streamline.points.Count <= 1) {
                    continue;
                }

                // basic approach, turn the streamline points into oriented points, then make basic meshes on them
                OrientedPoint[] points = new OrientedPoint[streamline.points.Count];

                for (int i = 0; i < points.Length - 1; i++) {
                    float2 pA = streamline.points[i];
                    float2 pB = streamline.points[i + 1];

                    float3 pA3D = new float3(pA.x, 0, pA.y);

                    float3 tangent = math.normalize(new float3(pB.x, 0, pB.y) - pA3D);
                    float3 binormal = math.normalize(math.cross(new float3(0, 1, 0), tangent));
                    float3 normal = math.cross(tangent, binormal);

                    points[i] = new OrientedPoint((UnityEngine.Vector3)pA3D, UnityEngine.Quaternion.LookRotation(tangent, normal));
                }

                // make the final point have the same orientation as the penultimate one
                float2 lastPoint = streamline.points[points.Length - 1];
                points[points.Length - 1] = new OrientedPoint(
                    new UnityEngine.Vector3(lastPoint.x, 0, lastPoint.y),
                    points[points.Length - 2].rotation
                );

                foreach (OrientedPoint pt in points) {
                    GameObject objDebug = new GameObject($"debug{pt.GetHashCode()}");
                    objDebug.transform.parent = CityRoot.transform;
                    var debug = objDebug.AddComponent<OrientedPointDebug>();
                    debug.point = pt;
                }

                // make an object to attach this road to
                GameObject obj = new GameObject($"Road{streamline.GetHashCode()}");
                obj.transform.parent = CityRoot.transform;
                UnityEngine.Mesh objMesh = obj.AddComponent<UniqueMesh>().Mesh;
                UnityEngine.Renderer objRend = obj.AddComponent<UnityEngine.MeshRenderer>();

                objRend.material = streetTemplate.roadMaterial;

                // create the mesh for the object
                MeshCreator.ExtrudeMeshAlongOrientedPath(
                    objMesh,
                    streetTemplate.outline,
                    points
                );

                yield return null;
            }

        }


    }
}
