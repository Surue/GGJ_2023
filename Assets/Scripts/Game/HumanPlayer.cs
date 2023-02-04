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

    public void NextTurn()
    {
        if (!_isPlaying) return;
        
        GameManager.Instance.OnTurnEnd();
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

    private void FreeState()
    {
        // Check if player hover card in hands
        if (CheckRaycastHit() == "Card")
        {
            if (cardController == null)
            {
                var tmpCard = _hit.transform.GetComponent<CardController>();
                if (tmpCard.currentCardState == CardController.CardState.inHand)
                {
                    cardController = tmpCard;
                    cardController.CardStateSwitch(CardController.CardState.isOverride);
                }
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

        // Check if player select card in hands
        if (Input.GetMouseButtonDown(0) && cardController != null)
        {
            if (cardController.isInteractible)
            {
                cardController.CardStateSwitch(CardController.CardState.isWaiting);
                cardController.PlayAnimationCard("ActiveAnim");
                currentHandState = HandState.cardSelectedInHand;

                _cardTransform = _hit.transform.GetComponent<Transform>();
            }
        }

        // Check if hover slots
        if (CheckRaycastHit() == "Slot")
        {
            _boardController = _hit.transform.GetComponent<BoardController>();

            if (_boardController.containCard is not true) return;
            
            _slotCardController = _boardController.cardController;

            // Check if player click on a slot
            if (Input.GetMouseButtonDown(0) && _slotCardController.isInteractible && _slotCardController.boardController.PlayerType == EPlayerType.Human)
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

    private void CardInvokeOnDesk()
    {
        if (CheckRaycastHit() == "Slot")
        {
            //R�cup�re le Slot detect� et son controller
            _boardSlot = _hit.transform.gameObject;
            _boardController = _hit.transform.GetComponent<BoardController>();

            if (!_boardController.containCard && _boardController.PlayerType == EPlayerType.Human)
            {
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
                DrawMovementLine(_cardTransform.position, _boardSlot.transform.position, _offsetYCurve, _lineColorNeutral);
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

    private void CardSelectedState()
    {
        // Move card to slot
        if (CheckRaycastHit() == "Slot")
        {
            _boardSlot = _hit.transform.gameObject;
            _boardController = _hit.transform.GetComponent<BoardController>();

            if (_boardController.PlayerType == EPlayerType.Human && !_boardController.containCard) // Drop card on empty board
            {
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
            else if (_boardController.PlayerType == EPlayerType.Human &&_boardController.containCard) // Swap cards
            {
                _targetCardController = _boardController.cardController;
                DrawMovementLine(_cardTransform.position, _targetCardController.transform.position, _offsetYCurve, _lineColorDisplacement);

                if (Input.GetMouseButtonDown(0) & _targetCardController.canMove)
                {
                    // Repasse la main en free
                    currentHandState = HandState.free;

                    _slotCardController.UpdatePreviousSlot(_targetCardController.boardController);
                    _targetCardController.UpdatePreviousSlot(_slotCardController.previousBoardController);
                    _targetCardController.moveJumpHeight = 0.15f;
                    _slotCardController.moveJumpHeight = 0.5f;
                    _slotCardController.CardStateSwitch(CardController.CardState.onDesk);
                    _slotCardController.PlayAnimationCard("IdleAnim");

                    _targetCardController.CardStateSwitch(CardController.CardState.onDesk);

                    _lineRenderer.enabled = false;
                    _lineIcon.SetActive(false);
                }
            }
            else
            {
                DrawMovementLine(_cardTransform.position, _boardSlot.transform.position, _offsetYCurve, _lineColorNeutral);
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
    
    private void DrawMovementLine(Vector3 startPos, Vector3 endPos, float offsetY, Color lineColor)
    {
        //Modifie la couleur du line Renderer
        _lineRenderer.material.SetColor("_DotColor", lineColor);
        //Active le line Renderer
        _lineRenderer.enabled = true;
        //Set le nombre de points du line renderer
        _lineRenderer.positionCount = 15;

        float distanceBetween = Vector3.Distance(startPos, endPos);
        _lineRenderer.material.SetFloat("_Tiling", distanceBetween * _dotPerUnit);

        Vector3 midPoint = (startPos + endPos) / 2;
        midPoint.y += offsetY;

        //Fix l'icon de la line au milieu de la courbe
        _lineIcon.SetActive(true);
        _lineIcon.transform.position = midPoint - Vector3.down * _lineIconOffset;

        // Bezier's curve
        float t = 0f;
        Vector3 B = new Vector3(0, 0, 0);
        for (int i = 0; i < _lineRenderer.positionCount; i++)
        {
            B = (1 - t) * (1 - t) * startPos + 2 * (1 - t) * t * midPoint + t * t * endPos;
            _lineRenderer.SetPosition(i, B);
            t += (1 / (float)_lineRenderer.positionCount);
        }
    }

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
}
