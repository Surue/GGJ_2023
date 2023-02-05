// Old Skull Games
// Benoit Constantin
// Thursday, February 28, 2019

using UnityEngine;
using System;

namespace OSG
{
    public class EditorBorderedFunctionalCurveDrawer : EditorBorderedCurveDrawer<float>
    {
        int graphPointCount = 500;
        public int GraphPointCount
        { get { return graphPointCount; }
            set {
                graphPointCount = value;
                ForceReSample();
            }
        }

        Func<float, float> f;
        public Func<float,float> F
        {
            get { return f; }
            set {
                f = value;
                ForceReSample();
            }
        }

        public override void GetSampledInputs(ref float[] sampledInputs, float startXGraphView, float graphViewLength)
        {
            float step = graphViewLength / (graphPointCount);

            if (sampledInputs.Length != graphPointCount)
                sampledInputs = new float[graphPointCount];

            for (int i = 0; i < sampledInputs.Length; i++)
            {
                sampledInputs[i] = startXGraphView + i * step;
            }
        }

        protected override Vector2 Sample(float input)
        {
            return new Vector2(input, f(input));
        }

    }
}
