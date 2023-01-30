using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SwitchTransform : MonoBehaviour
{
    [Header("TriggerParameters")]
    // La distance maximale à partir de laquelle l'animation commence
    public float startTriggerDistance = 10.0f;
    // La distance minimale à partir de laquelle l'animation se termine
    public float endTriggerDistance = 5.0f;
    // Le transform de l'objet à utiliser comme trigger pour l'animation
    public Transform triggerTransform;


    [Header("AnimationCurve")]
    public AnimationCurve lerpCurve;

    [Header ("Baked Start Transform")]
    public Vector3 initialPosition;
    public Quaternion initialRotation;
    public Vector3 initialScale;

    [Header("Baked End Transform")]
    public Vector3 endPosition;
    public Quaternion endRotation;
    public Vector3 endScale;

    [HideInInspector]
    [Range(0f, 1f)]
    public float lerpAmount;
    [HideInInspector]
    [Range(0f, 1f)]
    public float previousLerpAmount;

    [HideInInspector]
    // Le transform du joueur
    public Transform playerTransform;


    private void Start()
    {
        lerpAmount = 1f;
        previousLerpAmount = 1f;
        
        GameObject playerObject = GameObject.Find("Character");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
        else
        {
            Debug.LogError("Character object is missing, it must be called Character");
        }
    }

    void Update()
    {
        // ----- ANIMATION / CONDITIONS ------
        // Calcul de la distance entre l'objet de référence et le joueur
        float distance = Vector3.Distance(triggerTransform.position, playerTransform.position);

        // Calcul de la valeur entre 0 et 1 en fonction de la distance
        // Si la distance est inférieure ou égale à la distance de début du trigger, lerpAmount est égal à 0
        if (distance >= startTriggerDistance)
        {
            lerpAmount = 0f;
        }
        // Si la distance est supérieure ou égale à la distance de fin du trigger, lerpAmount est égal à 1
        else if (distance <= endTriggerDistance)
        {
            lerpAmount = 1f;
        }
        // Sinon, lerpAmount est calculé en fonction de la distance relative entre la distance de début et de fin du trigger
        else
        {
            lerpAmount = Mathf.Clamp01((startTriggerDistance - distance) / (startTriggerDistance - endTriggerDistance));
        }

        if (previousLerpAmount != lerpAmount)
        {
            LerpTransforms();
            previousLerpAmount = lerpAmount;
        }
    }

    // ----- TOOLS ------ 
    public void BakeInitialTransform()
    {
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
        initialScale = transform.localScale;
    }

    public void BakeEndTransform()
    {
        endPosition = transform.localPosition;
        endRotation = transform.localRotation;
        endScale = transform.localScale;
    }

    public void LerpTransforms()
    {
        transform.localPosition = Vector3.Lerp(initialPosition, endPosition, lerpCurve.Evaluate(lerpAmount));
        transform.localRotation = Quaternion.Lerp(initialRotation, endRotation, lerpCurve.Evaluate(lerpAmount));
        transform.localScale = Vector3.Lerp(initialScale, endScale, lerpCurve.Evaluate(lerpAmount));
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(triggerTransform.position, startTriggerDistance);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(triggerTransform.position, endTriggerDistance);
        
        GameObject playerObject = GameObject.Find("Character");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
        else
        {
            Debug.LogError("Character object is missing, it must be called Character");
        }
    }

}
