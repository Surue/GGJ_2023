// Old Skull Games
// Bernard Barthelemy
// Tuesday, January 24, 2018

using UnityEngine;
using UnityEngine.UI;

namespace OSG
{
    [ExecuteInEditMode]
    public class SetupRenderTexture : OSGMono
    {
        private RenderTexture renderTexture;
        [SerializeField] private RawImage rawImage;
        [SerializeField] private Camera renderingCamera;
        [SerializeField] private Camera[] extraCameras;


        private RectTransform it;
        private RectTransform ImageTransform => it ? it : (it = rawImage.GetComponent<RectTransform>());

        void OnEnable()
        {
            Setup();
        }

        public RenderTexture GetRenderTexture()
        {
            return renderTexture;
        }

        public Vector3 GetWorldNormalInImage(Vector3 sceneNormal)
        {
            return renderingCamera.transform.InverseTransformDirection(sceneNormal);
        }

        public Vector3 GetWorldPositionInImage(Vector3 scenePosition)
        {
            Vector3 viewportPoint = renderingCamera.WorldToViewportPoint(scenePosition);

            Rect r = ImageTransform.rect;
            Vector3 rectPos = new Vector3(Mathf.Lerp(r.xMin, r.xMax, viewportPoint.x),
                Mathf.Lerp(r.yMin, r.yMax, viewportPoint.y),0);

            return ImageTransform.TransformPoint(rectPos);
        }

        private void Setup()
        {
            if (!(rawImage && renderingCamera))
                return;

            var size = GetSize();
            if(size.x<=0)
                return;
            if(size.y<=0)
                return;

            renderTexture = new RenderTexture(size.x, size.y, 16);
            rawImage.texture = renderTexture;
            renderingCamera.targetTexture = renderTexture;
            if(extraCameras!=null)
                foreach (Camera extraCamera in extraCameras)
                {
                    extraCamera.targetTexture = renderTexture;
                }
        }

        public float GetFacingCameraFactor(Transform position)
        {
            return Vector3.Dot((renderingCamera.transform.position - position.position).normalized, 
                position.forward);
        }

        private Vector2Int GetSize()
        {
            Rect r = rawImage.GetComponent<RectTransform>().rect;
            Vector2Int size = new Vector2Int((int) r.width, (int) r.height);
            return size;
        }

#if UNITY_EDITOR
        void Update()
        {
            if(Application.isPlaying )
                return;

            if(renderTexture)
            {
                var size = GetSize();
                if(size.x != renderTexture.width || size.y != renderTexture.height)
                {
                    Setup();
                }
                return;
            }
            Setup();
        }
#endif

    }
}