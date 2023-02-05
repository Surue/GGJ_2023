using UnityEngine;
using MiniTools.BetterGizmos;

public class DrawCardPosition : MonoBehaviour
{
    public float width = 1;
    public float height = 1.7f;
    public float depth = 0.25f;
    public float arrowSize = 0.5f;
    public Transform facingCard = null;

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(width, height, depth));

        if (facingCard)
        {
            BetterGizmos.DrawViewFacingArrow(Color.red, transform.position, facingCard.position, arrowSize);
        }
    }
#endif
}