using Unity.Mathematics;

namespace CityGenerator.FlowFields {
    struct Tensor {
        float2x2 values;
    }
}

/*
References:
[1] G. Chen, G. Esch, P. Wonka, P. M¨uller, and E. Zhang, “Interactive procedural street modeling,”
    in ACM SIGGRAPH 2008 papers, ser. SIGGRAPH ’08. New York, NY, USA: Association for
    Computing Machinery, Aug. 2008, pp. 1–10. DOI:10.1145/1399504.1360702
*/

// https://github.com/minino92/InteractiveProceduralStreetModeling/blob/master/pdf/2008.SG.Chen.InteractiveProceduralStreetModeling.pdf
