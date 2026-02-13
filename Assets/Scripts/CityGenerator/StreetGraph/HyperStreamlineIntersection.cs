using Unity.Mathematics;
using UnityEngine;

namespace CityGenerator.StreetGraph {
    class HyperStreamlineIntersection {
        public readonly HyperStreamline[] intersectingStreamlines;
        // the index of the point in the streamlines that the intersection is closest to
        public readonly int[] pointIndexesInStreamlines;
        public float2 position;

        public HyperStreamlineIntersection(float2 position, HyperStreamline[] streamlines, int[] positionIndexes) {
            this.intersectingStreamlines = streamlines;
            this.pointIndexesInStreamlines = positionIndexes;
            this.position = position;
        }

        public void DebugRender() {
            Debug.DrawRay(new Vector3(position.x, 1.5f, position.y), Vector3.up * -3, Color.brown);
        }

        public int getPointIndex(HyperStreamline streamlineToCheck) {
            for (int i = 0; i < intersectingStreamlines.Length; i++) {
                if (intersectingStreamlines[i] == streamlineToCheck) {
                    return pointIndexesInStreamlines[i];
                }
            }
            // if the streamline isn't a part of this intersection
            return -1;
        }


    }
}
