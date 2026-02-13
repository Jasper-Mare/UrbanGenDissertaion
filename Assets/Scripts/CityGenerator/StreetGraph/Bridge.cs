using System.Collections.Generic;
using UnityEngine;

namespace CityGenerator.StreetGraph {
    class Bridge {
        public HyperStreamline streamline;
        public List<HyperStreamlineIntersection> intersections;

        public Bridge(HyperStreamline streamline) {
            this.streamline = streamline;
        }

        public void DebugRender() {

            Debug.DrawRay(new Vector3(intersections[0].position.x, 1.5f, intersections[0].position.y), Vector3.up * 1.5f, Color.green);
            for (int i = 1; i < intersections.Count; i++) {
                Debug.DrawRay(
                    new Vector3(intersections[i].position.x, 1.5f, intersections[i].position.y),
                    Vector3.up * 1.5f,
                    Color.green
                );
                Debug.DrawLine(
                    new Vector3(intersections[i].position.x, 3, intersections[i].position.y),
                    new Vector3(intersections[i - 1].position.x, 3, intersections[i - 1].position.y),
                    Color.green
                );
            }

        }

    }
}
