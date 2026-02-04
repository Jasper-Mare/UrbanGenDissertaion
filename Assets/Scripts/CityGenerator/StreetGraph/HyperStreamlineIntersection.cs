using Unity.Mathematics;
using UnityEngine;

namespace CityGenerator.StreetGraph {
    class HyperStreamlineIntersection {
        public readonly HyperStreamline[] intersectingStreamlines;
        public float2 position;

        public HyperStreamlineIntersection(float2 position, params HyperStreamline[] streamlines) {
            this.intersectingStreamlines = streamlines;
            this.position = position;
        }

        public void DebugRender() {
            Debug.DrawRay(new Vector3(position.x, 3, position.y), Vector3.up * 3, Color.cyan);
        }


    }
}
