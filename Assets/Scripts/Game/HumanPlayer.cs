using UnityEngine;
using TMPro;

public class HumanPlayer : Player
{
    private bool _isPlaying;
    
    // Active card
    private CardController activeCardController;

    // Drawing
    [Header("LINE PARAMETERS")]
    [SerializeField] private GameObject _lineIcon;
    [SerializeField] private SpriteRenderer _lineIconRenderer;
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private TMP_Text _iconText;
    [SerializeField] private float _lineIconOffset = -0.4f;
    [SerializeField] private float _offsetYCurve = 1f;
    [SerializeField] private float _dotPerUnit = 1.0f;
    [SerializeField] private Color _lineColorDeplacement;
    [SerializeField] private Color _lineColorAttack;
    [SerializeField] private Color _lineColorNeutral;
    
    // Raycast
    private RaycastHit _hit;
    private Transform _cardTransform;
    
    // Board
    private GameObject _boardSlot;
    private SlotController _slotController;
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
                FreeState();
                break;
            case HandState.CardSelectedOnBoard:
                CardSelectedState();
                break;
            case HandState.CardSelectedInHand:
                CardInvokeOnDesk();
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

    private void FreeState()
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
                 || CanMoveCardOnBoard() 
                 || _slotCardController.CanAttack()) 
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

    public void SetHandState(HandState handState)
    {
        currentHandState = handState;
    }

    private void CardInvokeOnDesk()
    {
        if (Input.GetMouseButtonDown(1))
        {
            activeCardController.SetCardState(CardController.CardState.inHand);
            
            activeCardController = null;
            SetHandState(HandState.Free);
            
            ResetLine();
            return;
        }
        
        if (CheckRaycastHit() == "Slot")
        {
            _boardSlot = _hit.transform.gameObject;
            _slotController = _hit.transform.GetComponent<SlotController>();

            if (CanDropCardOnBoard(activeCardController) && !_slotController.containCard && _slotController.PlayerType == EPlayerType.Human)
            {
                DrawMovementLine(_cardTransform.position, _boardSlot.transform.position, _offsetYCurve, _lineColorDeplacement, activeCardController.cardManaCost.ToString());

                if (Input.GetMouseButtonDown(0))
                {
                    DropCardOnBoard(activeCardController, _slotController);

                    activeCardController = null;
                    SetHandState(HandState.Free);

                    ResetLine();
                }
            }
            else
            {
                DrawMovementLine(_cardTransform.position, _boardSlot.transform.position, _offsetYCurve, _lineColorNeutral, "");
            }
        }
        else
        {
            _boardSlot = null;
            _slotController = null;

            // [TEST LINE]
            DrawMovementLine(_cardTransform.position, _hit.point, _offsetYCurve, _lineColorNeutral, "");
        }
    }

    /// <summary>
    /// Disable preview line
    /// </summary>
    private void ResetLine()
    {
        _lineRenderer.enabled = false;
        _lineIcon.SetActive(false);
    }

    private void CardSelectedState()
    {
        var layerHitName = CheckRaycastHit();
        
        if (Input.GetMouseButtonDown(1))
        {
            _slotCardController.SetCardState(CardController.CardState.onDesk);
            
            _slotCardController = null;
            SetHandState(HandState.Free);
            
            ResetLine();
            return;
        }

        // Move card to slot
        if (layerHitName == "Slot")
        {
            _boardSlot = _hit.transform.gameObject;
            _slotController = _hit.transform.GetComponent<SlotController>();

            if (CanMoveCardOnBoard() && _slotController.PlayerType == EPlayerType.Human && !_slotController.containCard) // Drop card on empty board
            {
                DrawMovementLine(_cardTransform.position, _boardSlot.transform.position, _offsetYCurve, _lineColorDeplacement, _gameRules.CardMoveManaCost.ToString());

                if (Input.GetMouseButtonDown(0))
                {
                    currentHandState = HandState.Free;
                    
                    MoveCardOnBoard(_slotCardController, _slotController);
                    
                    _lineRenderer.enabled = false;
                    _lineIcon.SetActive(false);
                }
            }
            else if (CanSwapCards() && _slotController.PlayerType == EPlayerType.Human && _slotController.containCard) // Swap cards
            {
                _targetCardController = _slotController.cardController;
                DrawMovementLine(_cardTransform.position, _targetCardController.transform.position, _offsetYCurve, _lineColorDeplacement, _gameRules.CardSwapManaCost.ToString());

                if (Input.GetMouseButtonDown(0))
                {
                    currentHandState = HandState.Free;
                    
                    SwapCardOnBoard(_slotCardController, _targetCardController);

                    _lineRenderer.enabled = false;
                    _lineIcon.SetActive(false);
                }
            }else if (_slotCardController.CanAttack() && _slotController.PlayerType == EPlayerType.CPU && _slotController.containCard) // Attack other card
            {
                _targetCardController = _slotController.cardController;
                DrawMovementLine(_cardTransform.position, _targetCardController.transform.position, _offsetYCurve, _lineColorAttack, _slotCardController.cardAttack.ToString());

                if (Input.GetMouseButtonDown(0) && GetPossibleCardToAttack(_slotCardController).Contains(_targetCardController))
                {
                    currentHandState = HandState.Free;
                    
                    AttackOtherCard(_slotCardController, _targetCardController);
                    
                    _lineRenderer.enabled = false;
                    _lineIcon.SetActive(false);
                }
            }
            else
            {
                DrawMovementLine(_cardTransform.position, _boardSlot.transform.position, _offsetYCurve, _lineColorNeutral, "");
            }
        }
        else if (layerHitName == "AttackZone") // Attack player
        {
            DrawMovementLine(_cardTransform.position, _hit.point, _offsetYCurve, _lineColorAttack, _slotCardController.cardAttack.ToString());
            
            // TODO Check line of attack
            if (Input.GetMouseButtonDown(0) && _slotCardController.CanAttack()) 
            {
                currentHandState = HandState.Free;
                    
                AttackOtherPlayer(_slotCardController);
                    
                _lineRenderer.enabled = false;
                _lineIcon.SetActive(false);
            }
        }
        else
        {
            //Si il d�tecte un autre collider il renvoie rien (donc il faut un collider pour le plateau)
            _boardSlot = null;
            _slotController = null;

            // [TEST LINE]
            DrawMovementLine(_cardTransform.position, _hit.point, _offsetYCurve, _lineColorNeutral,"");
        }
    }

    private void DrawMovementLine(Vector3 startPos, Vector3 endPos, float offsetY, Color lineColor, string value)
    {
        lineTargetEndPos = endPos;
        
        //Modifie la couleur du line Renderer
        _lineRenderer.material.SetColor("_DotColor", lineColor);
        _lineIconRenderer.color = lineColor;
        //Active le line Renderer
        _lineRenderer.enabled = true;
        //Set le nombre de points du line renderer
        _lineRenderer.positionCount = 15;

        _iconText.SetText(value);

        float distanceBetween = Vector3.Distance(startPos, lineCurrentEndPos);
        _lineRenderer.material.SetFloat("_Tiling", distanceBetween * _dotPerUnit);

        Vector3 midPoint = (startPos + lineCurrentEndPos) / 2;
        midPoint.y += offsetY;

        //Fix l'icon de la line au milieu de la courbe
        _lineIcon.SetActive(true);
        _lineIcon.transform.position = midPoint - Vector3.down * _lineIconOffset;

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

    public string CheckRaycastHit()
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
}
