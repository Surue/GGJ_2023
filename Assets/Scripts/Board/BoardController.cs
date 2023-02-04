using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniTools.BetterGizmos;

public class BoardController : MonoBehaviour
{
    [Header("PARAMETERS")]
    public bool containCard = false;
    public GameObject highlight;
    [SerializeField] private EPlayerType playerType;
    public EPlayerType PlayerType => playerType;

    [Header ("DEBUG COLLIDER")]
    public float arrowSize = 0.5f;
    public Vector3 slotSize;
    public Transform facingCard = null;

    // --- PRIVATE ---
    public CardController cardController;

    public EBoardLineType boardLineType;
    public int columnID;
    public int slotID;
    
    public void Setup(int index)
    {
        slotID = index;
        if (index < 4)
        {
            boardLineType = EBoardLineType.Front;
        }
        else
        {
            boardLineType = EBoardLineType.Back;
        }

        columnID = index % 4;
    }
    
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
