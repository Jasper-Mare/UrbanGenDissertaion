
using CityGenerator.FlowFields;
using System.Collections.Generic;
using Unity.Mathematics;

namespace CityGenerator.StreetGraph {
    class HyperStreamlineGenerator {

        TensorField tensorField;
        public List<HyperStreamline> majorStreamlines;
        public List<HyperStreamline> minorStreamlines;
        public List<HyperStreamlineIntersection> intersections;

        float maxLength;
        float minSeperation;
        float lookAheadDist;
        float seedPointDensity;

        Random rng;

        /// <param name="tensorField">The field to generate the streamlines on</param>
        /// <param name="maxLength">The furthest a streamline may grow from its seed</param>
        /// <param name="minSeperation">The closest to each other 2 streamlines may be</param>
        /// <param name="lookAheadDist">How far the streamlines search ahead to make an intersection</param>
        /// <param name="seedPointDensity">How many seed points there are per meter squared of tensor field</param>
        public HyperStreamlineGenerator(TensorField tensorField, float maxLength, float minSeperation, float lookAheadDist, float seedPointDensity, uint randomSeed) {
            this.tensorField = tensorField;
            majorStreamlines = new List<HyperStreamline>();
            minorStreamlines = new List<HyperStreamline>();
            intersections = new List<HyperStreamlineIntersection>();
            rng = new Random(randomSeed);

            this.maxLength = maxLength;
            this.minSeperation = minSeperation;
            this.lookAheadDist = lookAheadDist;
            this.seedPointDensity = seedPointDensity;
        }

        public System.Collections.IEnumerator Run(UnityEngine.MonoBehaviour runner) {
            // major streamlines
            yield return null;
            UnityEngine.Debug.Log("Started scattering major seed points");
            yield return runner.StartCoroutine(ScatterSeedPoints(majorStreamlines));
            UnityEngine.Debug.Log($"Done scattering major seed points, {majorStreamlines.Count} points scattered");
            yield return null;
            UnityEngine.Debug.Log("Started growing major streamlines");
            yield return runner.StartCoroutine(GrowStreamlines(majorStreamlines, true));
            UnityEngine.Debug.Log("Done growing major streamlines");

            // minor streamlines
            yield return null;
            UnityEngine.Debug.Log("Started scattering minor seed points");
            yield return runner.StartCoroutine(ScatterSeedPoints(minorStreamlines));
            UnityEngine.Debug.Log($"Done scattering minor seed points, {minorStreamlines.Count} points scattered");
            yield return null;
            UnityEngine.Debug.Log("Started growing minor streamlines");
            yield return runner.StartCoroutine(GrowStreamlines(minorStreamlines, false));
            UnityEngine.Debug.Log("Done growing minor streamlines");

            // identify intersections
            yield return null;
            UnityEngine.Debug.Log("Started identifying intersections");
            yield return runner.StartCoroutine(FindIntersections());
            UnityEngine.Debug.Log($"Done identifying intersections");
        }

        System.Collections.IEnumerator ScatterSeedPoints(List<HyperStreamline> streamlines) {
            int numSeedPoints = (int)(seedPointDensity * tensorField.width * tensorField.height);
            int2 numEdgeSeedPoints = (int2)(seedPointDensity *  new float2(tensorField.width, tensorField.height));

            float spacing = 1 / seedPointDensity;

            // place points at the boundaries of the zone
            for (int i = 0; i < numEdgeSeedPoints.x; i++) {
                float x = spacing * i + 0.5f * spacing; //rng.NextFloat(spacing * i, spacing * (i + 1));

                streamlines.Add(new HyperStreamline(x, 0));
                streamlines.Add(new HyperStreamline(tensorField.width - x, tensorField.height));
            }

            yield return null;

            for (int i = 0; i < numEdgeSeedPoints.y; i++) {
                float y = spacing * i + 0.5f * spacing; //rng.NextFloat(spacing * i, spacing * (i + 1));

                streamlines.Add(new HyperStreamline(0, y));
                streamlines.Add(new HyperStreamline(tensorField.width, tensorField.height - y));
            }

            yield return null;

            // place points throughout the zone

            for (int i = 0; i < numEdgeSeedPoints.x; i++) {
                for (int j = 0; j < numEdgeSeedPoints.y; j++) {
                    float x = spacing * i + 0.5f * spacing; //rng.NextFloat(spacing * i, spacing * (i + 1));
                    float y = spacing * j + 0.5f * spacing; //rng.NextFloat(spacing * j, spacing * (j + 1));
                    streamlines.Add(new HyperStreamline(x, y));
                }

                yield return null;
            }


        }

