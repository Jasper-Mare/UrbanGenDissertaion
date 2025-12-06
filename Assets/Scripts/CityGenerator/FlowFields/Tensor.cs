namespace CityGenerator.FlowFields {
    static class Tensor {
        /*
        public Vector2 getMajorEigenVector() {
            values
        }

        public Vector2 getMinorEigenVector() {

        }
        */
    }
}

/*
References:
[1] G. Chen, G. Esch, P. Wonka, P. M¨uller, and E. Zhang, “Interactive procedural street modeling,”
    in ACM SIGGRAPH 2008 papers, ser. SIGGRAPH ’08. New York, NY, USA: Association for
    Computing Machinery, Aug. 2008, pp. 1–10. DOI:10.1145/1399504.1360702

[2] https://www.3blue1brown.com/lessons/quick-eigen (Accessed 06/12/2025)

*/

/*

A  B

C  D

p = AD - BC

m = (A + B) / 2

m +- sqrt(m^2 - p)


*/

// https://github.com/minino92/InteractiveProceduralStreetModeling/blob/master/pdf/2008.SG.Chen.InteractiveProceduralStreetModeling.pdf
