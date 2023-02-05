// Old Skull Games
// Bernard Barthelemy
// Wednesday, December 5, 2018

using System;
using OSG.DebugTools;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace OSG
{
    public abstract  class RenderedInTextureClickable: OSGMono
    {
        [SerializeField] protected SetupRenderTexture setup;
        [SerializeField] private float radius = 1;
        [SerializeField] private float facingCamera = 0;
        [SerializeField] public UnityEvent onClick;
        [SerializeField] public UnityEvent onPressed;
        [SerializeField] public UnityEvent onReleased;
        [SerializeField] public float maxMouseMoveRatio = 1;
        [SerializeField] public float maxDownDurationForClick = float.MaxValue;

        private bool? wasOnWhenPressed;
        private Vector2 downPosition;
        private float downTime;
        
        private bool IsFacingCamera()
        {
            float f = setup.GetFacingCameraFactor(transform);
            return f >= facingCamera;
        }


#if UNITY_EDITOR

        private Renderer myRenderer;

        delegate void DrawArcDelegate(Vector3 center, Vector3 normal, Vector3 fram, float angle, float radius);

        private void OnDrawGizmosSelected()
        {
            if (!setup)
                return;
            
            Vector3 position = setup.GetWorldPositionInImage(transform.position);
            Handles.Label(position, setup.GetFacingCameraFactor(transform).ToString("0.00"));

            bool facingUs = IsFacingCamera();
            if(!facingUs)
                return;

            Handles.color = Color.magenta;
            if (!myRenderer)
                myRenderer = GetComponentInChildren<MeshRenderer>();
            else
            {
                myRenderer.bounds.Draw(Handles.color, 0.5f);
            }
            
            DrawArcDelegate drawArc =
                wasOnWhenPressed.HasValue && wasOnWhenPressed.Value ?
                (DrawArcDelegate)Handles.DrawSolidArc : Handles.DrawWireArc;

            drawArc(position, Vector3.forward, Vector3.up, 360, radius);

            Handles.color = Color.white - Handles.color;
        }
#endif
        protected virtual void Update()
        {
            if (!(IsFacingCamera() || wasOnWhenPressed.HasValue))
                return;

            var p = setup.GetWorldPositionInImage(transform.position);
            Ray pointToRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector2 delta = p - pointToRay.origin;
            var r2 = radius * radius;
            bool isOn = delta.sqrMagnitude < r2;
            if(Input.GetMouseButton(0))
            {
                if(wasOnWhenPressed.HasValue == false)
                {
                    downPosition = pointToRay.origin;
                    downTime = Time.time;
                    wasOnWhenPressed = isOn;
                    if(isOn)
                    {
                        onPressed?.Invoke();
                    }
                }
            }
            else
            {
                if(wasOnWhenPressed.HasValue) 
                {
                    if (isOn && wasOnWhenPressed.Value)
                    {
                        float elapsedTime = Time.time - downTime;
                        if (elapsedTime < maxDownDurationForClick)
                        {
                            delta = downPosition - (Vector2)pointToRay.origin;
                            if (delta.sqrMagnitude < r2 * maxMouseMoveRatio * maxMouseMoveRatio)
                            {
                                onClick?.Invoke();
                            }
                        }
                    }
                    onReleased?.Invoke();
                    wasOnWhenPressed = null;
                }
            }
        }
    }
}