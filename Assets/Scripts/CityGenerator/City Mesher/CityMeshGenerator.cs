using CityGenerator.StreetGraph;
using CityGenerator.Templates;
using System.Collections.Generic;
using System.Linq;
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

        public GameObject CityRoot;

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

        IEnumerator MakeRoads() {
            foreach (HyperStreamline streamline in inStreamlines) {
                // discard all lines made of only one point or less
                if (streamline.points.Count <= 1) {
                    continue;
                }

                // sort this streamline's intersections by their position indexes
                streamline.intersections = streamline.intersections.OrderBy(
                    x => x.getPointIndex(streamline)
                ).ToList();

                // get bridges on this streamline
                List<Bridge> streamlineBridges = inBridges.Where(x => x.streamline == streamline).ToList();

                // basic approach, turn the streamline points into oriented points, then make basic meshes on them
                OrientedPoint[] points = new OrientedPoint[streamline.points.Count];


                // make the first point based off it's connection to the next one and assuming they're level
                float2 firstPoint = streamline.points[0];
                points[0] = new OrientedPoint(
                    new UnityEngine.Vector3(
                        firstPoint.x,
                        GetBridgeHeight(streamline, 0, streamlineBridges),
                        firstPoint.y),
                    UnityEngine.Quaternion.LookRotation(
                        new UnityEngine.Vector3(
                            streamline.points[1].x - streamline.points[0].x,
                            0,
                            streamline.points[1].y - streamline.points[0].y
                        ),
                        UnityEngine.Vector3.up
                    )
                );

                for (int i = 1; i < points.Length; i++) {
                    float height = GetBridgeHeight(streamline, i, streamlineBridges);


                    float2 pA = streamline.points[i - 1];
                    float2 pB = streamline.points[i];

                    float3 pA3D = new float3(pA.x, height, pA.y);
                    float3 pB3D = new float3(pB.x, points[i - 1].position.y, pB.y);

                    float3 tangent = math.normalize(pB3D - pA3D);
                    float3 binormal = math.normalize(math.cross(new float3(0, 1, 0), tangent));
                    float3 normal = math.cross(tangent, binormal);

                    points[i] = new OrientedPoint((UnityEngine.Vector3)pA3D, UnityEngine.Quaternion.LookRotation(tangent, normal));
                }

                // debug oriented points
                //foreach (OrientedPoint pt in points) {
                //    GameObject objDebug = new GameObject($"debug{pt.GetHashCode()}");
                //    objDebug.transform.parent = CityRoot.transform;
                //    var debug = objDebug.AddComponent<OrientedPointDebug>();
                //    debug.point = pt;
                //}

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

        float GetBridgeHeight(HyperStreamline streamline, int iPoint, List<Bridge> bridges) {
            Bridge currentBridge;

            // if the current streamline has no bridges then all points will be at 0
            if (bridges.Count == 0) {
                return 0;
            } else {
                // otherwise find which bridge we are in
                currentBridge = bridges.Where(
                    x => x.intersections[0].getPointIndex(streamline) <= iPoint
                    && x.intersections[x.intersections.Count -1].getPointIndex(streamline) >= iPoint
                ).ElementAtOrDefault(0);
            }

            if (currentBridge is null) {
                return 0;
            } else {
                return streetTemplate.bridgingHeight;
            }

            // TODO: need to ease in and out of bridges

            // float bridgeRise = (streetTemplate.bridgingHeight / streetTemplate.maximumSteepness);
            // float minSeperation = bridgeRise + 2 * streetTemplate.minimumIntersectionRadius;

        }


    }
}
