using UnityEngine;
using UnityEngine.AI;

namespace OSG
{
    public static class NavigationExtensions{

		public static float GetLength(this NavMeshPath thePath, Vector3 startPosition){
			float distance = 0;
			foreach (Vector3 aPosition in thePath.corners){
				distance += (aPosition - startPosition).magnitude;
				startPosition = aPosition;
			}
			return distance;
		}
	}
}