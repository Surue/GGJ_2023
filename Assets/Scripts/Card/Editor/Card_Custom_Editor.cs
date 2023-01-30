using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Card))]
public class Card_Custom_Editor : Editor
{
    // SETUP INSPECTOR
    [SerializeField] private int smallSpacing = 5;
    [SerializeField] private int bigSpacing = 20;
    private bool showDebug;

    public override void OnInspectorGUI()
    {
        // SETUP DATA
        Card card = (Card)target;

        //________SECTION - GENERAL
        //Header Foldout - GENERAL
        EditorGUILayout.Space(smallSpacing);
        EditorGUILayout.LabelField("CARD INFORMATIONS", EditorStyles.boldLabel);
        // General Infos
        card.cardName = EditorGUILayout.TextField("Name", card.cardName);
        card.cardDescription = EditorGUILayout.TextField("Description", card.cardDescription);
        card.initialManaCost = EditorGUILayout.IntField("Mana Cost", card.initialManaCost);
        // Type
        EditorGUILayout.Space(smallSpacing);
        EditorGUILayout.LabelField("SPECS", EditorStyles.boldLabel);
        card.currentAbilty = (Card.SpecialAbilities)EditorGUILayout.EnumPopup("Special Ability", card.currentAbilty);
        card.currentType = (Card.CardType)EditorGUILayout.EnumPopup("Card Type", card.currentType);

        //________SECTION - CHARACTER CARAC
        //Header Foldout - CHARACTER
        if (card.currentType == Card.CardType.Character)
        {
            card.initialHealth = EditorGUILayout.IntField("Health", card.initialHealth);
            card.initialAttack = EditorGUILayout.IntField("Attack", card.initialAttack);
            card.attackType = (Card.AttackType)EditorGUILayout.EnumPopup("Attack Type", card.attackType);
            if (card.attackType == Card.AttackType.Melee)
            {
                card.attackMeleeSprite = (Sprite)EditorGUILayout.ObjectField("Melee Icon", card.attackMeleeSprite, typeof(Sprite), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            }
            else
            {
                card.attackDistanceSprite = (Sprite)EditorGUILayout.ObjectField("Distance Icon", card.attackDistanceSprite, typeof(Sprite), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            }
        }

        //________SECTION - VISUAL ELEMENTS
        //Header Foldout - VISUAL
        EditorGUILayout.Space(smallSpacing);
        EditorGUILayout.LabelField("VISUAL ELEMENTS", EditorStyles.boldLabel);
        card.cardSprite = (Sprite)EditorGUILayout.ObjectField("Card Aspect", card.cardSprite, typeof(Sprite), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));
        card.characterSprite = (Sprite) EditorGUILayout.ObjectField("Main Illu", card.characterSprite, typeof(Sprite), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));
        card.backgroundElement = (Sprite)EditorGUILayout.ObjectField("Background Element", card.backgroundElement, typeof(Sprite), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));
        card.backgroundColor = EditorGUILayout.ColorField("Background Color", card.backgroundColor);

        //________SECTION - DEBUG
        //Header Foldout - DEBUG
        EditorGUILayout.Space(bigSpacing);
        showDebug = EditorGUILayout.Toggle("Show Debug?", showDebug);
        if (showDebug)
        {
            EditorGUILayout.Space(smallSpacing);
            EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
            base.OnInspectorGUI();
        }

        EditorUtility.SetDirty(target);
    }
}
