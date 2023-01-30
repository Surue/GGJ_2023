using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandController : MonoBehaviour
{
    // --- PUBLIC ---
    [Header("LINE PARAMETERS")]
    public GameObject lineIcon;
    public float lineIconOffset = -0.4f;
    public float offsetYCurve = 1f;
    public int dotPerUnit = 1;
    public Color lineColorDeplacement;
    public Color lineColorAttack;
    public Color lineColorNeutral;
    [Space]
    [Space]

    // --- PUBLIC DEBUG ---
    [Header ("DEBUG")]
    // Card recup
    public CardController cardController;
    public CardController slotCardController;
    public CardController targetCardController;
    //public CardVisualBehaviour cardVisualBehaviour;
    // Board recup
    [Space]
    public GameObject boardSlot;
    public BoardController boardController;
    [Space]
    public string debugRaycast;

    // --- PRIVATE ---
    private Transform cardTransform;
    private Ray ray;
    private RaycastHit hit;
    private LineRenderer lineRenderer;


    public enum HandState
    {
        free, cardSelectedOnBoard, cardSelectedInHand, waitingTurn
    }
    public HandState currentHandState = HandState.free;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        CheckRaycastHit();
        debugRaycast = CheckRaycastHit();

        switch (currentHandState)
        {
            case HandState.free:
                FreeState();
                break;
            case HandState.cardSelectedOnBoard:
                CardSelectedState();
                break;
            case HandState.cardSelectedInHand:
                CardInvokeOnDesk();
                break;
            case HandState.waitingTurn:

                break;
            default:
                break;
        }
    }

    #region HAND STATES
    public void FreeState()
    {
        //CHECK LES CARTES EN MAIN
        if (CheckRaycastHit() == "Card")
        {
            if (cardController == null)
            {
                //Récupère le controller et le visuel de la carte override
                cardController = hit.transform.GetComponent<CardController>();
                cardController.CardStateSwitch(CardController.CardState.isOverride);
            }
        }
        else if (cardController != null)
        {
            cardController.CardStateSwitch(CardController.CardState.inHand);
            cardController = null;
        }
        else
        {
            cardController = null;
        }

        if (Input.GetMouseButtonDown(0) & cardController != null)
        {
            //Si le joueur clic sur une card dans la main qui est interactible passe dans la zone de selection (en attente d'être posée)
            if (cardController.isInteractible)
            {
                cardController.CardStateSwitch(CardController.CardState.isWaiting);
                cardController.PlayAnimationCard("ActiveAnim");
                currentHandState = HandState.cardSelectedInHand;

                // [TEST] pour draw line
                cardTransform = hit.transform.GetComponent<Transform>();
            }
        }

        //CHECK LES CARTES SUR LE PLATEAU
        if (CheckRaycastHit() == "Slot")
        {
            boardController = hit.transform.GetComponent<BoardController>();

            if (boardController.containCard is true)
            {
                slotCardController = boardController.cardController;

                //Si le joueur clic sur un slot qui contient un carte qui est interactible celle ci est est séléctionné
                if (Input.GetMouseButtonDown(0) & slotCardController.isInteractible)
                {
                    slotCardController.moveToPositon = slotCardController.transform.localPosition + Vector3.up * 0.25f;
                    slotCardController.CardStateSwitch(CardController.CardState.isSelected);
                    slotCardController.PlayAnimationCard("ActiveAnim");
                    currentHandState = HandState.cardSelectedOnBoard;

                    // [TEST] pour draw line
                    cardTransform = hit.transform.GetComponent<Transform>();
                }
            }
        }
    }

    //Fonction qui gère l'invocation d'une carte sur le terrain depuis la main
    public void CardInvokeOnDesk()
    {
        if (CheckRaycastHit() == "Slot")
        {
            //Récupère le Slot detecté et son controller
            boardSlot = hit.transform.gameObject;
            boardController = hit.transform.GetComponent<BoardController>();

            if (boardController.containCard is false)
            {
                // [TEST LINE] pour draw line si le joueur peut poser une carte
                DrawMovementLine(cardTransform.position, boardSlot.transform.position, offsetYCurve, lineColorDeplacement);

                if (Input.GetMouseButtonDown(0))
                {
                    //Dit a la carte d'enregistrer son slot actuel
                    cardController.UpdatePreviousSlot(boardController);
                    //Change l'état de la carte
                    cardController.CardStateSwitch(CardController.CardState.onDesk);
                    cardController.PlayAnimationCard("IdleAnim");

                    //Change l'état de la main
                    cardController = null;
                    currentHandState = HandState.free;

                    // [TEST LINE] Desactiver la line
                    lineRenderer.enabled = false;
                    lineIcon.SetActive(false);
                }
            }
            else
            {
                DrawMovementLine(cardTransform.position, hit.point, offsetYCurve, lineColorNeutral);
            }
        }
        else
        {
            //Si il détecte un autre collider il renvoie rien (donc il faut un collider pour le plateau)
            boardSlot = null;
            boardController = null;

            // [TEST LINE]
            DrawMovementLine(cardTransform.position, hit.point, offsetYCurve, lineColorNeutral);
            //lineRenderer.enabled = false;
        }
    }

    public void CardSelectedState()
    {
        // BOUGER UNE CARTE
        if (CheckRaycastHit() == "Slot")
        {
            //Récupère le Slot detecté et son controller
            boardSlot = hit.transform.gameObject;
            boardController = hit.transform.GetComponent<BoardController>();

            if (boardController.containCard is false)
            {
                // [TEST LINE] pour draw line si le joueur peut poser une carte
                DrawMovementLine(cardTransform.position, boardSlot.transform.position, offsetYCurve, lineColorDeplacement);

                if (Input.GetMouseButtonDown(0))
                {
                    //Change l'état de la main
                    currentHandState = HandState.free;

                    //Dit a la carte d'enregistrer son slot actuel
                    slotCardController.UpdatePreviousSlot(boardController);

                    //Change l'état de la carte
                    slotCardController.CardStateSwitch(CardController.CardState.onDesk);
                    slotCardController.PlayAnimationCard("IdleAnim");

                    // [TEST LINE] Desactiver la line
                    lineRenderer.enabled = false;
                    lineIcon.SetActive(false);
                }
            }

            // INVERSION DE DEUX CARTES
            if (boardController.containCard is true)
            {
                targetCardController = boardController.cardController;
                DrawMovementLine(cardTransform.position, targetCardController.transform.position, offsetYCurve, lineColorDeplacement);

                if (Input.GetMouseButtonDown(0) & targetCardController.canMove is true)
                {
                    //Repasse la main en free
                    currentHandState = HandState.free;

                    //Envoie la carte selectionné sur un emplacement déjà pris
                    slotCardController.UpdatePreviousSlot(targetCardController.boardController);
                    targetCardController.UpdatePreviousSlot(slotCardController.previousBoardController);
                    targetCardController.moveJumpHeight = 0.15f;
                    slotCardController.moveJumpHeight = 0.5f;
                    slotCardController.CardStateSwitch(CardController.CardState.onDesk);
                    slotCardController.PlayAnimationCard("IdleAnim");

                    //Envoie la carte visée à l'ancien emplacement de la carte selectionné

                    targetCardController.CardStateSwitch(CardController.CardState.onDesk);

                    //[TEST LINE] Desactiver la line
                    lineRenderer.enabled = false;
                    lineIcon.SetActive(false);
                }
            }
        }
        else
        {
            //Si il détecte un autre collider il renvoie rien (donc il faut un collider pour le plateau)
            boardSlot = null;
            boardController = null;

            // [TEST LINE]
            DrawMovementLine(cardTransform.position, hit.point, offsetYCurve, lineColorNeutral);
            //lineRenderer.enabled = false;
        }

        //[TEST] DE DEGATS SUR LA CARTE SELECT
        if (Input.GetMouseButtonDown(1))
        {
            slotCardController.CardTakeDamage(1);
            slotCardController.PlayAnimationCard("TakeDamageAnim");
            slotCardController.PlayAnimationQueuedCard("ActiveAnim");
        }
    }

    #endregion

    //Fonction qui permet de draw une ligne entre deux position avec un Offset en Y
    void DrawMovementLine(Vector3 startPos, Vector3 endPos, float offsetY, Color lineColor)
    {
        //Modifie la couleur du line Renderer
        lineRenderer.material.SetColor("_DotColor", lineColor);
        //Active le line Renderer
        lineRenderer.enabled = true;
        //Set le nombre de points du line renderer
        lineRenderer.positionCount = 15;

        //Récupère la distance et change le tiling des dots en fonction
        float distanceBetween = Vector3.Distance(startPos, endPos);
        lineRenderer.material.SetFloat("_Tiling", distanceBetween * dotPerUnit);

        //Récupère le point du milieu et applique un offset
        Vector3 midPoint = (startPos + endPos) / 2;
        midPoint.y += offsetY;

        //Fix l'icon de la line au milieu de la courbe
        lineIcon.SetActive(true);
        lineIcon.transform.position = midPoint - Vector3.down*lineIconOffset;

        //Fonction de courbe de bezier
        float t = 0f;
        Vector3 B = new Vector3(0, 0, 0);
        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            B = (1 - t) * (1 - t) * startPos + 2 * (1 - t) * t * midPoint + t * t * endPos;
            lineRenderer.SetPosition(i, B);
            t += (1 / (float)lineRenderer.positionCount);
        }
    }

    //Fonction qui Raycast depuis la camera vers le pointeur de la souris et renvoie le tag de l'objet avec lequel il collide
    public string CheckRaycastHit()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            return hit.collider.tag;
        }
        else
        {
            return null;
        }

    }
}
