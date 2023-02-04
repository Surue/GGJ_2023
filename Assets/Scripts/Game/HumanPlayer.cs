using UnityEditor.Rendering;
using UnityEngine;

public class HumanPlayer : Player
{
    private bool _isPlaying;
    
    // Active card
    private CardController activeCardController;

    // Drawing
    [Header("LINE PARAMETERS")]
    [SerializeField] private GameObject _lineIcon;
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private float _lineIconOffset = -0.4f;
    [SerializeField] private float _offsetYCurve = 1f;
    [SerializeField] private float _dotPerUnit = 1.0f;
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
            default:
                break;
        }
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
            if (tmpCard.currentCardState == CardController.CardState.inHand)
            {
                if (activeCardController != null && activeCardController != tmpCard)
                {
                    activeCardController.CardStateSwitch(CardController.CardState.inHand);
                }
                
                activeCardController = tmpCard;
                activeCardController.CardStateSwitch(CardController.CardState.isOverride);
            }
        }
        else if (activeCardController != null)
        {
            activeCardController.CardStateSwitch(CardController.CardState.inHand);
            activeCardController = null;
        }
        else
        {
            activeCardController = null;
        }

        // Check if player select card in hands
        if (Input.GetMouseButtonDown(0) && activeCardController != null && CanDropCardOnBoard(activeCardController) && activeCardController.isInteractible)
        {
            activeCardController.CardStateSwitch(CardController.CardState.isWaiting);
            activeCardController.PlayAnimationCard("ActiveAnim");
            currentHandState = HandState.CardSelectedInHand;

            _cardTransform = _hit.transform.GetComponent<Transform>();
        }

        // Check if hover slots
        if (CheckRaycastHit() == "Slot")
        {
            _boardController = _hit.transform.GetComponent<BoardController>();

            if (!_boardController.containCard) return;
            
            _slotCardController = _boardController.cardController;

            // Check if player click on a slot
            if ((CanDropCardOnBoard(_slotCardController) || CanMoveCardOnBoard()) && Input.GetMouseButtonDown(0) && _slotCardController.isInteractible && _slotCardController.boardController.PlayerType == EPlayerType.Human)
            {
                _slotCardController.moveToPositon = _slotCardController.transform.localPosition + Vector3.up * 0.25f;
                _slotCardController.CardStateSwitch(CardController.CardState.isSelected);
                _slotCardController.PlayAnimationCard("ActiveAnim");
                currentHandState = HandState.CardSelectedOnBoard;

                // [TEST] pour draw line
                _cardTransform = _hit.transform.GetComponent<Transform>();
            }
        }
    }

    private void CardInvokeOnDesk()
    {
        if (CheckRaycastHit() == "Slot")
        {
            _boardSlot = _hit.transform.gameObject;
            _boardController = _hit.transform.GetComponent<BoardController>();

            if (CanDropCardOnBoard(activeCardController) && !_boardController.containCard && _boardController.PlayerType == EPlayerType.Human)
            {
                DrawMovementLine(_cardTransform.position, _boardSlot.transform.position, _offsetYCurve, _lineColorDisplacement);

                if (Input.GetMouseButtonDown(0))
                {
                    DropCardOnBoard(activeCardController, _boardController);

                    activeCardController = null;
                    currentHandState = HandState.Free;

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
            _boardSlot = null;
            _boardController = null;

            // [TEST LINE]
            DrawMovementLine(_cardTransform.position, _hit.point, _offsetYCurve, _lineColorNeutral);
        }
    }

    private void CardSelectedState()
    {
        var layerHitName = CheckRaycastHit();
        
        // Move card to slot
        if (layerHitName == "Slot")
        {
            _boardSlot = _hit.transform.gameObject;
            _boardController = _hit.transform.GetComponent<BoardController>();

            if (CanMoveCardOnBoard() && _boardController.PlayerType == EPlayerType.Human && !_boardController.containCard) // Drop card on empty board
            {
                DrawMovementLine(_cardTransform.position, _boardSlot.transform.position, _offsetYCurve, _lineColorDisplacement);

                if (Input.GetMouseButtonDown(0))
                {
                    currentHandState = HandState.Free;
                    
                    MoveCardOnBoard(_slotCardController, _boardController);
                    
                    _lineRenderer.enabled = false;
                    _lineIcon.SetActive(false);
                }
            }
            else if (CanSwapCards() && _boardController.PlayerType == EPlayerType.Human && _boardController.containCard) // Swap cards
            {
                _targetCardController = _boardController.cardController;
                DrawMovementLine(_cardTransform.position, _targetCardController.transform.position, _offsetYCurve, _lineColorDisplacement);

                if (Input.GetMouseButtonDown(0))
                {
                    currentHandState = HandState.Free;
                    
                    SwapCardOnBoard(_slotCardController, _targetCardController);

                    _lineRenderer.enabled = false;
                    _lineIcon.SetActive(false);
                }
            }else if (_slotCardController.CanAttack() && _boardController.PlayerType == EPlayerType.CPU && _boardController.containCard) // Attack other card
            {
                _targetCardController = _boardController.cardController;
                DrawMovementLine(_cardTransform.position, _targetCardController.transform.position, _offsetYCurve, _lineColorAttack);

                if (Input.GetMouseButtonDown(0))
                {
                    currentHandState = HandState.Free;
                    
                    AttackOtherCard(_slotCardController, _targetCardController);
                    
                    _lineRenderer.enabled = false;
                    _lineIcon.SetActive(false);
                }
            }
            else
            {
                DrawMovementLine(_cardTransform.position, _boardSlot.transform.position, _offsetYCurve, _lineColorNeutral);
            }
        }
        else if (layerHitName == "AttackZone")
        {
            DrawMovementLine(_cardTransform.position, _hit.point, _offsetYCurve, _lineColorAttack);
            
            // TODO Check line of attack
            if (Input.GetMouseButtonDown(0) && _slotCardController.CanAttack()) // Attack player
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
