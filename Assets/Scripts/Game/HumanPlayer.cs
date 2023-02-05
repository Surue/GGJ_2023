using System;
using DG.Tweening;
using UnityEngine;
using TMPro;

public class HumanPlayer : Player
{
    // Active card
    private CardController activeCardController;

    // Drawing
    [Header("LINE PARAMETERS")]
    [SerializeField] private SpriteRenderer _lineIconRenderer;
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private TMP_Text _iconText;
    [SerializeField] private float _lineIconOffset = -0.4f;
    [SerializeField] private float _offsetYCurve = 1f;
    [SerializeField] private float _dotPerUnit = 1.0f;
    [SerializeField] private Color _lineColorDeplacement;
    [SerializeField] private Color _lineColorAttack;
    [SerializeField] private Color _lineColorNeutral;
    [SerializeField] private Transform _CPUCardTransform;

    // Raycast
    private RaycastHit _hit;
    private Transform _cardTransform;
    
    // Board
    private GameObject _boardSlot;

    private SlotController _slotController
    {
        get => __slotController;

        set
        {
            if (__slotController != null && value != __slotController)
                __slotController.SetHovered(false);
            
            __slotController = value;
        }
    }

    private SlotController __slotController;
    private CardController _slotCardController;
    private CardController _targetCardController;
    
    // Line
    private Vector3 lineTargetEndPos;
    private Vector3 lineCurrentEndPos;
    [SerializeField] private float lineLerpSpeed;

    private void Start()
    {
        GameManager.Instance.onHumanTurnStarted += StartTurn;
        GameManager.Instance.onHumanTurnStarted += FillHand;
        GameManager.Instance.onHumanTurnStarted += ResetCardStartTurn;
        GameManager.Instance.onHumanTurnStarted += AddManaStartTurn;
        GameManager.Instance.onHumanTurnFinished += EndTurn;
        _isPlaying = false;
    }

    private void Update()
    {
        if (!_isPlaying) return;

        switch (currentHandState)
        {
            case HandState.Free:
                OnFreeState();
                break;
            case HandState.CardSelectedOnBoard:
                OnCardSelectedOnBoard();
                break;
            case HandState.CardSelectedInHand:
                OnCardSelectedInHand();
                break;
            case HandState.WaitingTurn:
                break;
        }

        lineCurrentEndPos = Vector3.Lerp(lineCurrentEndPos, lineTargetEndPos, lineLerpSpeed * Time.deltaTime);
    }

    
    public void NextTurn()
    {
        if (!_isPlaying) return;

        GameManager.Instance.NextTurn();
    }

    private void OnFreeState()
    {
        // Check if player hover card in hands
        if (CheckRaycastHit() == "Card")
        {
            var tmpCard = _hit.transform.GetComponent<CardController>();
            if (tmpCard.currentCardState == CardController.CardState.inHand && _cardsInHand.Contains(tmpCard))
            {
                if (activeCardController != null && activeCardController != tmpCard)
                {
                    activeCardController.SetCardState(CardController.CardState.inHand);
                }
                
                activeCardController = tmpCard;
                activeCardController.SetCardState(CardController.CardState.isOverride);
            }
        }
        else if (activeCardController != null)
        {
            activeCardController.SetCardState(CardController.CardState.inHand);
            activeCardController = null;
        }
        else
        {
            activeCardController = null;
        }

        // Check if player select card in hands
        if (Input.GetMouseButtonDown(0))
        {
            if (activeCardController != null && CanDropCardOnBoard(activeCardController) &&
                activeCardController.isInteractible)
            {
                SetCardWaiting(activeCardController);

                _cardTransform = _hit.transform.GetComponent<Transform>();
            }
        }

        // Check if hover slots
        if (CheckRaycastHit() == "Slot")
        {
            var aimedSlotController = _hit.transform.GetComponent<SlotController>();
            // if (!_boardSlots.Contains(tmpBoardController)) return;
            
            _slotController = aimedSlotController;

            if (!_slotController.containCard) return;
            
            _slotCardController = _slotController.cardController;

            // Check if player click on a slot
            if ((CanDropCardOnBoard(_slotCardController) 
                 || _currentMana >= _gameRules.CardMoveManaCost 
                 || _currentMana >= _gameRules.CardSwapManaCost
                 || _slotCardController.CanAttackOtherPlayer()) 
                 && Input.GetMouseButtonDown(0) 
                 && _slotCardController.isInteractible 
                 && _slotCardController.slotController.PlayerType == EPlayerType.Human)
            {
                _slotCardController.moveToPositon = _slotCardController.transform.localPosition + Vector3.up * 0.25f;
                _slotCardController.SetCardState(CardController.CardState.isSelected);
                SetHandState(HandState.CardSelectedOnBoard);

                // [TEST] pour draw line
                _cardTransform = _hit.transform.GetComponent<Transform>();
            }
        }
    }
    
