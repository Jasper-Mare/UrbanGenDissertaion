using System;
using UnityEngine;

namespace CityGenerator.MeshUtilities {

    // from [1]
    public struct BezierCurve {
        // Instance

        private Vector3[] pts;

        public BezierCurve(Vector3[] pts) {
            if (pts.Length != 4) {
                throw new ArgumentException("A Bezier spline must have 4 points!");
            }

            this.pts = pts;
        }

        public Vector3 GetPoint(float t) {
            float omt = 1f-t;
            float omt2 = omt * omt;
            float t2 = t * t;
            return pts[0] * (omt2 * omt) +
                   pts[1] * (3f * omt2 * t) +
                   pts[2] * (3f * omt * t2) +
                   pts[3] * (t2 * t);
        }

        public Vector3 GetTangent(float t) {
            float omt = 1f-t;
            float omt2 = omt * omt;
            float t2 = t * t;
            Vector3 tangent =
                pts[0] * ( -omt2 ) +
                pts[1] * ( 3 * omt2 - 2 * omt ) +
                pts[2] * ( -3 * t2 + 2 * t ) +
                pts[3] * ( t2 );
            return tangent.normalized;
        }

        public Vector3 GetNormal2D(float t) {
            Vector3 tng = GetTangent( t );
            return new Vector3(-tng.y, tng.x, 0f);
        }

        public Vector3 GetNormal3D(float t, Vector3 up) {
            Vector3 tng = GetTangent( t );
            Vector3 binormal = Vector3.Cross( up, tng ).normalized;
            return Vector3.Cross(tng, binormal);
        }

        public Quaternion GetOrientation2D(float t) {
            Vector3 tng = GetTangent( t );
            Vector3 nrm = GetNormal2D( t );
            return Quaternion.LookRotation(tng, nrm);
        }

        public Quaternion GetOrientation3D(float t, Vector3 up) {
            Vector3 tng = GetTangent( t );
            Vector3 nrm = GetNormal3D( t, up );
            return Quaternion.LookRotation(tng, nrm);
        }

        // My code
        public OrientedPoint[] SampleOrientedPoints(float sampleStep) {
            int numSamples = Mathf.FloorToInt(1 / sampleStep) + 1;
            OrientedPoint[] samples = new OrientedPoint[numSamples];

            float t = 0;
            for (int i = 0; i < numSamples; i++) {
                samples[i] = new OrientedPoint(GetPoint(t), GetOrientation3D(t, Vector3.up));
                t += sampleStep;
            }

            return samples;
        }

    }

}

/*
References:
[1] J. Holmér, “A coder’s guide to spline-based procedural geometry,” 2015. 
    Available: https://docs.google.com/presentation/d/10XjxscVrm5LprOmG-VB2DltVyQ_QygD26N6XC2iap2A (Accessed 2025-10-14).

*/
