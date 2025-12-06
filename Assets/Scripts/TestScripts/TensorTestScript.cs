using CityGenerator.FlowFields;
using Unity.Mathematics;
using UnityEngine;

public class TensorTestScript : MonoBehaviour {
    [SerializeField]
    [Range(0, 10)]
    float R = 0;

    [SerializeField]
    [Range(0.1f, 10)]
    float lambda = 0;

    [SerializeField]
    [Range(0, math.PI2)]
    float theta = 0;

    void Start() {
        TensorField field = new TensorField(10, 10);
    }

    void Update() {
        float2x2 matrix = CalcMatrix();

        float2 a = Vector2.up;
        float2 b = Vector2.right;
        float2 e = new Vector2(-1, 1);

        float2 f = CalcMajor();
        float2 g = CalcMinor();

        Debug.DrawRay(Vector3.zero, (Vector2)math.mul(matrix, a), Color.red);
        Debug.DrawRay(Vector3.zero, (Vector2)math.mul(b, matrix), Color.green);
        Debug.DrawRay(Vector3.zero, (Vector2)math.mul(e, matrix), Color.black);

        Debug.DrawRay(Vector3.zero, (Vector2)math.mul(f, matrix), Color.blue);
        Debug.DrawRay(Vector3.zero, (Vector2)math.mul(g, matrix), Color.yellow);

    }

    float2x2 CalcMatrix() {

        float theta2 = 2*theta;

        float2x2 matrix = R * new float2x2(
            math.cos(theta2), math.sin(theta2),
            math.sin(theta2), -math.cos(theta2)
        );

        return matrix;
    }

    float2 CalcMajor() {
        return new float2(math.cos(theta), math.sin(theta)) * lambda;
    }

    float2 CalcMinor() {
        return new float2(math.cos(theta + math.PIHALF), math.sin(theta + math.PIHALF)) * lambda;
    }
}

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
