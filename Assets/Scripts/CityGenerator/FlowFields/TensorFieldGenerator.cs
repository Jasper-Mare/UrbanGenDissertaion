using Unity.Mathematics;
using Color = UnityEngine.Color;
using Debug = UnityEngine.Debug;
using Vector3 = UnityEngine.Vector3;

namespace CityGenerator.FlowFields {
    static class TensorFieldGenerator {

        public static TensorField Generate(float2 pos, float2 size, int2 numTensors, int iterations, float decayConstant, uint seed) {
            TensorField field = new TensorField(pos, size.x, size.y, numTensors.x, numTensors.y);
            field.decayConst = decayConstant;
            Random rng = new Random(seed);


            for (int i = 0; i < iterations; i++) {
                int opperation = rng.NextInt(4);
                float2 location = rng.NextFloat2(size) + pos;

                Debug.DrawRay(new float3(location.x, 0, location.y), Vector3.up * 30, Color.green, 30);

                switch (opperation) {
                    case 0:
                    field.ApplyCenterBasisField(location);
                    break;

                    case 1:
                    float angle = rng.NextFloat(-math.PI, math.PI);
                    float length = rng.NextFloat(0.1f, 2f);
                    field.ApplyGridBasisField(location, angle, length);
                    break;

                    case 2:
                    field.ApplyNodeBasisField(location);
                    break;

                    case 3:
                    field.ApplySaddleBasisField(location);
                    break;

                    case 4:
                    field.ApplyTrisectorBasisField(location);
                    break;
                }
            }

            return field;
        }

    }
}
