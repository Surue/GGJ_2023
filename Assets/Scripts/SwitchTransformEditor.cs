using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SwitchTransform))]
public class SwitchTransformEditor : Editor
{
    public override void OnInspectorGUI()
    {

        SwitchTransform script = (SwitchTransform)target;

        //Bouton qui permet de bake la position actuelle de l'objet en tant que "Initial Transform" donc en point de départ de l'animation
        if (GUILayout.Button("Bake Initial Transform"))
        {
            script.initialPosition = script.transform.localPosition;
            script.initialRotation = script.transform.localRotation;
            script.initialScale = script.transform.localScale;
        }

        //Bouton qui permet de bake la position actuelle de l'objet en tant que "End Transform" donc a la fin de l'animation
        if (GUILayout.Button("Bake End Transform"))
        {
            script.endPosition = script.transform.localPosition;
            script.endRotation = script.transform.localRotation;
            script.endScale = script.transform.localScale;
        }

        //Bouton qui permet d'inverser le transform initial et End
        if (GUILayout.Button("Invert Transforms"))
        {
            Vector3 tempPosition = script.initialPosition;
            script.initialPosition = script.endPosition;
            script.endPosition = tempPosition;

            Quaternion tempRotation = script.initialRotation;
            script.initialRotation = script.endRotation;
            script.endRotation = tempRotation;

            Vector3 tempScale = script.initialScale;
            script.initialScale = script.endScale;
            script.endScale = tempScale;
        }

        //Slider qui permet de prévisualiser l'animation
        script.lerpAmount = EditorGUILayout.Slider("Lerp Amount", script.lerpAmount, 0f, 1f);
        if (script.previousLerpAmount != script.lerpAmount)
        {
            script.LerpTransforms();
            script.previousLerpAmount = script.lerpAmount;
        }

        //Affichage de l'inspecteur de base
        base.OnInspectorGUI();
    }

}
