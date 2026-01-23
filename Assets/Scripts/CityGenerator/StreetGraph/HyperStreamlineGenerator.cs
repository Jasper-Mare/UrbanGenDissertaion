
using CityGenerator.FlowFields;
using System.Collections.Generic;
using Unity.Mathematics;

namespace CityGenerator.StreetGraph {
    class HyperStreamlineGenerator {

        TensorField tensorField;
        public List<HyperStreamline> majorStreamlines;
        public List<HyperStreamline> minorStreamlines;

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
        public HyperStreamlineGenerator(TensorField tensorField, float maxLength, float minSeperation, float lookAheadDist, float seedPointDensity) {
            this.tensorField = tensorField;
            majorStreamlines = new List<HyperStreamline>();
            minorStreamlines = new List<HyperStreamline>();
            rng = new Random((uint)System.DateTime.Now.Millisecond);

            this.maxLength = maxLength;
            this.minSeperation = minSeperation;
            this.lookAheadDist = lookAheadDist;
            this.seedPointDensity = seedPointDensity;
        }

        public System.Collections.IEnumerator Run(UnityEngine.MonoBehaviour runner) {
            yield return runner.StartCoroutine(ScatterSeedPoints(majorStreamlines));
            yield return runner.StartCoroutine(GrowStreamlines(majorStreamlines, true));

            //ScatterSeedPoints(minorStreamlines);
            //GrowStreamlines(minorStreamlines, false);
        }

        System.Collections.IEnumerator ScatterSeedPoints(List<HyperStreamline> streamlines) {
            int numSeedPoints = (int)(seedPointDensity * tensorField.width * tensorField.height);
            int2 numEdgeSeedPoints = (int2)(seedPointDensity *  new float2(tensorField.width, tensorField.height));

            float spacing = 1 / seedPointDensity;

            // place points at the boundaries of the zone
            for (int i = 0; i < numEdgeSeedPoints.x; i++) {
                float x = rng.NextFloat(spacing * i, spacing * (i + 1));

                streamlines.Add(new HyperStreamline(x, 0));
                streamlines.Add(new HyperStreamline(tensorField.width - x, tensorField.height));
            }

            for (int i = 0; i < numEdgeSeedPoints.y; i++) {
                float y = rng.NextFloat(spacing * i, spacing * (i + 1));

                streamlines.Add(new HyperStreamline(0, y));
                streamlines.Add(new HyperStreamline(tensorField.width, tensorField.height - y));
            }

            yield return null;

            // place points throughout the zone

            for (int i = 0; i < numEdgeSeedPoints.x; i++) {
                for (int j = 0; j < numEdgeSeedPoints.y; j++) {
                    float x = rng.NextFloat(spacing * i, spacing * (i + 1));
                    float y = rng.NextFloat(spacing * j, spacing * (j + 1));
                    streamlines.Add(new HyperStreamline(x, y));
                }
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
                    if (math.dot(eigenvector, streamline.PreviousDirection) > 0) {
                        eigenvector = -1 * eigenvector;
                    }

                    // find next position
                    float2 nextPos = getNextGridPoint(currentPos, eigenvector);

                    streamline.AddPoint(nextPos);

                    if (streamline.length >= maxLength) {
                        streamline.isComplete = true;
                        break;
                    }


                }

                yield return null;

                UnityEngine.Debug.Log($"Completed streamline has {streamline.points.Count} points and is {streamline.length}m long");

            }



        }

        // find next position based on [3]
        float2 getNextGridPoint(float2 currentPoint, float2 direction) {
            float2 cellSize = new float2(tensorField.width / tensorField.numTensorsX, tensorField.height / tensorField.numTensorsY);
            float2 tileCoords = math.floor(currentPoint / cellSize);
            float2 dirSign = math.sign(direction); // this might need changing

            float2 dt = ((tileCoords + dirSign) * cellSize - currentPoint) / direction;

            return float2.zero;
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

*/

/*
[2] https://tobydriscoll.net/fnc-julia/ivp/adaptive-rk.html
Adaptive RK is basically just estimating the step size based on the data

[3] https://theshoemaker.de/posts/ray-casting-in-2d-grids
As we have a grid with finite detail we should just step into the next grid cell like a raycast

*/

