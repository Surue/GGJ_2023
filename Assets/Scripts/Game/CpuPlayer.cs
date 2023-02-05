using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class CpuPlayer : Player
{
    protected override EPlayerType GetPlayerType() => EPlayerType.CPU;
    
    private enum ECpuPhase
    {
        InvokeCard,
        InvokingCard,
        Attack,
        Attacking
        ,
        End
    }

    private ECpuPhase _phase;

    private void Start()
    {
        GameManager.Instance.onCpuTurnStarted += StartTurn;
        GameManager.Instance.onCpuTurnFinished += EndTurn;

        _isPlaying = false;
    }

    private void Update()
    {
        if(!_isPlaying) return;

        if (!_hasFinishSimulating) return;

        switch (_phase)
        {
            case ECpuPhase.InvokeCard:
                _phase = ECpuPhase.InvokingCard;
                StartCoroutine(PlayAllAction());
                break;
            case ECpuPhase.InvokingCard:
                break;
            case ECpuPhase.Attack:
                break;
            case ECpuPhase.Attacking:
                break;
            case ECpuPhase.End:
                EndTurn();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private IEnumerator PlayAllAction()
    {
        var actions = _turnToPlay.parentSimulatedTurn.actions;
        while (actions.Count > 0)
        {
            var action = actions[0];
            actions.RemoveAt(0);

            if (action is InvokeCardPlayerAction invokeCardPlayerAction)
            {
                var cardController = invokeCardPlayerAction.cardToInvoke.cardController;
                
                SetCardWaiting(cardController);

                yield return null;
            
                while (cardController.isTweening)
                {
                    yield return null;
                }
            
                // random wait time
                float timer = Random.Range(0.5f, 1.5f);
                while (timer > 0)
                {
                    timer -= Time.deltaTime;
                    yield return null;
                }

                InvokeCardOnBoard(cardController, invokeCardPlayerAction.slot.slotController);
            
                yield return null;
            
                while (cardController.isTweening)
                {
                    yield return null;
                }
            }
            
            if (action is MovePlayerAction movePlayerAction)
            {
                var cardController = movePlayerAction.cardToMove.cardController;
                var slot = movePlayerAction.newSlot.slotController;
                
                MoveCardOnBoard(cardController, slot);

                yield return null;
            
                while (cardController.isTweening)
                {
                    yield return null;
                }
            }
            
            if (action is SwapCardsPlayerAction swapCardsPlayerAction)
            {
                var cardToMove = swapCardsPlayerAction.cardToMove.cardController;
                var cardToSwap = swapCardsPlayerAction.cardToSwap.cardController;
                
                SwapCardOnBoard(cardToMove, cardToSwap);

                yield return null;
            
                while (cardToMove.isTweening)
                {
                    yield return null;
                }
            }
            
            if (action is AttackOtherAction attackOtherCardAction)
            {
                var cardController = attackOtherCardAction.attackingCard.cardController;
                var cardToAttack = attackOtherCardAction.defendingCard.cardController;
                yield return StartCoroutine(AttackOtherCard(cardController, cardToAttack));
            }
            
            if (action is AttackOtherPlayerAction attackOtherPlayerAction)
            {
                var cardController = attackOtherPlayerAction.attackingCard.cardController;
                yield return StartCoroutine(AttackOtherPlayer(cardController));
            }
        }
        
        _phase = ECpuPhase.End;
    }

    private void StartTurn()
    {
        AddManaStartTurn();
        FillHand();
        ResetCardStartTurn();
        
        _isPlaying = true;

        _phase = ECpuPhase.InvokeCard;
        
        GameManager.Instance.HasFinishedStartingTurn();

        StartCoroutine(SimulateGames());
    }

    private IEnumerator InvokeCardsCoroutine()
    {
        _phase = ECpuPhase.InvokingCard;
        
        var freeSlots = GetFreeSlots();
        var cardsToInvoke = GetCardsToInvoke(freeSlots.Count);

        foreach (var cardController in cardsToInvoke)
        {
            SetCardWaiting(cardController);

            yield return null;
            
            while (cardController.isTweening)
            {
                yield return null;
            }
            
            // random wait time
            float timer = Random.Range(0.5f, 1.5f);
            while (timer > 0)
            {
                timer -= Time.deltaTime;
                yield return null;
            }

            var freeSlot = freeSlots[Random.Range(0, freeSlots.Count)];
            freeSlots.Remove(freeSlot);
            
            InvokeCardOnBoard(cardController, freeSlot);
            
            yield return null;
            
            while (cardController.isTweening)
            {
                yield return null;
            }
        }

        _phase = ECpuPhase.Attack;
    }

    private IEnumerator AttackCoroutine()
    {
        _phase = ECpuPhase.Attacking;
        for (var i = _cardsOnBoard.Count - 1; i >= 0; i--)
        {
            var cardController = _cardsOnBoard[i];
            var cardsToAttack = GetPossibleCardToAttack(cardController);

            if (cardsToAttack.Count > 0)
            {
                if (cardsToAttack.Count == 1)
                {
                    StartCoroutine(AttackOtherCard(cardController, cardsToAttack[0]));
                }
                else
                {
                    if (cardController.AttackSingleTarget())
                    {
                        StartCoroutine(AttackOtherCard(cardController, cardsToAttack[Random.Range(0, cardsToAttack.Count)]));
                    }
                    else
                    {
                        foreach (var controller in cardsToAttack)
                        {
                            StartCoroutine(AttackOtherCard(cardController, controller));
                        }
                    }
                }
            }
            else if(cardsToAttack.Count == 0 && cardController.slotController.boardLineType == EBoardLineType.Front)
            {
                StartCoroutine(AttackOtherPlayer(cardController));
            }

            yield return null;
            
            while (cardController.isTweening)
            {
                yield return null;
            }
        }

        _phase = ECpuPhase.End;
    }
    
    private void EndTurn()
    {
        _isPlaying = false;
        
        GameManager.Instance.NextTurn();
    }
    
    #region Game Simulation

    private float _maxTComputationTime = 5.0f;

    public class SimulatedCard
    {
        public int health;
        public int damage;
        public int manaCost;
        public int remainingAttackCount = 1; // TODO Incorrect simulation

        public SimulatedSlot simulatedSlot;

        public CardController cardController;

        public SimulatedCard(CardController card)
        {
            health = card.cardHealth;
            damage = card.cardAttack;
            manaCost = card.cardManaCost;

            cardController = card;
        }

        public void TakeDamage(SimulatedCard attackingCard)
        {
            health -= attackingCard.damage;
        }

        public bool IsDead()
        {
            return health < 0;
        }

        public bool IsFacing(SimulatedCard otherCard)
        {
            return simulatedSlot.isFront && 3 - otherCard.simulatedSlot.columnID == simulatedSlot.columnID;
        }
    }

    public class SimulatedSlot
    {
        public SimulatedCard simulatedCard;

        public int index;
        public int columnID;
        public bool isFront;

        public SlotController slotController;

        public SimulatedSlot(SlotController slotController)
        {
            index = slotController.slotID;
            columnID = slotController.columnID;
            isFront = slotController.boardLineType == EBoardLineType.Front;

            this.slotController = slotController;
        }

        public bool HasCard()
        {
            return simulatedCard != null;
        }
    }

    public class SimulatedDeck
    {
        public DeckScriptable deckScriptable;
    }
    
    public class SimulatedPlayer 
    {
        public int health;
        public int mana;

        public int minimumCardInHand;
        public int manaNextTurn;
        
        public List<SimulatedCard> cardsInHand = new ();
        public Queue<SimulatedCard> cardsInDeck = new();
        public List<SimulatedCard> cardsOnBoard = new();
        public List<SimulatedCard> cardsDiscarded = new();

        public List<SimulatedSlot> simulatedSlots = new();

        public GameRulesScriptables gameRulesScriptable;
        
        public SimulatedPlayer(Player player)
        {
            // Simulated slots
            foreach (var slotController in player.BoardSlots)
            {
                simulatedSlots.Add(new SimulatedSlot(slotController));
            }
            
            // Cards
            foreach (var cardController in player.CardsInHand)
            {
                cardsInHand.Add(new SimulatedCard(cardController));   
            }
            
            foreach (var cardController in player.CardsInDeck)
            {
                cardsInDeck.Enqueue(new SimulatedCard(cardController));   
            }
            cardsInDeck = new Queue<SimulatedCard>(cardsInDeck.Reverse());
            
            foreach (var cardController in player.CardsOnBoard)
            {
                var newCard = new SimulatedCard(cardController);
                cardsOnBoard.Add(newCard);

                simulatedSlots[cardController.slotController.slotID].simulatedCard = newCard;
                newCard.simulatedSlot = simulatedSlots[cardController.slotController.slotID];
            }
            
            foreach (var cardController in player.CardsDiscarded)
            {
                cardsDiscarded.Add(new SimulatedCard(cardController));   
            }

            gameRulesScriptable = player.GameRulesScriptables;
            minimumCardInHand = gameRulesScriptable.MaxCardInHand;
            manaNextTurn = player.PreviousMana;
            
            
            health = player.CurrentHealth;
            mana = player.CurrentMana;
        }
        
        public SimulatedPlayer(SimulatedPlayer player)
        {
            health = player.health;
            mana = player.mana;

            minimumCardInHand = player.minimumCardInHand;
            manaNextTurn = player.manaNextTurn + 1;
        
            cardsInHand = player.cardsInHand.ToList();
            cardsInDeck = new Queue<SimulatedCard>(player.cardsInDeck);
            cardsOnBoard = player.cardsOnBoard.ToList();
            cardsDiscarded = player.cardsDiscarded.ToList();

            simulatedSlots = player.simulatedSlots; 

            gameRulesScriptable = player.gameRulesScriptable;
        }

        public void StartTurn()
        {
            // Fill hand
            if (cardsInHand.Count < minimumCardInHand)
            {
                cardsInHand.Add(cardsInDeck.Dequeue());
            }

            // Increase mana
            mana = manaNextTurn;
            
            // Reset remaining attack
            foreach (var simulatedCard in cardsOnBoard)
            {
                simulatedCard.remainingAttackCount = 1; // TODO take initial attack charge
            }
        }
        
        public List<PlayerAction> GetInvokeCardAction()
        {
            var possibleCardToInvoke = new List<SimulatedCard>();
        
            foreach (var cardController in cardsInHand)
            {
                if (CanDropCardOnBoard(cardController))
                {
                    possibleCardToInvoke.Add(cardController);
                }
            }

            var freeSlots = new List<SimulatedSlot>();
            foreach (var simulatedSlot in simulatedSlots)
            {
                if (!simulatedSlot.HasCard())
                {
                    freeSlots.Add(simulatedSlot);
                }
            }

            var playerActions = new List<PlayerAction>();
            foreach (var slot in freeSlots)
            {
                foreach (var card in possibleCardToInvoke)
                {
                    playerActions.Add(new InvokeCardPlayerAction()
                    {
                        cardToInvoke = card,
                        slot = slot,
                        player = this
                    });
                }
            }

            return playerActions;
        }
        
        public List<PlayerAction> GetMoveAndSwapAction()
        {
            var playerActions = new List<PlayerAction>();
            foreach (var slot in simulatedSlots)
            {
                foreach (var card in cardsOnBoard)
                {
                    if (slot.HasCard() && CanSwapCard()) // Swap
                    {
                        playerActions.Add(new SwapCardsPlayerAction()
                        {
                            cardToMove = card,
                            cardToSwap = slot.simulatedCard
                        }); 
                    }
                    else if(!slot.HasCard() && CanMoveCard()) // Move
                    {
                        playerActions.Add(new MovePlayerAction()
                        {
                            cardToMove = card,
                            newSlot = slot
                        });   
                    }
                }
            }

            return playerActions;
        }
        
        public List<PlayerAction> GetAttackAction(SimulatedPlayer otherPlayer)
        {
            var playerActions = new List<PlayerAction>();
            foreach (var attackingCard in cardsOnBoard)
            {
                if (!CanAttack(attackingCard)) continue;
                    
                if (CardHasTarget(attackingCard, otherPlayer, out var cardToAttack))
                {
                    foreach (var defendingCard in cardToAttack)
                    {
                        playerActions.Add(new AttackOtherAction()
                        {
                            attackingCard = attackingCard,
                            defendingCard = defendingCard,
                            attackingPlayer = this,
                            defendingPlayer = otherPlayer,
                            canDefend = defendingCard.IsFacing(attackingCard)
                        });
                    }
                }else if (attackingCard.simulatedSlot.isFront)
                {
                    playerActions.Add(new AttackOtherPlayerAction()
                    {
                        attackingCard = attackingCard,
                        defendingPlayer = otherPlayer
                    });
                }
            }

            return playerActions;
        }
        
        

        private bool CardHasTarget(SimulatedCard attackingCard, SimulatedPlayer otherPlayer, out List<SimulatedCard> cardToAttack)
        {
            bool hasTarget = false;
            cardToAttack = new List<SimulatedCard>();
            foreach (var defendingCard in otherPlayer.cardsOnBoard)
            {
                // TODO Correctly find cards to attack
                if (defendingCard.simulatedSlot != null && defendingCard.IsFacing(attackingCard))
                {
                    hasTarget = true;
                    cardToAttack.Add(defendingCard);   
                }
                
                bool TryGetCardFacing(SimulatedCard cardController, out SimulatedCard result)
                {
                    if (cardController.simulatedSlot.isFront)
                    {
                        var slot = otherPlayer.simulatedSlots[3 - cardController.simulatedSlot.columnID];
                        result = slot.simulatedCard;
                        return slot.HasCard();
                    }
                
                    result = null;
                    return false;
                }

                List<SimulatedCard> TryGetFrontAndBack(SimulatedCard cardController)
                {
                    var result = new List<SimulatedCard>();
                    
                    var frontSlot = otherPlayer.simulatedSlots[3 - cardController.simulatedSlot.columnID];
                    if (frontSlot.HasCard())
                    {
                        result.Add(frontSlot.simulatedCard);
                    }
                    
                    var backSlot = otherPlayer.simulatedSlots[3 - cardController.simulatedSlot.columnID + 4];
                    if (backSlot.HasCard())
                    {
                        result.Add(backSlot.simulatedCard);
                    }
                    
                    return result;
                }
                
                List<SimulatedCard> TryGetFrontLine(SimulatedCard cardController)
                {
                    var result = new List<SimulatedCard>();

                    for (int i = 0; i < 4; i++)
                    {
                        var slot = otherPlayer.simulatedSlots[i];
                        if (slot.HasCard())
                        {
                            result.Add(slot.simulatedCard);
                        }
                    }
                    
                    return result;
                }
                
                switch (attackingCard.cardController.CardScriptable.AttackScriptable.AttackType)
                {
                    case EAttackType.Front:
                        if (TryGetCardFacing(attackingCard, out var card))
                        {
                            cardToAttack.Add(card);
                        }
                        break;
                    case EAttackType.FrontAndBack:
                        cardToAttack.AddRange(TryGetFrontAndBack(attackingCard));
                        break;
                    case EAttackType.FrontLine:
                        cardToAttack.AddRange(TryGetFrontLine(attackingCard));
                        break;
                    case EAttackType.NoAttack:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return hasTarget;
        }

        public bool IsDead()
        {
            return health <= 0;
        }

        public void TakeDamage(SimulatedCard attackingCard)
        {
            health -= attackingCard.damage;
        }
        
        public bool CanDropCardOnBoard(SimulatedCard cardToDrop)
        {
            return mana >= cardToDrop.manaCost;
        }
        
        public bool CanMoveCard()
        {
            return mana > 0; // TODO Use correct cost of moving card
        }
        
        public bool CanSwapCard()
        {
            return mana > 0; // TODO Use correct cost of swaping card
        }

        public bool CanAttack(SimulatedCard simulatedCard)
        {
            return simulatedCard.remainingAttackCount > 0 && simulatedCard.simulatedSlot.isFront && simulatedCard.cardController.CardScriptable.AttackScriptable.AttackType != EAttackType.NoAttack;
        }

        public void Attack(SimulatedCard simulatedCard)
        {
            simulatedCard.remainingAttackCount--;
        }
    }

    public abstract class PlayerAction
    {
        public abstract int ExecuteAndGetManaCost();
    }
    
    public class InvokeCardPlayerAction : PlayerAction
    {
        public SimulatedCard cardToInvoke;
        public SimulatedSlot slot;
        public SimulatedPlayer player;
        
        public override int ExecuteAndGetManaCost()
        {
            cardToInvoke.simulatedSlot = slot;
            slot.simulatedCard = cardToInvoke;
            
            player.cardsInHand.Remove(cardToInvoke);
            player.cardsOnBoard.Add(cardToInvoke);
            
            Debug.Log("Invoke card");

            return cardToInvoke.manaCost;
        }
    }
    
    public class MovePlayerAction : PlayerAction
    {
        public SimulatedCard cardToMove;
        public SimulatedSlot newSlot;
        
        public override int ExecuteAndGetManaCost()
        {
            cardToMove.simulatedSlot.simulatedCard = null;

            cardToMove.simulatedSlot = newSlot;
            newSlot.simulatedCard = cardToMove;

            Debug.Log("Move card");
            
            // TODO use correct cost
            return 1;
        }
    }

    public class SwapCardsPlayerAction : PlayerAction
    {
        public SimulatedCard cardToMove;
        public SimulatedCard cardToSwap;
        
        public override int ExecuteAndGetManaCost()
        {
            var tmpSlot = cardToMove.simulatedSlot;

            cardToMove.simulatedSlot = cardToSwap.simulatedSlot;
            cardToMove.simulatedSlot.simulatedCard = cardToMove;

            cardToSwap.simulatedSlot = tmpSlot;
            tmpSlot.simulatedCard = cardToSwap;
            
            Debug.Log("Swap card");

            // TODO use correct cost
            return 1;
        }
    }
    
    public class AttackOtherAction : PlayerAction
    {
        public SimulatedCard attackingCard;
        public SimulatedCard defendingCard;
        public bool canDefend;
        public SimulatedPlayer attackingPlayer;
        public SimulatedPlayer defendingPlayer;
        
        public override int ExecuteAndGetManaCost()
        {
            attackingCard.remainingAttackCount--;
            defendingCard.TakeDamage(attackingCard);
            if (canDefend)
            {
                attackingCard.TakeDamage(defendingCard);
            }

            var message = "Attack card";
            Debug.Log("Attack card");

            if (defendingCard.IsDead())
            {
                message += ", defending card is dead";
                defendingPlayer.cardsOnBoard.Remove(defendingCard);
                defendingPlayer.cardsDiscarded.Add(defendingCard);
            }
            
            if (attackingCard.IsDead())
            {
                message += ", attacking card is dead";
                attackingPlayer.cardsOnBoard.Remove(attackingCard);
                attackingPlayer.cardsDiscarded.Add(attackingCard);
            }
            Debug.Log(message);
                
            return 0;
        }
    }
    
    public class AttackOtherPlayerAction : PlayerAction
    {
        public SimulatedCard attackingCard;
        public SimulatedPlayer defendingPlayer;
        
        public override int ExecuteAndGetManaCost()
        {
            attackingCard.remainingAttackCount--;
            defendingPlayer.TakeDamage(attackingCard);
            Debug.Log("Attack other player HP(" + defendingPlayer.health + ")");
            return 0;
        }
    }
    
    public class SimulatedTurn
    {
        // Tree
        public SimulatedTurn parentSimulatedTurn;
     
        // Global values
        public GameRulesScriptables gameRulesScriptables;
        public DeckScriptable simulatedDeck;
        
        // Values
        public SimulatedPlayer simulatedHuman;
        public SimulatedPlayer simulatedCpu;
        
        // Result
        public bool cpuWin;
        
        // Selected action
        public List<PlayerAction> actions = new();
        
        // Constructors
        public SimulatedTurn(Player humanPlayer, Player cpuPlayer)
        {
            gameRulesScriptables = humanPlayer.GameRulesScriptables;
            simulatedDeck = humanPlayer.DeckScriptable;
            simulatedDeck = humanPlayer.DeckScriptable;
            
            // TODO Cheat by knowing all player cards ahead of time
            simulatedHuman = new SimulatedPlayer(humanPlayer);
            simulatedCpu = new SimulatedPlayer(cpuPlayer);

            parentSimulatedTurn = this;
            turnCount = 0;
        }
        
        public SimulatedTurn(SimulatedPlayer humanPlayer, SimulatedPlayer cpuPlayer, SimulatedTurn parentTurn)
        {
            simulatedHuman = humanPlayer;
            simulatedCpu = cpuPlayer;

            gameRulesScriptables = parentTurn.gameRulesScriptables;
            simulatedDeck = parentTurn.simulatedDeck;

            parentSimulatedTurn = parentTurn.parentSimulatedTurn;
            turnCount = parentTurn.turnCount + 1;
        }
        
        // Simulation
        public SimulatedTurn SimulateFullTurn()
        {
            Debug.Log("Start simulating turns");
            // CPU turn
            actions = new List<PlayerAction>(SimulateTurnOfPlayer(simulatedCpu, simulatedHuman, out var simulatedCpuNextTurn, out var simulatedHumanNextTurn));

            if (simulatedHumanNextTurn.IsDead())
            {
                Debug.Log("Human is dead");
                var finalTurn = new SimulatedTurn(simulatedHumanNextTurn, simulatedCpuNextTurn, this)
                {
                    cpuWin = true
                };
                return finalTurn;
            }
            
            // Human turn
            SimulateTurnOfPlayer(simulatedHumanNextTurn, simulatedCpuNextTurn, out simulatedHumanNextTurn, out simulatedCpuNextTurn);
            
            if (simulatedCpuNextTurn.IsDead())
            {
                Debug.Log("CPU is dead");
                var finalTurn = new SimulatedTurn(simulatedHumanNextTurn, simulatedCpuNextTurn, this)
                {
                    cpuWin = false
                };
                return finalTurn;
            }

            Debug.Log("Simulate new turns");
            return new SimulatedTurn(simulatedHumanNextTurn, simulatedCpuNextTurn, this);
        }

        public List<PlayerAction> SimulateTurnOfPlayer(SimulatedPlayer player, SimulatedPlayer otherPlayer, out SimulatedPlayer outNewPlayer, out SimulatedPlayer outNewOtherPlayer)
        {
            // Fill Hand
            outNewPlayer = new SimulatedPlayer(player);
            outNewOtherPlayer= new SimulatedPlayer(otherPlayer);
            outNewPlayer.StartTurn();
            Debug.Log("Start turn");
            
            var playerActions = new List<PlayerAction>();
            playerActions.AddRange(outNewPlayer.GetInvokeCardAction());
            playerActions.AddRange(outNewPlayer.GetMoveAndSwapAction());
            playerActions.AddRange(outNewPlayer.GetAttackAction(outNewOtherPlayer));

            var actionDone = new List<PlayerAction>();
            while (playerActions.Count > 0)
            {
                // Select action
                var action = playerActions[Random.Range(0, playerActions.Count)];
                actionDone.Add(action);
                
                var mana = action.ExecuteAndGetManaCost();
                outNewPlayer.mana -= mana;
                Debug.Log("Remaining mana == " + outNewPlayer.mana);
                
                // Check for remaining action
                playerActions.Clear();
                playerActions.AddRange(outNewPlayer.GetInvokeCardAction());
                playerActions.AddRange(outNewPlayer.GetMoveAndSwapAction()); // TODO Optional
                playerActions.AddRange(outNewPlayer.GetAttackAction(outNewOtherPlayer));
            }
            
            Debug.Log("End turn");

            return actionDone;
        }

        // Utilities
        public bool IsWinner()
        {
            return simulatedHuman.health < 0;
        }

        public int turnCount;
        private float scoreCpuHealthFactor = 1.0f;
        private float scoreTurnCountFactor = 5.0f;
        
        public float GetScore()
        {
            return simulatedCpu.health * scoreCpuHealthFactor + (1 / turnCount) * scoreTurnCountFactor;
        }
    }

    public List<SimulatedTurn> _endSimulationTurn = new();

    private bool _hasFinishSimulating;
    private SimulatedTurn _turnToPlay;
    
    private IEnumerator SimulateGames()
    {
        _hasFinishSimulating = false;
        var gameManager = GameManager.Instance;
        var humanPlayer = gameManager.EnemyPlayer;

        float timer = 0.0f;

        _endSimulationTurn.Clear();
        while (timer < _maxTComputationTime)
        {
            var initialTurn = new SimulatedTurn(humanPlayer, this);
            _endSimulationTurn.Add(initialTurn.SimulateFullTurn());
            timer += Time.deltaTime;
            yield return null;
        }

        _endSimulationTurn = _endSimulationTurn.OrderBy(x => x.GetScore()).ToList();
        _turnToPlay = _endSimulationTurn[0];
        
        _hasFinishSimulating = true;
    }
    
    #endregion

    #region AI Utilities

    private List<CardController> GetCardsToInvoke(int freeSlotCount)
    {
        var possibleCardToInvoke = new List<CardController>();
        if (freeSlotCount == 0) return possibleCardToInvoke;
        
        foreach (var cardController in _cardsInHand)
        {
            if (CanDropCardOnBoard(cardController))
            {
                possibleCardToInvoke.Add(cardController);
            }
        }

        possibleCardToInvoke = possibleCardToInvoke.OrderBy(x => x.cardAttack).Reverse().ToList();

        var cardToInvoke = new List<CardController>();
        
        var mana = _currentMana;

        foreach (var card in possibleCardToInvoke)
        {
            if (mana >= card.cardManaCost)
            {
                cardToInvoke.Add(card);
                mana -= card.cardManaCost;

                if (cardToInvoke.Count >= freeSlotCount)
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }

        return cardToInvoke;
    }

    private List<SlotController> GetFreeSlots()
    {
        var result = new List<SlotController>();
        foreach (var boardSlot in _boardSlots)
        {
            if (!boardSlot.containCard)
            {
                result.Add(boardSlot);
            }
        }

        return result;
    }

    #endregion
}