        System.Collections.IEnumerator GrowStreamlines(List<HyperStreamline> streamlines, bool useMajorEigenVectors) {
            // this keeps track of which streamlines are still going, it starts off the same as the list of streamlines
            Queue<HyperStreamline> unfinishedStreamlines = new Queue<HyperStreamline>(streamlines);

            while (unfinishedStreamlines.Count > 0) {
                // pick the next streamline and grow it till it is finished
                HyperStreamline streamline = unfinishedStreamlines.Dequeue();

                while (!streamline.isComplete) {

                    // get current eigenvector
                    float2 currentPos = streamline.GetCurrentPosition();
                    float2x2 tensor;
                    if (!tensorField.TryGetTensor(currentPos, out tensor)) {
                        // we couldn't get the tensor so we are outside the field
                        streamline.isComplete = true;
                        break;
                    }

                    float2 eigenvector;
                    if (useMajorEigenVectors) {
                        eigenvector = Tensor.getMajorEigenVector(tensor);
                    } else {
                        eigenvector = Tensor.getMinorEigenVector(tensor);
                    }

                    // [1] "select the direction satisfying Ev · Vpre ≤ 0"
                    // ie if Ev · Vpre > 0 then use -Ev,
                    // this is because we want the eigenvectors to represent both directions
                    if (math.dot(eigenvector, streamline.PreviousDirection) < 0) {
                        eigenvector = -1 * eigenvector;
                    }

                    // find next position
                    float2 nextPos = getNextGridPoint(currentPos, eigenvector);

                    // if the streamline gets too close
                    foreach (HyperStreamline compStreamline in streamlines) {
                        // if comparing with itself, only check the tail
                        if (compStreamline == streamline) {
                            // it returns to its origin which indicates a loop
                            // (it's too close to its own tail and hasn't gone far enough to have left yet) 
                            if (
                                streamline.length > minSeperation
                                && math.lengthsq(streamline.points[0] - nextPos) < minSeperation * minSeperation
                            ) {
                                streamline.isComplete = true;
                                break;
                            }
                        } else {
                            foreach (float2 point in compStreamline.points) {
                                // it is too close to an existing hyperstreamline by violating dsep.
                                if (math.lengthsq(point - nextPos) < minSeperation * minSeperation) {
                                    streamline.isComplete = true;
                                    break;
                                }
                            }
                        }
                    }

                    streamline.AddPoint(nextPos);

                    if (streamline.length >= maxLength) {
                        streamline.isComplete = true;
                        break;
                    }


                }

                yield return null;

                UnityEngine.Debug.Log($"Completed streamline, {unfinishedStreamlines.Count} streamlines left");
                // UnityEngine.Debug.Log($"Completed streamline has {streamline.points.Count} points and is {streamline.length}m long");

            }
        }

        System.Collections.IEnumerator FindIntersections() {
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
                                intersections.Add(new HyperStreamlineIntersection(intersectPoint, majorStreamline, minorStreamline));
                            }


                        }
                    }

                    yield return null;
                }
            }


            // check for which tips end near other things

            yield return null;
        }

        // find next position based on DDA (inspired from [3A] and [3B])
        float2 getNextGridPoint(float2 currentPoint, float2 direction) {
            float2 cellSize = new float2(tensorField.width / tensorField.numTensorsX, tensorField.height / tensorField.numTensorsY);
            float2 coordsInTile = currentPoint % cellSize;
            float2 tilePos = currentPoint - coordsInTile;

            return currentPoint + (direction * cellSize * 0.5f);
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

/*
[1] https://dl.acm.org/doi/pdf/10.1145/1360612.1360702

An adaptive Runge-Kutta scheme [Cash and Karp 1990][2] is used to compute a 
hyperstreamline, which has been modified to handle tensor fields. Given a
position of the current end point, we extract the direction in which the
hyperstreamline grows by finding the major eigenvector value Ev at the end
point. Let Vpre be the previous direction we use to compute the current point,
to remove the sign ambiguity in eigenvector directions, we select the direction
satisfying Ev · Vpre ≤ 0. The next integration point is then found using the
numerical scheme.

A hyperstreamline stops growing on the following stopping criteria: 
1) it hits the boundary of the domain, 
2) it runs into a degenerate point,
3) it returns to its origin which indicates a loop, 
4) it exceeds a user defined maximum length, 
or 
5) it is too close to an existing hyperstreamline by violating dsep. 

Additionally, we improve connectivity by continuing the tracing for a distance 
dlookahead to search an intersection with other hyperstreamline even when 
stopping criteria 4 or 5 is met.

We also allow the tracing to cross relatively narrow water regions to form
bridges depending on the required length of the bridge and the angle of the
intersection with the coastline.


[2] https://tobydriscoll.net/fnc-julia/ivp/adaptive-rk.html
Adaptive RK is basically just estimating the step size based on the data

[3A] https://theshoemaker.de/posts/ray-casting-in-2d-grids
[3B] https://www.youtube.com/watch?v=NbSee-XM7WA
As we have a grid with finite detail we should just step into the next grid cell like a raycast

[4] https://stackoverflow.com/a/1120126
*/