    private void OnCardSelectedInHand()
    {
        foreach (var boardSlot in _boardSlots)
        {
            boardSlot.SetHighlighted(true);
        }
        
        if (Input.GetMouseButtonDown(1))
        {
            activeCardController.SetCardState(CardController.CardState.inHand);
            
            activeCardController = null;
            SetHandState(HandState.Free);
            
            ResetLine();
            return;
        }
        
        if (CheckRaycastHit() == "Slot") // Invoke
        {
            _boardSlot = _hit.transform.gameObject;
            _slotController = _hit.transform.GetComponent<SlotController>();

            if (CanDropCardOnBoard(activeCardController) && !_slotController.containCard && _slotController.PlayerType == EPlayerType.Human)
            {
                _slotController.SetHovered(true);
                DrawMovementLine(_cardTransform.position, _boardSlot.transform.position, _offsetYCurve,
                    _lineColorDeplacement, activeCardController.cardManaCost);

                if (Input.GetMouseButtonDown(0))
                {
                    InvokeCardOnBoard(activeCardController, _slotController);

                    activeCardController = null;
                    SetHandState(HandState.Free);

                    ResetLine();
                    
                    _slotController.SetHovered(false);

                }
            }
            else
            { 
                DrawMovementLine(_cardTransform.position, _boardSlot.transform.position, _offsetYCurve, _lineColorNeutral, -1);
            }
        }
        else
        {
            _boardSlot = null;
            _slotController = null;

            // [TEST LINE]
            DrawMovementLine(_cardTransform.position, _hit.point, _offsetYCurve, _lineColorNeutral, -1);
        }
    }

    private void OnCardSelectedOnBoard()
    {
        var layerHitName = CheckRaycastHit();

        foreach (var boardSlot in _boardSlots)
        {
            boardSlot.SetHighlighted(true);
        }
        
        if (Input.GetMouseButtonDown(1))
        {
            _slotCardController.SetCardState(CardController.CardState.onDesk);
            
            _slotCardController = null;
            SetHandState(HandState.Free);
            
            foreach (var boardSlot in _boardSlots)
            {
                boardSlot.SetHighlighted(false);
            }
            
            ResetLine();
            return;
        }

        // Move card to slot
        if (layerHitName == "Slot")
        {
            _boardSlot = _hit.transform.gameObject;
            _slotController = _hit.transform.GetComponent<SlotController>();

            var possibleSlotToMoveTo = GetSlotPossibleToMoveTo(_slotCardController);

            if (_slotController.PlayerType == EPlayerType.Human && !_slotController.containCard) // Move card
            {

                if (CanMoveCardOnBoard(_slotCardController, _slotController))
                {
                    DrawMovementLine(_cardTransform.position, _boardSlot.transform.position, _offsetYCurve, _lineColorDeplacement, GetMoveCost(_slotCardController));

                    _slotController.SetHovered(true);
                    
                    if (Input.GetMouseButtonDown(0))
                    {
                        currentHandState = HandState.Free;
                    
                        MoveCardOnBoard(_slotCardController, _slotController);
                    
                        _lineRenderer.enabled = false;
                        _lineIconRenderer.gameObject.SetActive(false);
                        
                        _slotController.SetHovered(false);
                    }
                }
                else
                {
                    DrawMovementLine(_cardTransform.position, _boardSlot.transform.position, _offsetYCurve, _lineColorNeutral, -1);

                    _slotController.SetHovered(false);
                }
            }
            else if (_slotController.PlayerType == EPlayerType.Human && _slotController.containCard) // Swap cards
            {
                _targetCardController = _slotController.cardController;

                if (CanSwapCards(_slotCardController, _slotController))
                {
                    DrawMovementLine(_cardTransform.position, _targetCardController.transform.position, _offsetYCurve,
                        _lineColorDeplacement, GetSwapCost(_slotCardController));

                    if (Input.GetMouseButtonDown(0))
                    {
                        currentHandState = HandState.Free;
                    
                        SwapCardOnBoard(_slotCardController, _targetCardController);

                        _lineRenderer.enabled = false;
                        _lineIconRenderer.gameObject.SetActive(false);
                    }
                }
                else
                {
                    DrawMovementLine(_cardTransform.position, _targetCardController.transform.position, _offsetYCurve,
                        _lineColorNeutral, -1);
                }
            }
            else if (_slotCardController.CanAttack(GetPossibleCardToAttack(_slotCardController)) && _slotController.PlayerType == EPlayerType.CPU) // Attack other card
            {
                if (_slotController.containCard && _slotCardController.AttackSingleTarget() && GetPossibleCardToAttack(_slotCardController).Contains(_slotController.cardController)) // Target a specific card
                {
                    _targetCardController = _slotController.cardController;
                
                    DrawMovementLine(_cardTransform.position, _targetCardController.transform.position, _offsetYCurve, _lineColorAttack, _slotCardController.cardAttack);
                    if (Input.GetMouseButtonDown(0))
                    {
                        currentHandState = HandState.Free;
                
                        StartCoroutine(AttackOtherCard(_slotCardController, _targetCardController));
                
                        _lineRenderer.enabled = false;
                        _lineIconRenderer.gameObject.SetActive(false);
                    }
                }
                else if(!_slotCardController.AttackSingleTarget()) // Target multiple cards
                {
                    DrawMovementLine(_cardTransform.position, _slotController.transform.position, _offsetYCurve, _lineColorAttack, _slotCardController.cardAttack);
                    if (Input.GetMouseButtonDown(0))
                    {
                        currentHandState = HandState.Free;

                        foreach (var cardController in GetPossibleCardToAttack(_slotCardController))
                        {
                            StartCoroutine(AttackOtherCard(_slotCardController, cardController));
                        }
                
                        _lineRenderer.enabled = false;
                        _lineIconRenderer.gameObject.SetActive(false);
                    }
                }
                else
                {
                    DrawMovementLine(_cardTransform.position, _hit.point, _offsetYCurve, _lineColorNeutral, -1);
                }
            }
            else
            {
                DrawMovementLine(_cardTransform.position, _hit.point, _offsetYCurve,
                    _lineColorNeutral, -1);
            }
        }
        else if (layerHitName == "AttackZone" && _slotCardController.CanAttackOtherPlayer() && !TryGetCardInFront(_slotCardController, out var _)) // Attack player
        {
            DrawMovementLine(_cardTransform.position, _CPUCardTransform.position, _offsetYCurve, _lineColorAttack, _slotCardController.cardAttack);
            
            // TODO Check line of attack
            if (Input.GetMouseButtonDown(0)) 
            {
                currentHandState = HandState.Free;
                    
                StartCoroutine(AttackOtherPlayer(_slotCardController));
                    
                _lineRenderer.enabled = false;
                _lineIconRenderer.gameObject.SetActive(false);
            }
        }
        else
        {
            //Si il d�tecte un autre collider il renvoie rien (donc il faut un collider pour le plateau)
            _boardSlot = null;
            _slotController = null;

            // [TEST LINE]
            DrawMovementLine(_cardTransform.position, _hit.point, _offsetYCurve, _lineColorNeutral,-1);
        }
    }


