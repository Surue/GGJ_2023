using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniTools.BetterGizmos;

public class BoardController : MonoBehaviour
{
    [Header("PARAMETERS")]
    public bool containCard = false;
    public GameObject highlight;

    [Header ("DEBUG COLLIDER")]
    public float arrowSize = 0.5f;
    public Vector3 slotSize;
    public Transform facingCard = null;

    // --- PRIVATE ---
    public CardController cardController;

    private void Update()
    {

    }

    void OnDrawGizmosSelected()
    {
        // dessin de l'overlapBox pour faciliter la visualisation
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, slotSize);

        if (facingCard)
        {
            BetterGizmos.DrawViewFacingArrow(Color.red, transform.position, facingCard.position, arrowSize);
        }
    }
}
