using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "Deck", menuName = "ScriptableObjects/Deck", order = 1)]
public class DeckScriptable : ScriptableObject
{
    [Serializable]
    public class PairCardQuantity
    {
        public GameObject cardPrefab;
        public int cardQuantity;
    }

    [SerializeField] private List<PairCardQuantity> _cards;
    
    public int GetTotalNumberOfCard()
    {
        int count = 0;

        foreach (var pairCardQuantity in _cards)
        {
            count += pairCardQuantity.cardQuantity;
        }

        return count;
    }

    public void FillList(ref Queue<CardController> refCardList, Transform deckTransform)
    {
        List<CardController> shuffledCards = new List<CardController>();
        foreach (var pairCardQuantity in _cards)
        {
            for (int i = 0; i < pairCardQuantity.cardQuantity; i++)
            {
                var instance = Instantiate(pairCardQuantity.cardPrefab);
                var cardController = instance.GetComponent<CardController>();

                cardController.Setup(deckTransform);
                shuffledCards.Add(cardController);
            }
        }

        shuffledCards = shuffledCards.OrderBy(x => Random.value).ToList();
        foreach (var cardController in shuffledCards)
        {
            refCardList.Enqueue(cardController);
        }
    }
}
