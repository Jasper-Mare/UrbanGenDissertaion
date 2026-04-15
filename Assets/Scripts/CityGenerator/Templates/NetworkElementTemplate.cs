using CityGenerator.MeshUtilities;
using UnityEngine;

namespace CityGenerator.Templates {

    [CreateAssetMenu(fileName = "NewTemplate", menuName = "CityGenerator/NetworkElementTemplate", order = 1)]
    public class NetworkElementTemplate : ScriptableObject {

        public OutlineShape outline;
        public Material roadMaterial;

        /// <summary>
        /// The maximum meters the road can rise per meter traveled horizontally
        /// </summary>
        public float maximumSteepness;
        public float minimumIntersectionRadius;
        public float bridgingHeight;

    }
}

// https://docs.unity3d.com/6000.3/Documentation/Manual/class-ScriptableObject.html
