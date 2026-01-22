
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace CityGenerator.StreetGraph {

    class HyperStreamline {
        public List<float2> points = new List<float2>();
        public float2 PreviousDirection = float2.zero;

        public void DebugRender() {
            if (points.Count == 0) {
                return;
            } else if (points.Count == 1) {
                Debug.DrawRay(new Vector3(points[0].x, 0, points[0].y), Vector3.up, Color.white);
            } else {
                for (int i = 0; i < points.Count - 1; i++) {
                    Debug.DrawLine(new Vector3(points[i].x, 0, points[i].y), new Vector3(points[i-1].x, 0, points[i-1].y), Color.orangeRed);
                }
            }
        }


    }


}
