using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanPlayer : Player
{
    private bool _isPlaying;
    
    // Active card
    private CardController cardController;

    // Drawing
    [Header("LINE PARAMETERS")]
    [SerializeField] private GameObject _lineIcon;
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private float _lineIconOffset = -0.4f;
    [SerializeField] private float _offsetYCurve = 1f;
    [SerializeField] private int _dotPerUnit = 1;
    [SerializeField] private Color _lineColorDisplacement;
    [SerializeField] private Color _lineColorAttack;
    [SerializeField] private Color _lineColorNeutral;
    
    // Raycast
    private RaycastHit _hit;
    private Transform _cardTransform;
    
    // Board
    private GameObject _boardSlot;
    private BoardController _boardController;
    private CardController _slotCardController;
    private CardController _targetCardController;

    private void Start()
    {
        GameManager.Instance.onHumanTurnStarted += StartTurn;
        GameManager.Instance.onHumanTurnFinished += EndTurn;
        _isPlaying = false;
    }

    private void Update()
    {
        if (!_isPlaying) return;

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
    
    public void FreeState()
    {
        //CHECK LES CARTES EN MAIN
        if (CheckRaycastHit() == "Card")
        {
            if (cardController == null)
            {
                //R�cup�re le controller et le visuel de la carte override
                cardController = _hit.transform.GetComponent<CardController>();
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
            //Si le joueur clic sur une card dans la main qui est interactible passe dans la zone de selection (en attente d'�tre pos�e)
            if (cardController.isInteractible)
            {
                cardController.CardStateSwitch(CardController.CardState.isWaiting);
                cardController.PlayAnimationCard("ActiveAnim");
                currentHandState = HandState.cardSelectedInHand;

                // [TEST] pour draw line
                _cardTransform = _hit.transform.GetComponent<Transform>();
            }
        }

        //CHECK LES CARTES SUR LE PLATEAU
        if (CheckRaycastHit() == "Slot")
        {
            _boardController = _hit.transform.GetComponent<BoardController>();

            if (_boardController.containCard is true)
            {
                _slotCardController = _boardController.cardController;

                //Si le joueur clic sur un slot qui contient un carte qui est interactible celle ci est est s�l�ctionn�
                if (Input.GetMouseButtonDown(0) & _slotCardController.isInteractible)
                {
                    _slotCardController.moveToPositon = _slotCardController.transform.localPosition + Vector3.up * 0.25f;
                    _slotCardController.CardStateSwitch(CardController.CardState.isSelected);
                    _slotCardController.PlayAnimationCard("ActiveAnim");
                    currentHandState = HandState.cardSelectedOnBoard;

                    // [TEST] pour draw line
                    _cardTransform = _hit.transform.GetComponent<Transform>();
                }
            }
        }
    }

    public void CardInvokeOnDesk()
    {
        if (CheckRaycastHit() == "Slot")
        {
            //R�cup�re le Slot detect� et son controller
            _boardSlot = _hit.transform.gameObject;
            _boardController = _hit.transform.GetComponent<BoardController>();

            if (_boardController.containCard is false)
            {
                // [TEST LINE] pour draw line si le joueur peut poser une carte
                DrawMovementLine(_cardTransform.position, _boardSlot.transform.position, _offsetYCurve, _lineColorDisplacement);

                if (Input.GetMouseButtonDown(0))
                {
                    //Dit a la carte d'enregistrer son slot actuel
                    cardController.UpdatePreviousSlot(_boardController);
                    //Change l'�tat de la carte
                    cardController.CardStateSwitch(CardController.CardState.onDesk);
                    cardController.PlayAnimationCard("IdleAnim");

                    //Change l'�tat de la main
                    cardController = null;
                    currentHandState = HandState.free;

                    // [TEST LINE] Desactiver la line
                    _lineRenderer.enabled = false;
                    _lineIcon.SetActive(false);
                }
            }
            else
            {
                DrawMovementLine(_cardTransform.position, _hit.point, _offsetYCurve, _lineColorNeutral);
            }
        }
        else
        {
            //Si il d�tecte un autre collider il renvoie rien (donc il faut un collider pour le plateau)
            _boardSlot = null;
            _boardController = null;

            // [TEST LINE]
            DrawMovementLine(_cardTransform.position, _hit.point, _offsetYCurve, _lineColorNeutral);
        }
    }

    public void CardSelectedState()
    {
        // BOUGER UNE CARTE
        if (CheckRaycastHit() == "Slot")
        {
            //R�cup�re le Slot detect� et son controller
            _boardSlot = _hit.transform.gameObject;
            _boardController = _hit.transform.GetComponent<BoardController>();

            if (_boardController.containCard is false)
            {
                // [TEST LINE] pour draw line si le joueur peut poser une carte
                DrawMovementLine(_cardTransform.position, _boardSlot.transform.position, _offsetYCurve, _lineColorDisplacement);

                if (Input.GetMouseButtonDown(0))
                {
                    //Change l'�tat de la main
                    currentHandState = HandState.free;

                    //Dit a la carte d'enregistrer son slot actuel
                    _slotCardController.UpdatePreviousSlot(_boardController);

                    //Change l'�tat de la carte
                    _slotCardController.CardStateSwitch(CardController.CardState.onDesk);
                    _slotCardController.PlayAnimationCard("IdleAnim");

                    // [TEST LINE] Desactiver la line
                    _lineRenderer.enabled = false;
                    _lineIcon.SetActive(false);
                }
            }

            // INVERSION DE DEUX CARTES
            if (_boardController.containCard is true)
            {
                _targetCardController = _boardController.cardController;
                DrawMovementLine(_cardTransform.position, _targetCardController.transform.position, _offsetYCurve, _lineColorDisplacement);

                if (Input.GetMouseButtonDown(0) & _targetCardController.canMove is true)
                {
                    //Repasse la main en free
                    currentHandState = HandState.free;

                    _slotCardController.UpdatePreviousSlot(_targetCardController.boardController);
                    _targetCardController.UpdatePreviousSlot(_slotCardController.previousBoardController);
                    _targetCardController.moveJumpHeight = 0.15f;
                    _slotCardController.moveJumpHeight = 0.5f;
                    _slotCardController.CardStateSwitch(CardController.CardState.onDesk);
                    _slotCardController.PlayAnimationCard("IdleAnim");

                    _targetCardController.CardStateSwitch(CardController.CardState.onDesk);

                    //[TEST LINE] Desactiver la line
                    _lineRenderer.enabled = false;
                    _lineIcon.SetActive(false);
                }
            }
        }
        else
        {
            //Si il d�tecte un autre collider il renvoie rien (donc il faut un collider pour le plateau)
            _boardSlot = null;
            _boardController = null;

            // [TEST LINE]
            DrawMovementLine(_cardTransform.position, _hit.point, _offsetYCurve, _lineColorNeutral);
        }
    }

    //Fonction qui permet de draw une ligne entre deux position avec un Offset en Y
    private void DrawMovementLine(Vector3 startPos, Vector3 endPos, float offsetY, Color lineColor)
    {
        //Modifie la couleur du line Renderer
        _lineRenderer.material.SetColor("_DotColor", lineColor);
        //Active le line Renderer
        _lineRenderer.enabled = true;
        //Set le nombre de points du line renderer
        _lineRenderer.positionCount = 15;

        //R�cup�re la distance et change le tiling des dots en fonction
        float distanceBetween = Vector3.Distance(startPos, endPos);
        _lineRenderer.material.SetFloat("_Tiling", distanceBetween * _dotPerUnit);

        //R�cup�re le point du milieu et applique un offset
        Vector3 midPoint = (startPos + endPos) / 2;
        midPoint.y += offsetY;

        //Fix l'icon de la line au milieu de la courbe
        _lineIcon.SetActive(true);
        _lineIcon.transform.position = midPoint - Vector3.down * _lineIconOffset;

        //Fonction de courbe de bezier
        float t = 0f;
        Vector3 B = new Vector3(0, 0, 0);
        for (int i = 0; i < _lineRenderer.positionCount; i++)
        {
            B = (1 - t) * (1 - t) * startPos + 2 * (1 - t) * t * midPoint + t * t * endPos;
            _lineRenderer.SetPosition(i, B);
            t += (1 / (float)_lineRenderer.positionCount);
        }
    }

    //Fonction qui Raycast depuis la camera vers le pointeur de la souris et renvoie le tag de l'objet avec lequel il collide
    private string CheckRaycastHit()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out _hit))
        {
            return _hit.collider.tag;
        }
        else
        {
            return null;
        }

    }

    private void StartTurn()
    {
        _isPlaying = true;
        
        FillHand();
    }
    
    private void EndTurn()
    {
        _isPlaying = false;
    }
}
