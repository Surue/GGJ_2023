using UnityEngine;

namespace OSG
{
    public class ParallaxTransform : OSGMono
    {
        [SerializeField] private Transform anchor;
        [SerializeField] private Vector3 translateFactor;

        [Space]
        [SerializeField] private bool useMinLocalPosition;
        [SerializeField] private Vector3 minLocalPosition;

        [Space]
        [SerializeField] private bool useMaxLocalPosition;
        [SerializeField] private Vector3 maxLocalPosition;

        // Cache
        private bool initialized;
        private Vector3 anchorStartPosition;
        private Vector3 startPosition;

        private void Start()
        {
            if (anchor)
                Initialize();    
        }

        private void LateUpdate()
        {
            if (!initialized)
                return;

            Vector3 targetLocalPosition = startPosition + (anchor.position - anchorStartPosition).Multiply(translateFactor);

            // Clamp minimum position
            if (useMinLocalPosition)
            {
                if (targetLocalPosition.x < minLocalPosition.x)
                    targetLocalPosition.x = minLocalPosition.x;

                if (targetLocalPosition.y < minLocalPosition.y)
                    targetLocalPosition.y = minLocalPosition.y;

                if (targetLocalPosition.z < minLocalPosition.z)
                    targetLocalPosition.z = minLocalPosition.z;
            }

            // Clamp max position
            if (useMaxLocalPosition)
            {
                if (targetLocalPosition.x > maxLocalPosition.x)
                    targetLocalPosition.x = maxLocalPosition.x;

                if (targetLocalPosition.y > maxLocalPosition.y)
                    targetLocalPosition.y = maxLocalPosition.y;

                if (targetLocalPosition.z > maxLocalPosition.z)
                    targetLocalPosition.z = maxLocalPosition.z;
            }

            transformCached.localPosition = targetLocalPosition;
        }

        private void Initialize()
        {
            anchorStartPosition = anchor.position;
            startPosition = transformCached.localPosition;

            initialized = true;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying)
                return;

            if (!initialized && anchor)
                Initialize();
        }
#endif
    }
}