using CityGenerator.MeshUtilities;
using UnityEngine;

namespace CityGenerator.Templates {
    public class NetworkElementTemplate {

        public OutlineShape outline;
        public Material mat;

        /// <summary>
        /// The maximum meters the road can rise per meter traveled horizontally
        /// </summary>
        public float maximumSteepness;
        public float minimumIntersectionRadius;

    }
}
