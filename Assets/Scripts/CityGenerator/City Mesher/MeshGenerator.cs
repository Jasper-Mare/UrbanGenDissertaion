using CityGenerator.StreetGraph;
using CityGenerator.Templates;
using System.Collections.Generic;
using Unity.Mathematics;
using Debug = UnityEngine.Debug;
using IEnumerator = System.Collections.IEnumerator;
using MonoBehaviour = UnityEngine.MonoBehaviour;

namespace CityGenerator.MeshUtilities {
    class MeshGenerator {

        Random rng;
        NetworkElementTemplate streetTemplate;

        List<HyperStreamline> streamlines;
        List<HyperStreamlineIntersection> intersections;
        List<Bridge> bridges;

        public MeshGenerator(uint randomSeed, NetworkElementTemplate template, List<HyperStreamline> streamlines, List<HyperStreamlineIntersection> intersections, List<Bridge> bridges) {
            rng = new Random(randomSeed);
            streetTemplate = template;

            // these ccome from the previous generation stage
            this.streamlines = streamlines;
            this.intersections = intersections;
            this.bridges = bridges;
        }

        public IEnumerator Run(MonoBehaviour runner) {

            yield return null;
            Debug.Log("");
        }


    }
}
