using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Deck", menuName = "ScriptableObjects/Deck", order = 1)]
public class DeckScriptable : ScriptableObject
{
    [Serializable]
    public class PairCardQuantity
    {
        public CardScriptable cardScriptable;
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
}
