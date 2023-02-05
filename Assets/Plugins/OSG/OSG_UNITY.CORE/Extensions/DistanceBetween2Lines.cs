// Old Skull Games
// Bernard Barthelemy
// Wednesday, May 24, 2017
using UnityEngine;

namespace OSG
{
    public struct DistanceBetween2Lines
    {
        //   Given the four vectors defining your points A1, A2, B1, and B2
        //   ComputeDistance determines
        //   - which points PA and PB on the lines A1A2 and B1B2, respectively, 
        //     are the closest together
        //   - what ratio FA, FB those points are on the segments (FA = 0 means FA is A1, FA = 1 means FA is A2)
        //   - returns the distance d between PA and PB

        public Vector3 A1;
        public Vector3 A2;
        public Vector3 B1;
        public Vector3 B2;

        public Vector3 PA;
        public Vector3 PB;
        public float FA;
        public float FB;

        public bool OnA
        {
            get { return FA >= 0 && FA <= 1.0f; }
        }

        public bool OnB
        {
            get { return FB >= 0 && FB <= 1.0f; }
        }

        public float LengthA
        {
            get { return (A2 - A1).magnitude; }
        }

        public float LengthB
        {
            get { return (B2 - B1).magnitude; }
        }

        public float ComputeDistance()
        {
            Vector3 U = A2 - A1;
            Vector3 V = B2 - B1;
            Vector3 W = Vector3.Cross(U, V);
            float dot = Vector3.Dot(W, W);
            FA = Vector3.Dot(Vector3.Cross(B1 - A1, V), W) / dot;
            PA = A1 + FA * U;
            FB = Vector3.Dot(Vector3.Cross(B1 - A1, U), W) / dot;
            PB = B1 + FB * V;
            return (PB - PA).magnitude;
        }
    }
}