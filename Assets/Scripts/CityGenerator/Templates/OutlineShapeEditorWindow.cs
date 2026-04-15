using CityGenerator.MeshUtilities;
using UnityEditor;
using UnityEngine;

namespace CityGenerator.Templates {
    // based off [1]
    class OutlineShapeEditorWindow : EditorWindow {
        OutlineShape outlineShape;

        // Create a simple Editor Window to modify the high score in the ScriptableObject
        // Accessible at Window > City Generator > Template Editor
        [MenuItem("Window/City Generator/Outline Shape Editor")]
        public static void ShowWindow() {
            GetWindow<OutlineShapeEditorWindow>("Outline Shape Editor");
        }

        void OnGUI() {
            outlineShape = EditorGUILayout.ObjectField("Outline Shape", outlineShape, typeof(OutlineShape), false) as OutlineShape;
            if (outlineShape == null) return;

            int numPoints = (outlineShape.points is null) ? 0 : outlineShape.points.Length;
            EditorGUILayout.LabelField("Outline's number of points: ", numPoints.ToString());

            // Click the Increase High Score button to increase the high score by 10
            if (GUILayout.Button("Setup outline shape.")) {
                // Do work

                /*

                    O=========O
                    |         |
                    O         O
                     \       /  
                      O-----O

                */
                outlineShape.Setup(
                    new Vector2[] {
                        new(1, 0.1f), new(-1, 0.1f), // top edge
                        new(-1, 0.1f), new(-1, -0.3f), new(-0.75f, -0.5f), // left edge
                        new(-0.75f, -0.5f), new(0.75f, -0.5f), // bottom edge
                        new(0.75f, -0.5f), new(1, -0.3f), new(1, 0.1f), // right edge
                    },
                    new Vector2[] {
                        new(0, 1), new(0, 1), // top edge
                        new(-1, 0), new(-0.89f, -0.45f), new(-0.45f, -0.89f), // left edge
                        new(0, -1), new(0, -1), // bottom edge
                        new(0.45f, -0.89f), new(0.89f, -0.45f), new(1, 0), // right edge
                    },
                    new float[] {
                        0.0f, 0.5f, // top edge (road)
                        0.5f, 0.57f, 0.62f, // left edge (concrete)
                        0.62f, 0.88f, // bottom edge (concrete)
                        0.88f, 0.93f, 1.0f // right edge (concrete)
                    },
                    new int[] {
                        0,1, // top
                        2,3, 3,4, // left
                        5,6, // bottom
                        7,8, 8,9 // right
                    }
                );

                // Call SetDirty to ensure the change is saved
                EditorUtility.SetDirty(outlineShape);
                AssetDatabase.SaveAssets(); // To save immediately
            }

        }
    }
}
/*
References:

[1] https://docs.unity3d.com/6000.3/Documentation/Manual/class-ScriptableObject.html

*/
