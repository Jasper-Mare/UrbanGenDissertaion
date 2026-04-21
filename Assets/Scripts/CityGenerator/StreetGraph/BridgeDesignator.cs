using CityGenerator.Templates;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Debug = UnityEngine.Debug;
using IEnumerator = System.Collections.IEnumerator;
using MonoBehaviour = UnityEngine.MonoBehaviour;

namespace CityGenerator.StreetGraph {
    class BridgeDesignator {

        public List<HyperStreamline> majorStreamlines;
        public List<HyperStreamline> minorStreamlines;
        public List<HyperStreamlineIntersection> intersections;
        public List<Bridge> bridges;


        NetworkElementTemplate streetTemplate;

        float bridgeProportion;

        Random rng;

        public BridgeDesignator(List<HyperStreamline> majorStreamlines, List<HyperStreamline> minorStreamlines, float bridgeProportion, NetworkElementTemplate template, uint randomSeed) {
            intersections = new List<HyperStreamlineIntersection>();
            bridges = new List<Bridge>();

            this.streetTemplate = template;
            this.majorStreamlines = majorStreamlines;
            this.minorStreamlines = minorStreamlines;
            rng = new Random(randomSeed);

            this.bridgeProportion = bridgeProportion;
        }

        public IEnumerator Run(MonoBehaviour runner, bool showDebug) {

            // identify intersections
            yield return null;
            if (showDebug) Debug.Log("CityGenerator - BridgeDesignator: Started identifying intersections");
            yield return runner.StartCoroutine(FindIntersections());
            if (showDebug) Debug.Log("CityGenerator - BridgeDesignator: Done identifying intersections");

            // build bridges
            yield return null;
            if (showDebug) Debug.Log("CityGenerator - BridgeDesignator: Started identifying bridges");
            yield return runner.StartCoroutine(IdentifyBridges(runner));
            if (showDebug) Debug.Log($"CityGenerator - BridgeDesignator: Done identifying bridges {intersections.Count} intersections found.");
        }

        IEnumerator FindIntersections() {
            // streamlines of the same order don't cross over, so won't have intersections,
            // however the streamlines of the any order may join at the end of the streamline

            // check for each major which minor crosses over
            foreach (HyperStreamline majorStreamline in majorStreamlines) {
                foreach (HyperStreamline minorStreamline in minorStreamlines) {
                    for (int iMajor = 0; iMajor < majorStreamline.points.Count - 1; iMajor++) {
                        for (int iMinor = 0; iMinor < minorStreamline.points.Count - 1; iMinor++) {

                            float2 majorA = majorStreamline.points[iMajor];
                            float2 majorB = majorStreamline.points[iMajor + 1];

                            float2 minorA = minorStreamline.points[iMinor];
                            float2 minorB = minorStreamline.points[iMinor + 1];

                            float2 intersectPoint;
                            if (CheckLinesIntersect(majorA, majorB, minorA, minorB, out intersectPoint)) {
                                HyperStreamline[] intersectingStreamlines = new HyperStreamline[] {
                                    majorStreamline,
                                    minorStreamline
                                };
                                int[] posIndexes = new int[] {
                                    iMajor,
                                    iMinor
                                };

                                HyperStreamlineIntersection newIntersection = new HyperStreamlineIntersection(
                                    intersectPoint,
                                    intersectingStreamlines,
                                    posIndexes
                                );

                                intersections.Add(newIntersection);
                                majorStreamline.intersections.Add(newIntersection);
                                minorStreamline.intersections.Add(newIntersection);

                            }


                        }
                    }

                    yield return null;
                }
            }


            // check for which tips end near other things

            yield return null;
        }

