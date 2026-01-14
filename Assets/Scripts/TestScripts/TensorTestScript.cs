using CityGenerator.FlowFields;
using CityGenerator.MeshUtilities;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(UniqueMesh))]
public class TensorTestScript : MonoBehaviour {

    [SerializeField]
    int xSize = 10, zSize = 10;

    [SerializeField]
    float angle = 0;

    [SerializeField]
    Transform[] splinePoints;
    [SerializeField]
    float sampleStep;

    [SerializeField]
    float decayConst = 0.0005f;

    [SerializeField]
    bool updateVisualisation = false;

    [SerializeField]
    Material visualierMaterial;

    [SerializeField]
    [Range(0, 5)]
    int basisField = 0;

    private TensorField field;
    private Mesh mesh;
    private Renderer rend;
    private Material visMatInstance;

    private List<OrientedPoint> debugPoints;

    void Start() {
        mesh = GetComponent<UniqueMesh>().Mesh;
        rend = GetComponent<Renderer>();

        MeshCreator.CreatePlane(mesh, xSize, zSize, 2, 2);

        updateVisualisation = true;

        visMatInstance = new Material(visualierMaterial);
        rend.material = visMatInstance;

        debugPoints = new List<OrientedPoint>();
    }

    void Update() {

        if (updateVisualisation) {

            field = new TensorField(xSize, zSize);
            MeshCreator.CreatePlane(mesh, xSize, zSize);
            field.decayConst = decayConst;

            float2 pos = new float2((xSize - 1) * 0.5f, (zSize - 1) * 0.5f);
            switch (basisField) {
                case 0:
                field.ApplyGridBasisField(pos, math.radians(angle), 2);
                break;
                case 1:
                field.ApplyCenterBasisField(pos);
                break;
                case 2:
                field.ApplyNodeBasisField(pos);
                break;
                case 3:
                field.ApplySaddleBasisField(pos);
                break;
                case 4:
                field.ApplyTrisectorBasisField(pos);
                break;
                case 5: {
                    if (splinePoints.Length != 4) {
                        break;
                    }

                    BezierCurve curve = new BezierCurve( splinePoints.Select(t => t.position).ToArray() );
                    OrientedPoint[] points = curve.SampleOrientedPoints(sampleStep);

                    debugPoints.Clear();
                    debugPoints.AddRange(points);

                    field.ApplyBoundryField(points);
                }
                break;
            }
            DebugFlow();
            field.Visualise(visMatInstance);

            updateVisualisation = false;
        }

        if (basisField == 5) {
            foreach (OrientedPoint point in debugPoints) {
                Debug.DrawRay(point.position, point.LocalToWorldDirection(Vector3.right), Color.orange);
                Debug.DrawRay(point.position, Vector3.up);
            }
        }
    }

    void DebugFlow() {
        for (int i = 0; i < xSize; i++) {
            for (int j = 0; j < zSize; j++) {
                Vector3 pos = new Vector3(i + 0.5f, 0.25f, j + 0.5f);

                float2 major = Tensor.getMajorEigenVector(field[i, j]);
                float2 minor = Tensor.getMinorEigenVector(field[i, j]);

                Vector3 majorVisOff = new Vector3(major.x, 0, major.y) * 0.6f;
                Vector3 minorVisOff = new Vector3(minor.x, 0, minor.y) * 0.3f;
                Debug.DrawLine(pos + majorVisOff, pos - majorVisOff, Color.green, 300);
                Debug.DrawLine(pos + minorVisOff, pos - minorVisOff, Color.blue, 300);
            }
        }
    }

    // [4]
    void OnDestroy() {
        Destroy(visMatInstance);
    }

}

/*

https://stackoverflow.com/questions/65661496/how-to-get-vertex-colors-working-in-unity-project
https://catlikecoding.com/unity/tutorials/procedural-grid/
https://stackoverflow.com/questions/75737648/is-it-possible-to-add-a-render-texture-to-a-gameobject-through-a-script
[4] https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Renderer-material.html
*/

/*
in unity c# how do I apply a float2x2 (matrix) to a float2 (vector)?

In Unity C#, you can multiply a **float2x2 matrix** by a **float2 vector** directly — but only if you’re using the **Unity.Mathematics** package (commonly used in DOTS/ECS or Jobs).

### ✔️ If you're using **Unity.Mathematics**

`float2x2` already defines the `*` operator for `float2`, so you can multiply them directly:

```csharp
using Unity.Mathematics;

float2x2 m = new float2x2(
    1, 2,
    3, 4
);

float2 v = new float2(5, 6);

float2 result = math.mul(m, v);
// or simply: float2 result = m * v;
```

### Note on order:

Unity.Mathematics uses **column-major** matrices (like HLSL), so:

```
float2x2(
    a, b,
    c, d
)

corresponds to matrix:
| a  b |
| c  d |
```

The multiplication is:

```
result.x = a * v.x + b * v.y
result.y = c * v.x + d * v.y
```

### ✔️ Output

If `m = [ [1,2], [3,4] ]` and `v = (5,6)`:

```
result = (17, 39)
```

---

### ✔️ If you're *not* using Unity.Mathematics

Unity’s regular built-in math library doesn't have float2 or float2x2, so you must either:

1. Install the **Unity.Mathematics** package
   (recommended), or
2. Implement your own struct/multiplication.

---

If you want, I can show you how to implement a custom 2×2 matrix multiply, or how to adapt this for shaders (HLSL) or Burst Jobs.

*/
