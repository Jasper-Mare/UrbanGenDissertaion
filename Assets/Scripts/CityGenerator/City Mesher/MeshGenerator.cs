using Unity.Mathematics;
using Debug = UnityEngine.Debug;
using IEnumerator = System.Collections.IEnumerator;
using MonoBehaviour = UnityEngine.MonoBehaviour;

namespace CityGenerator.MeshUtilities {
    class MeshGenerator {

        Random rng;

        public MeshGenerator(uint randomSeed) {
            rng = new Random(randomSeed);
        }

        public IEnumerator Run(MonoBehaviour runner) {

            yield return null;
            Debug.Log("");
        }


    }
}
