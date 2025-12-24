using CityGenerator.FlowFields;
using CityGenerator.MeshUtilities;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(UniqueMesh))]
public class TensorTestScript : MonoBehaviour {

    [SerializeField]
    int xSize = 10, zSize = 10;

    [SerializeField]
    float decayConst = 0.0005f;

    [SerializeField]
    int numPasses = 1;

    [SerializeField]
    bool updateVisualisation = false;

    [SerializeField]
    Material visualierMaterial;

    private TensorField field;
    private Mesh mesh;
    private Renderer rend;
    private RenderTexture rendText;
    private Material displayMatInstance;
    [SerializeField]
    private Material visMatInstance;

    void Start() {
        mesh = GetComponent<UniqueMesh>().Mesh;
        rend = GetComponent<Renderer>();

        MeshCreator.CreatePlane(mesh, xSize, zSize);

        updateVisualisation = true;

        // [3]
        rendText = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB32);
        rendText.Create();
        (displayMatInstance = rend.material).mainTexture = rendText;

        visMatInstance = new Material(visualierMaterial);
    }

    void Update() {

        if (updateVisualisation) {

            field = new TensorField(xSize, zSize);
            field.decayConst = decayConst;
            // field.ApplyGridBasisField(new float2((xSize - 1) * 0.75f, (zSize - 1) * 0.75f), math.radians(60), 2);
            // field.ApplyGridBasisField(new float2((xSize - 1) * 0.25f, (zSize - 1) * 0.25f), math.radians(30), 2);
            field.ApplyCenterBasisField(new float2((xSize - 1) * 0.25f, (zSize - 1) * 0.25f));
            field.ApplyNodeBasisField(new float2((xSize - 1) * 0.75f, (zSize - 1) * 0.75f));
            field.ApplySaddleBasisField(new float2((xSize - 1) * 0.25f, (zSize - 1) * 0.75f));
            field.ApplyTrisectorBasisField(new float2((xSize - 1) * 0.75f, (zSize - 1) * 0.25f));
            DebugFlow();
            field.Visualise(rendText, visMatInstance, numPasses);

            updateVisualisation = false;
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
        Destroy(displayMatInstance);
        Destroy(visMatInstance);
        rendText.Release();
        Destroy(rendText);
    }

}

/*

https://stackoverflow.com/questions/65661496/how-to-get-vertex-colors-working-in-unity-project
https://catlikecoding.com/unity/tutorials/procedural-grid/
[3] https://stackoverflow.com/questions/75737648/is-it-possible-to-add-a-render-texture-to-a-gameobject-through-a-script
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
