using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CardScriptable))]
public class Card_Custom_Editor : Editor
{
    // SETUP INSPECTOR
    [SerializeField] private int smallSpacing = 5;
    [SerializeField] private int bigSpacing = 20;
    private bool showDebug;

    public override void OnInspectorGUI()
    {
        // SETUP DATA
        CardScriptable cardScriptable = (CardScriptable)target;

        //________SECTION - GENERAL
        //Header Foldout - GENERAL
        EditorGUILayout.Space(smallSpacing);
        EditorGUILayout.LabelField("CARD INFORMATIONS", EditorStyles.boldLabel);
        // General Infos
        cardScriptable.cardName = EditorGUILayout.TextField("Name", cardScriptable.cardName);
        cardScriptable.cardDescription = EditorGUILayout.TextField("Description", cardScriptable.cardDescription);
        cardScriptable.initialManaCost = EditorGUILayout.IntField("Mana Cost", cardScriptable.initialManaCost);
        // Type
        EditorGUILayout.Space(smallSpacing);
        EditorGUILayout.LabelField("SPECS", EditorStyles.boldLabel);
        cardScriptable.currentAbilty = (CardScriptable.SpecialAbilities)EditorGUILayout.EnumPopup("Special Ability", cardScriptable.currentAbilty);
        cardScriptable.currentType = (CardScriptable.CardType)EditorGUILayout.EnumPopup("Card Type", cardScriptable.currentType);

        //________SECTION - CHARACTER CARAC
        //Header Foldout - CHARACTER
        if (cardScriptable.currentType == CardScriptable.CardType.Character)
        {
            cardScriptable.initialHealth = EditorGUILayout.IntField("Health", cardScriptable.initialHealth);
            cardScriptable.initialAttack = EditorGUILayout.IntField("Attack", cardScriptable.initialAttack);
            cardScriptable.attackType = (CardScriptable.AttackType)EditorGUILayout.EnumPopup("Attack Type", cardScriptable.attackType);
            if (cardScriptable.attackType == CardScriptable.AttackType.Melee)
            {
                cardScriptable.attackMeleeSprite = (Sprite)EditorGUILayout.ObjectField("Melee Icon", cardScriptable.attackMeleeSprite, typeof(Sprite), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            }
            else
            {
                cardScriptable.attackDistanceSprite = (Sprite)EditorGUILayout.ObjectField("Distance Icon", cardScriptable.attackDistanceSprite, typeof(Sprite), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            }
        }

        //________SECTION - VISUAL ELEMENTS
        //Header Foldout - VISUAL
        // EditorGUILayout.Space(smallSpacing);
        // EditorGUILayout.LabelField("VISUAL ELEMENTS", EditorStyles.boldLabel);
        // cardScriptable.cardSprite = (Sprite)EditorGUILayout.ObjectField("Card Aspect", cardScriptable.cardSprite, typeof(Sprite), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));
        // cardScriptable.characterSprite = (Sprite) EditorGUILayout.ObjectField("Main Illu", cardScriptable.characterSprite, typeof(Sprite), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));
        // cardScriptable.backgroundElement = (Sprite)EditorGUILayout.ObjectField("Background Element", cardScriptable.backgroundElement, typeof(Sprite), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));
        // cardScriptable.backgroundColor = EditorGUILayout.ColorField("Background Color", cardScriptable.backgroundColor);

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
