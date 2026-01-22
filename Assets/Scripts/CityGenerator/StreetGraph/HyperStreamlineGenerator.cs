
using CityGenerator.FlowFields;
using System.Collections.Generic;

namespace CityGenerator.StreetGraph {
    class HyperStreamlineGenerator {

        TensorField tensorField;
        List<HyperStreamline> majorStreamlines;
        List<HyperStreamline> minorStreamlines;

        public HyperStreamlineGenerator(TensorField tensorField, float maxLength, float minSeperation, float lookaheadDist) {
            this.tensorField = tensorField;
            majorStreamlines = new List<HyperStreamline>();
            minorStreamlines = new List<HyperStreamline>();
        }

    }
}

/*
https://dl.acm.org/doi/pdf/10.1145/1360612.1360702

An adaptive Runge-Kutta scheme [Cash and Karp 1990] is used to compute a 
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

