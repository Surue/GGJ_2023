using System.Collections.Generic;
using OSG;
using UnityEngine;

public class ChildrenActivation : OSGMono
{
	[SerializeField][HideInInspector] public List<string> objectsToActivate;
}
