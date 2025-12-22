using CityGenerator.FlowFields;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class TensorTestScript : MonoBehaviour {

    [SerializeField]
    int xSize = 10, zSize = 10;

    [SerializeField]
    float decayConst = 0.0005f;

    [SerializeField]
    int numPasses = 1;

    [SerializeField]
    bool updateMesh = false;

    [SerializeField]
    Material visualierMaterial;

    private TensorField field;
    //private Mesh mesh;
    private Renderer rend;
    private RenderTexture rendText;

    void Start() {
        //mesh = GetComponent<UniqueMesh>().Mesh;
        rend = GetComponent<Renderer>();

        updateMesh = true;

        // [3]
        rendText = new RenderTexture(512, 512, 16, RenderTextureFormat.ARGB32);
        rendText.useDynamicScale = true;
        rendText.Create();
        rend.sharedMaterial.mainTexture = rendText;
    }

    void Update() {

        if (updateMesh) {

            field = new TensorField(xSize, zSize);
            field.decayConst = decayConst;
            // field.ApplyGridBasisField(new float2((xSize - 1) * 0.75f, (zSize - 1) * 0.75f), math.radians(60), 2);
            // field.ApplyGridBasisField(new float2((xSize - 1) * 0.25f, (zSize - 1) * 0.25f), math.radians(30), 2);
            field.ApplyCenterBasisField(new float2((xSize - 1) * 0.25f, (zSize - 1) * 0.25f));
            field.ApplyNodeBasisField(new float2((xSize - 1) * 0.75f, (zSize - 1) * 0.75f));
            field.ApplySaddleBasisField(new float2((xSize - 1) * 0.25f, (zSize - 1) * 0.75f));
            field.ApplyTrisectorBasisField(new float2((xSize - 1) * 0.75f, (zSize - 1) * 0.25f));
            DebugFlow();
            field.Visualise(rendText, visualierMaterial, numPasses);

            updateMesh = false;
        }
    }

    void DebugFlow() {
        for (int i = 0; i < xSize; i++) {
            for (int j = 0; j < zSize; j++) {
                Vector3 vertex = new Vector3(i, 0, j);

                float2 major = Tensor.getMajorEigenVector(field[i, j]);
                float2 minor = Tensor.getMinorEigenVector(field[i, j]);
                //colors[i] = new Color(major.x * 0.5f + 0.5f, major.y * 0.5f + 0.5f, minor.x * 0.5f + 0.5f, minor.y * 0.5f + 0.5f); // map -1 to 1 to 0 to 1

                Vector3 majorVisOff = new Vector3(major.x, 0, major.y) * 0.6f;
                Vector3 minorVisOff = new Vector3(minor.x, 0, minor.y) * 0.3f;
                Debug.DrawLine(vertex + Vector3.up + majorVisOff, vertex + Vector3.up - majorVisOff, Color.black, 300);
                Debug.DrawLine(vertex + Vector3.up + minorVisOff, vertex + Vector3.up - minorVisOff, Color.blue, 300);
            }
        }
    }

    //void CreateMesh(Mesh mesh) {

    //    float uvScale = 1.0f / math.max(xSize, zSize);

    //    int vertCount = xSize * zSize;
    //    int triIndexCount = (xSize - 1) * (zSize - 1) * 6;

    //    Vector3[] vertices     = new Vector3[ vertCount ];
    //    int[] triangleIndices  = new int[ triIndexCount ];
    //    Vector3[] normals      = new Vector3[ vertCount ];
    //    Vector2[] uvs          = new Vector2[ vertCount ];
    //    Color[] colors         = new Color[vertices.Length];

    //    Vector2 centre = new float2((xSize - 1) * 0.5f, (zSize - 1) * 0.5f);
    //    float2x2 rot90 = new float2x2(
    //        0, 1,
    //        -1, 0
    //    );

    //    for (int i = 0, z = 0; z < zSize; z++) {
    //        for (int x = 0; x < xSize; x++, i++) {
    //            vertices[i] = new Vector3(x, 0, z);
    //            normals[i] = Vector3.up;
    //            uvs[i] = new Vector2((x * uvScale), (z * uvScale));

    //            float2 major = Tensor.getMajorEigenVector(field[x, z]);
    //            float2 minor = Tensor.getMinorEigenVector(field[x, z]);
    //            //colors[i] = new Color(major.x * 0.5f + 0.5f, major.y * 0.5f + 0.5f, minor.x * 0.5f + 0.5f, minor.y * 0.5f + 0.5f); // map -1 to 1 to 0 to 1

    //        }
    //    }

    //    for (int ti = 0, vi = 0, z = 0; z < zSize - 1; z++, vi++) {
    //        for (int x = 0; x < xSize - 1; x++, ti += 6, vi++) {
    //            triangleIndices[ti] = vi;
    //            triangleIndices[ti + 3] = triangleIndices[ti + 2] = vi + 1;
    //            triangleIndices[ti + 4] = triangleIndices[ti + 1] = vi + xSize;
    //            triangleIndices[ti + 5] = vi + xSize + 1;
    //        }
    //    }

    //    mesh.Clear();
    //    mesh.vertices = vertices;
    //    mesh.triangles = triangleIndices;
    //    mesh.normals = normals;
    //    mesh.uv = uvs;
    //    mesh.colors = colors;


    //}

}

/*

https://stackoverflow.com/questions/65661496/how-to-get-vertex-colors-working-in-unity-project
https://catlikecoding.com/unity/tutorials/procedural-grid/
[3] https://stackoverflow.com/questions/75737648/is-it-possible-to-add-a-render-texture-to-a-gameobject-through-a-script
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
