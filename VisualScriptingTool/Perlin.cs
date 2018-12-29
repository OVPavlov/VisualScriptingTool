using UnityEngine;

namespace NodeEditor
{
    public static class Perlin
    {
        static Vector2[] _vectors;
        const int VectorsCount = 1753;

        static Perlin()
        {
            Random.InitState(7852384);
            _vectors = new Vector2[VectorsCount];
            for (int i = 0; i < VectorsCount; i++)
                _vectors[i] = Random.insideUnitCircle.normalized;

        }


        public static float Get(float x, float y, int repeat, int seed)
        {
            int ix0 = (int)x;
            int iy0 = (int)y;
            int ix1 = ix0 + 1;
            int iy1 = iy0 + 1;
            float fx0 = x - ix0;
            float fy0 = y - iy0;
            float fx1 = fx0 - 1f;
            float fy1 = fy0 - 1f;
            float xs = InterpHermiteFunc(fx0);
            float ys = InterpHermiteFunc(fy0);


            ix0 %= repeat;
            iy0 %= repeat;
            ix1 %= repeat;
            iy1 %= repeat;



            float p00 = GradCoord(ix0, iy0, fx0, fy0, seed);
            float p10 = GradCoord(ix1, iy0, fx1, fy0, seed);
            float p01 = GradCoord(ix0, iy1, fx0, fy1, seed);
            float p11 = GradCoord(ix1, iy1, fx1, fy1, seed);


            float l0 = Mathf.LerpUnclamped(p00, p10, xs);
            float l1 = Mathf.LerpUnclamped(p01, p11, xs);

            return Mathf.LerpUnclamped(l0, l1, ys);
        }

        static float GradCoord(int x, int y, float dx, float dy, int seed)
        {
            int i = (x ^ (y << 7)) ^ seed;
            i &= 2147483647;//eliminate negative
            Vector2 vector = _vectors[i % VectorsCount];
            return vector.x * dx + vector.y * dy;
        }

        static float InterpHermiteFunc(float t)
        {
            return t * t * (3 - 2 * t);
        }

        static float InterpQuinticFunc(float t)
        {
            return t * t * t * (t * (t * 6 - 15) + 10);
        }





        public static float FBM(float x, float y, int octaves, int lacunarity, float gain, int repeat, int seed)
        {
            float sum = Get(x, y, repeat, seed);
            float amp = 1;

            float shift = 0.5f;
            for (int i = 1; i < octaves; i++)
            {
                x *= lacunarity;
                y *= lacunarity;
                x += shift;
                y += shift;
                shift *= 0.5f;
                repeat *= lacunarity;

                amp *= gain;
                sum += Get(x, y, repeat, ++seed) * amp;
            }

            return sum;
        }

        public static float Billow(float x, float y, int octaves, int lacunarity, float gain, int repeat, int seed)
        {
            float sum = Mathf.Abs(Get(x, y, repeat, seed)) * 2 - 1;
            float amp = 1;

            for (int i = 1; i < octaves; i++)
            {
                x *= lacunarity;
                y *= lacunarity;
                repeat *= lacunarity;

                amp *= gain;
                sum += (Mathf.Abs(Get(x, y, repeat, +seed)) * 2 - 1) * amp;
            }

            return sum;
        }

        public static float RigidMulti(float x, float y, int octaves, int lacunarity, float gain, int repeat, int seed)
        {
            float sum = 1 - Mathf.Abs(Get(x, y, repeat, seed));
            float amp = 1;

            float shift = 0.5f;
            for (int i = 1; i < octaves; i++)
            {
                x *= lacunarity;
                y *= lacunarity;
                x += shift;
                y += shift;
                shift *= 0.5f;
                repeat *= lacunarity;

                amp *= gain;
                sum -= (1 - Mathf.Abs(Get(x, y, repeat, ++seed))) * amp;
            }

            return sum;
        }





    }
}


