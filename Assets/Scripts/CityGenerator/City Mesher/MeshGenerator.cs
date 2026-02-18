using CityGenerator.Templates;
using Unity.Mathematics;
using Debug = UnityEngine.Debug;
using IEnumerator = System.Collections.IEnumerator;
using MonoBehaviour = UnityEngine.MonoBehaviour;

namespace CityGenerator.MeshUtilities {
    class MeshGenerator {

        Random rng;
        NetworkElementTemplate streetTemplate;

        public MeshGenerator(uint randomSeed, NetworkElementTemplate template) {
            rng = new Random(randomSeed);
            streetTemplate = template;
        }

        public IEnumerator Run(MonoBehaviour runner) {

            yield return null;
            Debug.Log("");
        }


    }
}
