using Unity.Mathematics;

namespace CityGenerator.FlowFields {
    static class Tensor {

        /*
        | A  B |
        | B -A |

        theta = 1/2 * atan2(B, A)
        */
        public static float getAngle(float2x2 tensor) {
            return 0.5f * math.atan2(tensor.c1.x, tensor.c0.x);
        }

        public static float2 getMajorEigenVector(float2x2 tensor) {
            float t = getAngle(tensor);
            return new float2(math.cos(t), math.sin(t));

        }

        public static float2 getMinorEigenVector(float2x2 tensor) {
            float t = getAngle(tensor) + math.PIHALF; // PIHALF can be changed to be altered by a rotation field later
            return new float2(math.cos(t), math.sin(t));
        }

    }
}

/*
References:
[1] G. Chen, G. Esch, P. Wonka, P. M¨uller, and E. Zhang, “Interactive procedural street modeling,”
    in ACM SIGGRAPH 2008 papers, ser. SIGGRAPH ’08. New York, NY, USA: Association for
    Computing Machinery, Aug. 2008, pp. 1–10. DOI:10.1145/1399504.1360702

[2] https://www.3blue1brown.com/lessons/quick-eigen (Accessed 06/12/2025)

*/

/*
Conversation with ChatGPT:

In the paper "Chen, G., Esch, G., Wonka, P., Müller, P., Zhang, E. 2008. Interactive Procedural Street Modeling" they define a tensor as " R \left(\matrix{\cos2\theta & \sin2\theta \cr \sin2\theta & -\cos2\theta}\right) " and the major eigenvector of this tensor as " \left\{\lambda\left(\matrix{\cos\theta \cr \sin\theta}\right)\bigg|\; \lambda \neq 0 \right\} " how is the eigenvector derived from the tensor?

Below is the full derivation. The key idea is that the tensor is built from a **2θ rotation**, so its eigenvectors naturally appear at angle **θ**.

---

# **Given tensor**

[
T = R
\begin{pmatrix}
\cos 2\theta & \sin 2\theta \
\sin 2\theta & -\cos 2\theta
\end{pmatrix}
]

Since the scalar (R) does not affect eigenvectors, we can ignore it and compute eigenvectors of

[
A=
\begin{pmatrix}
\cos 2\theta & \sin 2\theta \
\sin 2\theta & -\cos 2\theta
\end{pmatrix}.
]

---

# **Step 1: Compute eigenvalues**

Solve:

[
\det(A - \lambda I) = 0
]

[
\det\begin{pmatrix}
\cos 2\theta - \lambda & \sin 2\theta \
\sin 2\theta & -\cos 2\theta - \lambda
\end{pmatrix}=0.
]

Compute determinant:

[
(\cos 2\theta - \lambda)(-\cos 2\theta - \lambda) - \sin^2 2\theta = 0
]

Expand:

[
-(\cos^2 2\theta - \lambda^2) - \sin^2 2\theta = 0
]

[
-\cos^2 2\theta + \lambda^2 - \sin^2 2\theta = 0
]

[
\lambda^2 - (\cos^2 2\theta+\sin^2 2\theta)=0
]

Since (\cos^2 x + \sin^2 x = 1):

[
\lambda^2 -1 = 0  \quad\Rightarrow\quad \lambda = \pm 1.
]

So, the matrix has eigenvalues (+1) and (-1).

---

# **Step 2: Eigenvector for eigenvalue +1**

Solve:

[
A v = v \quad \Rightarrow\quad (A - I)v=0.
]

[
\begin{pmatrix}
\cos 2\theta -1 & \sin 2\theta \
\sin 2\theta & -\cos 2\theta -1
\end{pmatrix}
\begin{pmatrix} x \ y \end{pmatrix}
=0.
]

Use the first row:

[
(\cos 2\theta -1)x + \sin 2\theta , y = 0.
]

Use trig identities:

[
\cos 2\theta - 1 = -2\sin^2\theta,\qquad \sin 2\theta = 2\sin\theta\cos\theta.
]

Substitute them:

[
(-2\sin^2\theta)x + 2\sin\theta\cos\theta, y = 0.
]

Divide by (2\sin\theta) (assuming nondegenerate tensor):

[
-\sin\theta, x + \cos\theta, y = 0.
]

Thus:

[
y = (\tan\theta) x.
]

So the eigenvector is:

[
v = \begin{pmatrix} x \ x \tan\theta \end{pmatrix}
\propto \begin{pmatrix} \cos\theta \ \sin\theta \end{pmatrix}.
]

This matches exactly the result in the paper:

[
\left{\lambda
\begin{pmatrix}
\cos\theta \
\sin\theta
\end{pmatrix}
\Big|\ \lambda \neq 0\right}.
]

---

# **Interpretation**

The tensor is constructed using **2θ**, but the principal direction (the “major eigenvector”) corresponds to the direction **θ**, because orientation fields in procedural street modeling are **π-periodic**, not (2\pi)-periodic. The tensor doubles the angle so opposite directions collapse into one (streets have direction but not orientation), and the eigenvector extraction recovers the angle θ.

---

# **Summary**

1. Compute eigenvalues: (\lambda = \pm 1).
2. Solve eigenvector equation for (\lambda=1).
3. Use trig identities to reduce the system.
4. Resulting eigenvector is proportional to:

[
(\cos\theta, \sin\theta).
]

---

If you'd like, I can also show the geometric meaning of this tensor or how it relates to line fields and street network generation.

In the paper multiple tensors are summed together, how can the eigenvector of the tensor resulting from this sum be derived?

When multiple tensors of the form

[
T_i = R_i
\begin{pmatrix}
\cos 2\theta_i & \sin 2\theta_i \
\sin 2\theta_i & -\cos 2\theta_i
\end{pmatrix}
]

are summed,

[
T = \sum_i T_i,
]

**you do *not* combine their eigenvectors directly**.
Instead, you compute the eigenvector of the *resulting matrix* (T) the usual way—by finding the principal eigenvector of the 2×2 matrix.

But because each (T_i) is a “line-field tensor,” the resulting tensor keeps the special form that makes the computation simple.

---

# ✔ **Key fact: The sum of these tensors is still a real symmetric 2×2 matrix**

Each (T_i) is symmetric, so

[
T = \sum_i T_i
==============

\begin{pmatrix}
a & b \
b & c
\end{pmatrix},
]

where

[
a = \sum_i R_i \cos 2\theta_i,\quad
b = \sum_i R_i \sin 2\theta_i,\quad
c = -a.
]

This gives:

[
T =
\begin{pmatrix}
a & b \
b & -a
\end{pmatrix}.
]

---

# ✔ **Principal eigenvector of a general tensor of this form**

Let

[
T=
\begin{pmatrix}
a & b \
b & -a
\end{pmatrix}.
]

The eigenvalues solve:

[
\lambda^2 = a^2 + b^2
\quad\Rightarrow\quad
\lambda = \pm \sqrt{a^2+b^2}.
]

The **principal eigenvector** for the positive eigenvalue satisfies:

[
T v = \lambda v.
]

Solve using the first row:

[
a x + b y = \lambda x.
]

[
b y = (\lambda - a)x.
]

Thus:

[
\frac{y}{x} = \frac{\lambda - a}{b}.
]

But in directional-field form, it is much cleaner to rewrite (a) and (b) in polar coordinates:

[
a = R \cos 2\Theta,\qquad b = R \sin 2\Theta
]

with

[
R = \sqrt{a^2+b^2},\qquad \Theta = \frac{1}{2}\operatorname{atan2}(b,a).
]

Then the principal eigenvector is simply:

[
v =
\begin{pmatrix}
\cos \Theta \
\sin \Theta
\end{pmatrix}.
]

---

# ✔ Final result: **Eigenvector of a summed tensor**

Let

[
A = \sum_i R_i \cos 2\theta_i,\qquad
B = \sum_i R_i \sin 2\theta_i.
]

Then the resulting tensor is

[
T =
\begin{pmatrix}
A & B \
B & -A
\end{pmatrix}.
]

The principal direction angle is:

[
\boxed{\Theta = \tfrac12 \operatorname{atan2}(B, A)}.
]

And the major eigenvector is:

[
\boxed{
v =
\begin{pmatrix}
\cos\Theta \
\sin\Theta
\end{pmatrix}
=============

\begin{pmatrix}
\cos \left(\tfrac12\operatorname{atan2}(B,A)\right) \
\sin \left(\tfrac12\operatorname{atan2}(B,A)\right)
\end{pmatrix}.
}
]

---

# ✔ Interpretation

This is exactly like averaging orientation fields:

1. Convert each angle (\theta_i) to a unit vector in *doubled angle space*:
   ((\cos2\theta_i,, \sin2\theta_i)).
2. Sum them weighted by (R_i).
3. Convert back to an angle by halving the angle:
   (\Theta = \tfrac12\operatorname{atan2}(B,A)).

This avoids π-period ambiguity and gives the smooth street direction field used in the paper.

---

If you want, I can show a numerical example or derive the same result using complex numbers (a common trick in directional-field design).

*/
