using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace CityGenerator.FlowFields {
    class TensorField {
        readonly public int Width, Height;
        float2x2[,] tensors;

        public TensorField(int w, int h) {
            Width = w;
            Height = h;
            tensors = new float2x2[w, h];

        }

        public float2x2 this[int x, int y] {
            get { return tensors[x, y]; }
        }

        // perhaps for parrallelism these apply methods could be turned into compute shaders? at least for large fields
        public void ApplyGridBasisField(float2 location, float2 direction, float length) {
            float theta = math.atan2(direction.x, direction.y);

            float sin = math.sin(2 * theta);
            float cos = math.cos(2 * theta);

            float2x2 basis = new float2x2(
                cos, sin,
                sin,-cos
            ) * length;

            for (int i = 0; i < Width; i++) {
                for (int j = 0; j < Height; j++) {
                    CombineTensor(i, j, basis, location);
                }
            }
        }

        public void ApplyRadialBasisField() {

        }

        public void ApplyBoundryField() {

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CombineTensor(int i, int j, float2x2 tensor, float2 location) {
            const float decayConst = 0.6f;

            // c^2 = a^2 + b^2
            float a = (location.x - i);
            float b = (location.y - j);
            float distSq = a*a + b*b;
            tensors[i, j] += tensor * math.exp(-decayConst * distSq);
        }
    }
}

/*
In "Interactive Procedural Street Modeling" by Chen et al they use a tensor field, how should these tensors and tensor fields be represented in code?

In **Chen et al., “Interactive Procedural Street Modeling”**, the *tensor field* refers to a **direction field** constructed from **2nd-order symmetric tensors** that encode preferred road orientations (e.g., orthogonal or multi-modal directions).
Nothing about this requires a special library—the tensors are small (2×2), symmetric, and easy to represent numerically.

Below is a concise guide for how to represent them in code and how to work with entire tensor fields.

---

# ✅ 1. What the tensors are (mathematically)

For a 2D tensor field they use **symmetric 2×2 matrices**:

[
T = \begin{bmatrix}
a & b \
b & c
\end{bmatrix}
]

This encodes:

* **Principal directions** (eigenvectors)
* **Importance** or “strength” of each direction (eigenvalues)
* **Undirected** orientation (a direction and its opposite are equivalent — ideal for streets)

---

# ✅ 2. How to represent a tensor in code

## **Option A: Store the full 2×2 matrix**

```python
import numpy as np

tensor = np.array([[a, b],
                   [b, c]], dtype=float)
```

Pros:

* Easy to apply linear algebra (eigenvectors)
* Matches the math in the paper

Cons:

* Redundant storage of symmetry (not a big issue)

---

## **Option B: Store the minimal 3 components**

Because the matrix is symmetric:

```python
class Tensor2:
    def __init__(self, a, b, c):
        self.a = a  # Txx
        self.b = b  # Txy = Tyx
        self.c = c  # Tyy

    def as_matrix(self):
        return np.array([[self.a, self.b],
                         [self.b, self.c]])
```

Pros:

* Clean and efficient
* Easy to store in images, grids, or GPU textures

This is how many orientation-field papers implement it.

---

## **Option C: Store as eigenvalues + angle**

A 2D symmetric tensor can be stored as:

```python
lambda1  # major axis strength
lambda2  # minor axis strength
theta    # angle of major axis
```

But the Chen paper specifically constructs **frame fields**, so the full tensor representation is more direct.

---

# ✅ 3. How to represent the entire tensor field

A **tensor field** is simply a **2D grid (image or mesh)** where each cell/node holds one tensor.

Examples:

## **On a regular grid**

```python
import numpy as np

H, W = 512, 512
tensor_field = np.zeros((H, W, 3))  # storing a,b,c

tensor_field[y, x] = [a, b, c]
```

## **On a mesh (per-vertex or per-face)**

```python
tensors = [Tensor2(a, b, c) for _ in mesh.vertices]
```

## **On the GPU**

Use a 3-channel floating texture storing `(a, b, c)`.

---

# ✅ 4. Computing directions from the tensor

Eigen decomposition gives the principal directions:

```python
def principal_directions(tensor):
    T = tensor.as_matrix()
    w, v = np.linalg.eigh(T)  # symmetric → eigh()
    # v[:,0] = minor direction, v[:,1] = major direction
    return w, v
```

This is exactly how roads follow orientations in Chen et al.

---

# ✅ 5. Summary Table

| Representation          | How              | Pros                          | Use case                              |
| ----------------------- | ---------------- | ----------------------------- | ------------------------------------- |
| **Full 2×2 matrix**     | `[[a, b],[b,c]]` | Most intuitive, matches paper | CPU computation, clarity              |
| **Compact (a,b,c)**     | 3 floats         | Efficient, GPU-friendly       | Real-time, large fields               |
| **Eigenvalues + angle** | `(λ1,λ2,θ)`      | Smallest storage              | If you only need direction/anisotropy |

For Chen et al., **Option B (a,b,c)** is the most common and practical representation.

---

# If you'd like…

I can also provide:

✅ code to construct the tensor field from user-sketches (as they do in the paper)
✅ code to visualize the frame field
✅ code to integrate streamlines to generate streets

Just tell me what you want next!






how do the basis Fields described in the paper turn into the final tensor field?


Below is the **core mechanism** described in *Interactive Procedural Street Modeling* (Chen et al. 2008) for turning **basis fields** into the **final tensor field**.
The paper does not provide explicit code, but the math is standard frame-field construction using **weighted combinations of symmetric 2×2 tensors**.

I’ll explain the pipeline clearly and concretely so you can implement it.

---

# ✔ What the paper calls “basis fields”

Basis fields are **user-sketched orientation constraints**:

* highway directions
* orthogonal grid directions
* radial directions
* ring patterns
* preferred vs suppressed directions

Each basis field defines orientation **at any point in the plane**, usually by a simple analytic formula.

---

# ✔ Each basis field is converted into a tensor field

Each basis field represents a **set of preferred directions**.
A direction θ is encoded as a **2nd order symmetric tensor**:

[
T(\theta) =
\begin{bmatrix}
\cos^2\theta & \cos\theta\sin\theta \
\cos\theta\sin\theta & \sin^2\theta
\end{bmatrix}
]

This tensor:

* encodes a **line direction** (θ ≡ θ + π)
* has eigenvalues (1, 0): a single principal direction

If a basis field has **two directions** (typical for grid patterns):

[
T = T(\theta_1) + T(\theta_2)
]

If the directions are orthogonal (θ and θ + π/2) this produces a tensor with two strong perpendicular principal directions → grid.

---

# ✔ The key: All basis tensors are **linearly blended using spatial weights**

Every basis field has a **weight function** wᵢ(p):

* falloff region
* masks for “influence areas”
* user-drawn brush strength

At any point p:

[
T_{\text{final}}(p) = \sum_i w_i(p) , T_i(p)
]

Where:

* Tᵢ(p) is the tensor produced by the basis field i
* wᵢ(p) is the weight (usually normalized so ∑ wᵢ = 1)

The result is still a **symmetric 2×2** tensor so all the usual operations (eigenvectors = street directions) work.

---

# ✔ Implementation in code

## Step 1 — Compute tensor from a direction

```python
import numpy as np

def tensor_from_angle(theta):
    c = np.cos(theta)
    s = np.sin(theta)
    return np.array([[c*c, c*s],
                     [c*s, s*s]])
```

## Step 2 — Compute tensor for each basis field

Example: A grid basis field has two directions:

```python
def grid_tensor(theta):
    return tensor_from_angle(theta) + tensor_from_angle(theta + np.pi/2)
```

Radial / ring fields compute θ from position:

```python
def radial_tensor(px, py, cx, cy):
    angle = np.arctan2(py - cy, px - cx)
    return tensor_from_angle(angle)
```

## Step 3 — Blend all basis fields

```python
def blended_tensor(p, basis_fields):
    T = np.zeros((2,2))
    weight_sum = 0.0

    for basis in basis_fields:
        w = basis.weight(p)
        T += w * basis.tensor(p)
        weight_sum += w

    return T / weight_sum if weight_sum > 0 else T
```

This gives a final tensor at every pixel, mesh vertex, etc.

---

# ✔ Why this works

* Symmetric 2×2 tensors **add linearly**, preserving symmetry.
* Weighted sums let user-sketched regions smoothly merge.
* Eigenvectors of the result give the actual street directions.
* The paper then traces curves along these directions → roads.

---

# ✔ Visual Summary

At each point **p**:

```
   [basis field 1] → T1(p) --\
   [basis field 2] → T2(p) ---- weighted sum → Final T(p)
   [basis field 3] → T3(p) --/
```

Final T(p) encodes the *local preferred street orientations*.

---

# If you want:

I can provide:

✅ Full working Python demo creating a tensor field
✅ Visualization code to draw a 2D frame field
✅ Complete road-tracing algorithm à la Chen et al.

Just tell me!

*/