        IEnumerator IdentifyBridges(MonoBehaviour runner) {
            int numBridges = (int)(intersections.Count * bridgeProportion);

            // loop over all the intersections
            for (int iBridge = 0; iBridge < numBridges; iBridge++) {
                // find the intersection to turn into a bridge
                int iIntersection = (int)(iBridge * bridgeProportion);
                HyperStreamlineIntersection intersection = intersections[iIntersection];

                // make a bridge around that intersection
                yield return runner.StartCoroutine(MakeBridge(intersection));

            }

        }

        IEnumerator MakeBridge(HyperStreamlineIntersection intersection) {
            float requiredMinSeperation = (streetTemplate.bridgingHeight / streetTemplate.maximumSteepness) + streetTemplate.minimumIntersectionRadius * 2;

            // pick a random streamline to make the bridge
            HyperStreamline bridgeStreamline = intersection.intersectingStreamlines[
                rng.NextInt(0, intersection.intersectingStreamlines.Length - 1)
            ];

            // sort that streamline's intersections by their position indexes
            bridgeStreamline.intersections = bridgeStreamline.intersections.OrderBy(
                x => x.getPointIndex(bridgeStreamline)
            ).ToList();

            // explore the surrounding intersections to find which ones are too close to unbridge
            int iCurrentIntersection = bridgeStreamline.intersections.IndexOf(intersection);
            int iBridgeLeft = iCurrentIntersection;
            int iBridgeRight = iCurrentIntersection;

            // explore leftward
            while (true) {
                // stop if the index reaches the lowest index intersection 
                if (iBridgeLeft == 0) {
                    break;
                }

                HyperStreamlineIntersection leftIntersection = bridgeStreamline.intersections[iBridgeLeft];
                HyperStreamlineIntersection nextLeftIntersection = bridgeStreamline.intersections[iBridgeLeft - 1];

                // stop if this and the next intersection are far enough apart
                if (math.distance(leftIntersection.position, nextLeftIntersection.position) >= requiredMinSeperation) {
                    break;
                }

                iBridgeLeft--;

            }

            // explore rightward
            while (true) {
                // stop if the index reaches the highest index intersection 
                if (iBridgeRight == bridgeStreamline.intersections.Count - 1) {
                    break;
                }

                HyperStreamlineIntersection rightIntersection = bridgeStreamline.intersections[iBridgeRight];
                HyperStreamlineIntersection nextRightIntersection = bridgeStreamline.intersections[iBridgeRight + 1];

                // stop if this and the next intersection are far enough apart
                if (math.distance(rightIntersection.position, nextRightIntersection.position) >= requiredMinSeperation) {
                    break;
                }

                iBridgeRight++;

            }


            // Make a new bridge
            Bridge newBridge = new Bridge(bridgeStreamline);
            // add all the intersections between iBridgeLeft and iBridgeRight to the bridge
            for (int i = iBridgeLeft; i <= iBridgeRight; i++) {
                newBridge.intersections.Add(bridgeStreamline.intersections[i]);
            }

            bridges.Add(newBridge);

            yield return null;
        }

        // from [4]
        private bool CheckLinesIntersect(float2 start1, float2 end1, float2 start2, float2 end2, out float2 overlap) {
            float denom = ((end1.x - start1.x) * (end2.y - start2.y)) - ((end1.y - start1.y) * (end2.x - start2.x));

            //  AB & CD are parallel 
            if (denom == 0) {
                overlap = float2.zero;
                return false;
            }

            float numer = ((start1.y - start2.y) * (end2.x - start2.x)) - ((start1.x - start2.x) * (end2.y - start2.y));
            float r = numer / denom;

            float numer2 = ((start1.y - start2.y) * (end1.x - start1.x)) - ((start1.x - start2.x) * (end1.y - start1.y));
            float s = numer2 / denom;

            if ((r < 0 || r > 1) || (s < 0 || s > 1)) {
                overlap = float2.zero;
                return false;
            }

            // Find intersection point
            overlap.x = start1.x + (r * (end1.x - start1.x));
            overlap.y = start1.y + (r * (end1.y - start1.y));

            return true;
        }
    }
}
