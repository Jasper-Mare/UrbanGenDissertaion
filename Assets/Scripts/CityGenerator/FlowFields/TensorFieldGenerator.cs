using Unity.Mathematics;

namespace CityGenerator.FlowFields {
    static class TensorFieldGenerator {

        public static TensorField Generate(float2 pos, float2 size, int2 numTensors, int iterations) {
            TensorField field = new TensorField(pos, size.x, size.y, numTensors.x, numTensors.y);
            Random rng = new Random((uint)System.DateTime.Now.Millisecond);


            for (int i = 0; i < iterations; i++) {
                int opperation = rng.NextInt(4);
                float2 location = rng.NextFloat2(size);

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