    protected override void TakeDamage(CardController attackingCard)
    {
        base.TakeDamage(attackingCard);
        hurtVignet.DOFade(1, 0.4f).SetEase(EaseExtensions.FadeInFadeOutCurve).From(0);
    }

    public CanvasGroup hurtVignet;

    private void DrawMovementLine(Vector3 startPos, Vector3 endPos, float offsetY, Color lineColor, int value)
    {
        lineTargetEndPos = endPos;
        
        //Modifie la couleur du line Renderer
        _lineRenderer.material.SetColor("_DotColor", lineColor);
        _lineIconRenderer.color = lineColor;
        //Active le line Renderer
        _lineRenderer.enabled = true;
        //Set le nombre de points du line renderer
        _lineRenderer.positionCount = 15;

        if (lineColor != _lineColorAttack)
        {
            var currentMana = GameManager.Instance.GetPlayer(EPlayerType.Human).CurrentMana;
            _iconText.color = value > currentMana ? Color.red : Color.white;
        }
        else
        {
            _iconText.color = Color.white;
        }

        _iconText.SetText(value == -1 ? "":value.ToString());

        float distanceBetween = Vector3.Distance(startPos, lineCurrentEndPos);
        _lineRenderer.material.SetFloat("_Tiling", distanceBetween * _dotPerUnit);

        Vector3 midPoint = (startPos + lineCurrentEndPos) / 2;
        midPoint.y += offsetY;

        //Fix l'icon de la line au milieu de la courbe
        _lineIconRenderer.gameObject.SetActive(true);
        _lineIconRenderer.transform.position = midPoint - Vector3.down * _lineIconOffset;

        // Bezier's curve
        float t = 0f;
        Vector3 B = new Vector3(0, 0, 0);
        for (int i = 0; i < _lineRenderer.positionCount; i++)
        {
            B = (1 - t) * (1 - t) * startPos + 2 * (1 - t) * t * midPoint + t * t * lineCurrentEndPos;
            
            _lineRenderer.SetPosition(i, B);
            t += (1 / (float) _lineRenderer.positionCount);
        }
    }

    /// <summary>
    /// Disable preview line
    /// </summary>
    private void ResetLine()
    {
        _lineRenderer.enabled = false;
        _lineIconRenderer.gameObject.SetActive(false);
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

    public string CheckRaycastHit(out RaycastHit hit)
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out _hit))
        {
            hit = _hit;
            return _hit.collider.tag;
        }
        else
        {
            hit = new RaycastHit();
            return null;
        }

    }

    private void StartTurn()
    {
        _isPlaying = true;

        GameManager.Instance.HasFinishedStartingTurn();
    }
    
    private void EndTurn()
    {
        _isPlaying = false;
    }
    
    
    public void SetHandState(HandState handState)
    {
        currentHandState = handState;
    }
}