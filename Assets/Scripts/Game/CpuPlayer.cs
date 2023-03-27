using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class CpuPlayer : Player
{
    protected override EPlayerType GetPlayerType() => EPlayerType.CPU;
    
    public List<SimulatedTurn> _endSimulationTurn = new();

    private bool _hasFinishSimulating = false;
    private SimulatedTurn _turnToPlay;

    [SerializeField] private AiDescriptionScriptable _aiDescriptionScriptable;

    private enum ECpuPhase
    {
        WaitEndSimulation,
        PlayAllActions,
        WaitEndAllAction,
        End
    }

    private ECpuPhase _phase = ECpuPhase.WaitEndSimulation;

    private void Start()
    {
        GameManager.Instance.onCpuTurnStarted += StartTurn;
        GameManager.Instance.onCpuTurnFinished += EndTurn;

        _isPlaying = false;
    }

    private void Update()
    {
        if(!_isPlaying) return;

        switch (_phase)
        {
            case ECpuPhase.WaitEndSimulation:
                if (_hasFinishSimulating)
                {
                    _phase = ECpuPhase.PlayAllActions;
                }
                break;
            case ECpuPhase.PlayAllActions:
                _phase = ECpuPhase.WaitEndAllAction;
                StartCoroutine(PlayAllAction());
                break;
            case ECpuPhase.WaitEndAllAction:
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
                var cards = new List<CardController>();
                foreach (var simulatedCard in attackOtherCardAction.cardsToAttack)
                {
                    cards.Add(simulatedCard.cardController);
                }
                yield return StartCoroutine(DuelOtherCards(cardController, cards));
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
        FillHand();
        ResetCardStartTurn();
        
        _isPlaying = true;

        _phase = ECpuPhase.WaitEndSimulation;
        
        GameManager.Instance.HasFinishedStartingTurn();

        StartCoroutine(SimulateGames());
    }
    
    private void EndTurn()
    {
        _isPlaying = false;
        
        GameManager.Instance.NextTurn();
    }
    
    #region Game Simulation

    public class SimulatedCard
    {
        public int health;
        public int damage;
        public int manaCost;
        public int remainingAttackCount;

        public SimulatedSlot simulatedSlot;

        public CardController cardController;

        public SimulatedCard(CardController card)
        {
            health = card.cardHealth;
            damage = card.cardAttack;
            manaCost = card.cardManaCost;

            cardController = card;
            remainingAttackCount = card.CardScriptable.AttackScriptable.AttackCharge;
        }

        public void TakeDamage(SimulatedCard attackingCard)
        {
            if (attackingCard == null)
            {
                return;
            }
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
        public int minimumCardInHand;
        
        public List<SimulatedCard> cardsInHand = new ();
        public Queue<SimulatedCard> cardsInDeck = new();
        public List<SimulatedCard> cardsOnBoard = new();

        public List<SimulatedSlot> simulatedSlots = new();

        public GameRulesScriptables gameRulesScriptable;

        public EPlayerType playerType;

        public int cardLostCount = 0;
        public int cardDestroyedCount = 0;
        
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

            gameRulesScriptable = player.GameRulesScriptable;
            minimumCardInHand = gameRulesScriptable.MaxCardInHand;
            
            playerType = player.PlayerType;
        }
        
        public SimulatedPlayer(SimulatedPlayer player)
        {
            minimumCardInHand = player.minimumCardInHand;
        
            cardsInHand = player.cardsInHand.ToList();
            cardsInDeck = new Queue<SimulatedCard>(player.cardsInDeck);
            cardsOnBoard = new List<SimulatedCard>(player.cardsOnBoard);

            simulatedSlots = player.simulatedSlots; 

            gameRulesScriptable = player.gameRulesScriptable;
            
            cardLostCount = player.cardLostCount;
            cardDestroyedCount = player.cardDestroyedCount;

            playerType = player.playerType;
        }

        public void StartTurn()
        {
            // Fill hand
            if (cardsInHand.Count < minimumCardInHand)
            {
                for(int i = 0; i < minimumCardInHand - cardsInHand.Count; i++)
                {
                    cardsInHand.Add(cardsInDeck.Dequeue());
                }
            }

            // Reset remaining attack
            foreach (var simulatedCard in cardsOnBoard)
            {
                simulatedCard.remainingAttackCount = simulatedCard.cardController.CardScriptable.AttackScriptable.AttackCharge;
            }
        }
        
        public List<PlayerAction> GetInvokeCardAction(GameState gameState)
        {
            var possibleCardToInvoke = new List<SimulatedCard>();
            
            foreach (var cardController in cardsInHand)
            {
                if (gameState.CanPlayerDropCardOnBoard(playerType, cardController.manaCost))
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
                if(slot.isFront) continue;
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

        public List<SimulatedSlot> GetPossibleSlot(SimulatedCard cardToMove)
        {
            if (cardToMove.cardController.CardScriptable.MovementDescriptionScriptable == null)
                return null;
        
            var columnID = cardToMove.simulatedSlot.columnID;

            var lineOffset = cardToMove.simulatedSlot.isFront ? 0 : 4;

            var lineMaxMovement = cardToMove.cardController.CardScriptable.MovementDescriptionScriptable.MaxMovementLineCount;

            if (lineMaxMovement < 0)
            {
                lineMaxMovement = int.MaxValue;
            }

            var result = new List<SimulatedSlot>();
            // Left
            for (int i = columnID - 1, count = 0; i >= 0 && count < lineMaxMovement; i--, count++)
            {
                result.Add(simulatedSlots[i + lineOffset]);
            }
        
            // Right
            for (int i = columnID + 1, count = 0; i < 4 && count < lineMaxMovement; i++, count++)
            {
                result.Add(simulatedSlots[i + lineOffset]);
            }

            if (cardToMove.cardController.CardScriptable.MovementDescriptionScriptable.MaxMovementColumnCount > 0)
            {
                if (cardToMove.simulatedSlot.isFront)
                {
                    result.Add(simulatedSlots[columnID + 4]);
                }
                else
                {
                    result.Add(simulatedSlots[columnID]);
                }
            }

            return result;
        }
        
        public List<PlayerAction> GetMoveAndSwapAction(GameState gameState)
        {
            var playerActions = new List<PlayerAction>();
            
            foreach (var card in cardsOnBoard)
            {
                // card cannot move if it has used all its attack
                if(card.remainingAttackCount <= 0) continue;

                var possibleSlots = GetPossibleSlot(card);
                foreach (var slot in possibleSlots)
                {
                    if (slot.HasCard() && gameState.CanPlayerSwapCard(playerType)) // Swap
                    {
                        playerActions.Add(new SwapCardsPlayerAction()
                        {
                            cardToMove = card,
                            cardToSwap = slot.simulatedCard,
                            gameRulesScriptable = gameRulesScriptable
                        });
                    }
                    else if (!slot.HasCard() && gameState.CanPlayerMoveCard(playerType)) // Move
                    {
                        playerActions.Add(new MovePlayerAction()
                        {
                            cardToMove = card,
                            newSlot = slot,
                            gameRulesScriptable = gameRulesScriptable
                        });
                    }
                }
            }

            return playerActions;
        }
        
        public List<PlayerAction> GetAttackAction(SimulatedPlayer otherPlayer, GameState gameState)
        {
            var playerActions = new List<PlayerAction>();
            foreach (var attackingCard in cardsOnBoard)
            {
                if (!CanAttack(attackingCard)) continue;
                    
                if (CardHasTarget(attackingCard, otherPlayer, out var cardsToAttack))
                {
                    
                    playerActions.Add(new AttackOtherAction()
                    {
                        attackingCard = attackingCard,
                        cardsToAttack = cardsToAttack,
                        attackingPlayer = this,
                        defendingPlayer = otherPlayer
                    });
                }else if (attackingCard.simulatedSlot.isFront)
                {
                    playerActions.Add(new AttackOtherPlayerAction()
                    {
                        attackingCard = attackingCard,
                        gameState = gameState,
                        defendingPlayerType = otherPlayer.playerType
                    });
                }
            }

            return playerActions;
        }

        private bool CardHasTarget(SimulatedCard attackingCard, SimulatedPlayer otherPlayer, out List<SimulatedCard> cardToAttack)
        {
            bool hasTarget = false;
            cardToAttack = new List<SimulatedCard>();
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
                        hasTarget = true;
                    }
                    break;
                case EAttackType.FrontAndBack:
                    cardToAttack.AddRange(TryGetFrontAndBack(attackingCard));
                    if (cardToAttack.Count > 0)
                    {
                        hasTarget = true;
                    }
                    break;
                case EAttackType.FrontLine:
                    if (!TryGetCardFacing(attackingCard, out var _))
                    {
                        hasTarget = false;
                    }
                    else
                    {
                        cardToAttack.AddRange(TryGetFrontLine(attackingCard));
                        if (cardToAttack.Count > 0)
                        {
                            hasTarget = true;
                        }
                    }
                    break;
                case EAttackType.NoAttack:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return hasTarget;
        }

        public bool CanAttack(SimulatedCard simulatedCard)
        {
            return simulatedCard.remainingAttackCount > 0 &&
                   (simulatedCard.simulatedSlot.isFront || simulatedCard.cardController.CardScriptable.AttackScriptable.AttackType == EAttackType.FrontAndBack) && 
                   simulatedCard.cardController.CardScriptable.AttackScriptable.AttackType != EAttackType.NoAttack;
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
            
            return cardToInvoke.manaCost;
        }
    }
    
    public class MovePlayerAction : PlayerAction
    {
        public SimulatedCard cardToMove;
        public SimulatedSlot newSlot;
        public GameRulesScriptables gameRulesScriptable;
        
        public override int ExecuteAndGetManaCost()
        {
            cardToMove.simulatedSlot.simulatedCard = null;

            cardToMove.simulatedSlot = newSlot;
            newSlot.simulatedCard = cardToMove;
            
            return gameRulesScriptable.CardMoveManaCost;
        }
    }

    public class SwapCardsPlayerAction : PlayerAction
    {
        public SimulatedCard cardToMove;
        public SimulatedCard cardToSwap;
        public GameRulesScriptables gameRulesScriptable;
        
        public override int ExecuteAndGetManaCost()
        {
            var tmpSlot = cardToMove.simulatedSlot;

            cardToMove.simulatedSlot = cardToSwap.simulatedSlot;
            cardToMove.simulatedSlot.simulatedCard = cardToMove;

            cardToSwap.simulatedSlot = tmpSlot;
            tmpSlot.simulatedCard = cardToSwap;

            return gameRulesScriptable.CardSwapManaCost;
        }
    }
    
    public class AttackOtherAction : PlayerAction
    {
        public SimulatedCard attackingCard;
        public List<SimulatedCard> cardsToAttack;
        public SimulatedPlayer attackingPlayer;
        public SimulatedPlayer defendingPlayer;
        
        public override int ExecuteAndGetManaCost()
        {
            attackingCard.remainingAttackCount--;

            SimulatedCard defendingCard = null;
            if (cardsToAttack.Count > 1)
            {
                foreach (var simulatedCard in cardsToAttack)
                {
                    if (simulatedCard.IsFacing(attackingCard))
                    {
                        defendingCard = simulatedCard;
                    }
                }
            }
            else
            {
                defendingCard = cardsToAttack[0];
            }

            attackingCard.TakeDamage(defendingCard);

            foreach (var simulatedCard in cardsToAttack)
            {
                simulatedCard.TakeDamage(attackingCard);
                
                if (simulatedCard.IsDead())
                {
                    simulatedCard.simulatedSlot.simulatedCard = null;
                    defendingPlayer.cardsOnBoard.Remove(simulatedCard);
                    defendingPlayer.cardsInDeck.Enqueue(simulatedCard);
                    
                    defendingPlayer.cardLostCount++;
                    attackingPlayer.cardDestroyedCount++;
                }
            }
            
            if (attackingCard.IsDead())
            {
                attackingCard.simulatedSlot.simulatedCard = null;
                attackingPlayer.cardsOnBoard.Remove(attackingCard);
                attackingPlayer.cardsInDeck.Enqueue(attackingCard);

                attackingPlayer.cardLostCount++;
                defendingPlayer.cardDestroyedCount++;
            }
                
            return 0;
        }
    }
    
    public class AttackOtherPlayerAction : PlayerAction
    {
        public SimulatedCard attackingCard;
        public EPlayerType defendingPlayerType;
        public GameState gameState;
        
        public override int ExecuteAndGetManaCost()
        {
            attackingCard.remainingAttackCount--;
            gameState.PlayerTakeDamage(defendingPlayerType, attackingCard.damage);
            return 0;
        }
    }
    
    public class SimulatedTurn : GameState
    {
        // Tree
        public SimulatedTurn parentSimulatedTurn;
     
        // Global values
        public GameRulesScriptables gameRulesScriptables;
        public DeckScriptable simulatedDeck;
        
        // Values
        public SimulatedPlayer simulatedHuman;
        public SimulatedPlayer simulatedCpu;

        // Selected action
        public List<PlayerAction> actions = new();
        
        // Scoring
        public int turnCount;
        private AiDescriptionScriptable _aiDescriptionScriptable;
        
        // In progress
        public GameState nextTurnGameState;

        // Constructors
        public SimulatedTurn(Player humanPlayer, Player cpuPlayer, AiDescriptionScriptable aiDescriptionScriptable)
        {
            gameRulesScriptables = humanPlayer.GameRulesScriptable;
            simulatedDeck = humanPlayer.DeckScriptable;
            simulatedDeck = humanPlayer.DeckScriptable;
            
            simulatedHuman = new SimulatedPlayer(humanPlayer); // TODO Cheat by knowing all player cards ahead of time
            simulatedCpu = new SimulatedPlayer(cpuPlayer);

            parentSimulatedTurn = this;
            turnCount = 0;

            _aiDescriptionScriptable = aiDescriptionScriptable;

            var rules = humanPlayer.GameRulesScriptable;
            base.humanPlayer = new PlayerState()
            {
                boardState = new BoardState()
                {

                },
                cardMoveCost = rules.CardMoveManaCost,
                cardSwapCost = rules.CardSwapManaCost,
                deckState = new DeckState()
                {

                },
                health = humanPlayer.CurrentHealth,
                mana = humanPlayer.CurrentMana,
                manaNextTurn = humanPlayer.CurrentMana + 1, // Human has already played
                maximumMana = rules.MaxMana,
                minimumCardInHand = rules.MaxCardInHand,
                turn = 0,
                type = EPlayerType.Human
            };
            
            base.cpuPlayer = new PlayerState()
            {
                boardState = new BoardState()
                {

                },
                cardMoveCost = rules.CardMoveManaCost,
                cardSwapCost = rules.CardSwapManaCost,
                deckState = new DeckState()
                {

                },
                health = cpuPlayer.CurrentHealth,
                mana = cpuPlayer.CurrentMana,
                manaNextTurn = cpuPlayer.CurrentMana,
                maximumMana = rules.MaxMana,
                minimumCardInHand = rules.MaxCardInHand,
                turn = 0,
                type = EPlayerType.Human
            };

            nextTurnGameState = new GameState()
            {
                humanPlayer = base.humanPlayer,
                cpuPlayer = base.cpuPlayer
            };
        }
        
        public SimulatedTurn(SimulatedPlayer humanPlayer, SimulatedPlayer cpuPlayer, SimulatedTurn parentTurn, AiDescriptionScriptable aiDescriptionScriptable, GameState newGameState)
        {
            simulatedHuman = humanPlayer;
            simulatedCpu = cpuPlayer;

            gameRulesScriptables = parentTurn.gameRulesScriptables;
            simulatedDeck = parentTurn.simulatedDeck;

            parentSimulatedTurn = parentTurn.parentSimulatedTurn;
            turnCount = parentTurn.turnCount + 1;

            _aiDescriptionScriptable = aiDescriptionScriptable;
            
            var rules = gameRulesScriptables;
            base.humanPlayer = newGameState.humanPlayer;

            base.cpuPlayer = newGameState.cpuPlayer;
            
            nextTurnGameState = new GameState()
            {
                humanPlayer = base.humanPlayer,
                cpuPlayer = base.cpuPlayer
            };
        }
        
        // Simulation
        public SimulatedTurn SimulateFullTurn()
        {
            // CPU turn
            cpuPlayer.StartTurn();
            nextTurnGameState.cpuPlayer.StartTurn();
            actions = new List<PlayerAction>(SimulateTurnOfPlayer(simulatedCpu, simulatedHuman, out var simulatedCpuNextTurn, out var simulatedHumanNextTurn));

            // Human turn
            humanPlayer.StartTurn();
            nextTurnGameState.humanPlayer.StartTurn();
            SimulateTurnOfPlayer(simulatedHumanNextTurn, simulatedCpuNextTurn, out simulatedHumanNextTurn, out simulatedCpuNextTurn);
            
            if (nextTurnGameState.IsPlayerDead(EPlayerType.CPU) || nextTurnGameState.IsPlayerDead(EPlayerType.Human))
            {
                var finalTurn = new SimulatedTurn(simulatedHumanNextTurn, simulatedCpuNextTurn, this, _aiDescriptionScriptable, nextTurnGameState);
                return finalTurn;
            }

            return new SimulatedTurn(simulatedHumanNextTurn, simulatedCpuNextTurn, this, _aiDescriptionScriptable, nextTurnGameState).SimulateFullTurn();
        }

        public List<PlayerAction> SimulateTurnOfPlayer(SimulatedPlayer player, SimulatedPlayer otherPlayer, out SimulatedPlayer outNewPlayer, out SimulatedPlayer outNewOtherPlayer)
        {
            // Fill Hand
            outNewPlayer = new SimulatedPlayer(player);
            outNewOtherPlayer = new SimulatedPlayer(otherPlayer);
            outNewPlayer.StartTurn();
            
            var playerActions = new List<PlayerAction>();
            playerActions.AddRange(outNewPlayer.GetInvokeCardAction(this));
            playerActions.AddRange(outNewPlayer.GetMoveAndSwapAction(this));
            playerActions.AddRange(outNewPlayer.GetAttackAction(outNewOtherPlayer, nextTurnGameState));

            var actionDone = new List<PlayerAction>();
            while (playerActions.Count > 0)
            {
                // Select action
                var action = playerActions[Random.Range(0, playerActions.Count)];
                actionDone.Add(action);
                
                var mana = action.ExecuteAndGetManaCost();
                PlayerUseMana(outNewPlayer.playerType, mana);
                
                // Check for remaining action
                playerActions.Clear();
                playerActions.AddRange(outNewPlayer.GetInvokeCardAction(this));
                playerActions.AddRange(outNewPlayer.GetMoveAndSwapAction(this)); // TODO Optional
                playerActions.AddRange(outNewPlayer.GetAttackAction(outNewOtherPlayer, nextTurnGameState));
            }

            return actionDone;
        }
        
        public float GetScore()
        {
            return (GetPlayerHealth(EPlayerType.CPU) <= 0 ? 0 : GetPlayerHealth(EPlayerType.CPU) * _aiDescriptionScriptable.ScoreCpuHealthFactor) +
                   (GetPlayerHealth(EPlayerType.CPU) <= 0 ? 0 : (1.0f / turnCount) * _aiDescriptionScriptable.ScoreTurnCountFactor) +
                   (simulatedCpu.cardLostCount * _aiDescriptionScriptable.ScoreCardLostCountFactor) +
                   (simulatedCpu.cardDestroyedCount + _aiDescriptionScriptable.ScoreCardDestroyedFactor);
        }
    }
    
    private IEnumerator SimulateGames()
    {
        _hasFinishSimulating = false;
        var gameManager = GameManager.Instance;
        var humanPlayer = gameManager.EnemyPlayer;

        float timer = 0.0f;

        _endSimulationTurn.Clear();
        while (timer < _aiDescriptionScriptable.MaxTComputationTime)
        {
            var initialTurn = new SimulatedTurn(humanPlayer, this, _aiDescriptionScriptable);
            _endSimulationTurn.Add(initialTurn.SimulateFullTurn());
            timer += Time.deltaTime;
            yield return null;
        }

        _endSimulationTurn = _endSimulationTurn.OrderBy(x => x.GetScore()).ToList();
        _turnToPlay = _endSimulationTurn[^1];

        var bestScore = _endSimulationTurn[^1].GetScore();
        _endSimulationTurn = _endSimulationTurn.OrderBy(x => x.turnCount).ToList();
        var lowestTurn = _endSimulationTurn[0].turnCount;
        
        Debug.Log("Best score " + bestScore);
        Debug.Log("Lowest turn " + lowestTurn);

        // foreach (var simulatedTurn in _endSimulationTurn)
        // {
        //     Debug.Log(simulatedTurn.GetScore() + " in " + simulatedTurn.turnCount);
        // }
        Debug.Log("Computed simulation = " + _endSimulationTurn.Count);
        
        _hasFinishSimulating = true;
    }
    
    #endregion
}
