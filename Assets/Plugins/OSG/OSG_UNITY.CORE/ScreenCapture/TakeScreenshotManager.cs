// Old Skull Games
// Antoine Pastor
// 03/01/2019
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


namespace OSG
{
    public class TakeScreenshotManager : MonoBehaviour
    {
        private enum eCaptureMode
        {
            screen,
            camera
        }
        [SerializeField] private eCaptureMode captureMode;
        [ConditionalHideFunction("CaptureModeIsCamera", true)]
        [SerializeField] private SetupRenderTexture setupRenderTexture;
        
        [SerializeField] private GameObject[] objectsToHide; //Things you don't want on the screenshot (buttons, UI, ...)
        [SerializeField] private GameObject[] screenshotWaterMarks; //Extra things you want to have on the screenshot (game logo, ...)  
        
        [SerializeField] private RawImage rawImage;
        [SerializeField] private AspectRatioFitter screenshotSAspectRatioFitter;

        [SerializeField] private float waitDelay = 0f;

        [SerializeField] private UnityEvent OnScreenshotWillBeTaken;
        [SerializeField] private UnityEvent OnScreenshotHasBeenTaken;
        [SerializeField] private UnityEvent OnScreenshotSaved;


        
        private float _waitTime = 4f;
        private float _time;
        private byte[] _byteData;
        private bool _isSharing;
        private string screenshotName = "screenshot.png";
        private Texture2D _texture2D;
    
        //TODO : why the string builder ????
        private StringBuilder screenShotPathSB;
        private Sprite _sprite;
    
        void Init()
        {
    
            screenShotPathSB = new StringBuilder(Path.Combine(Application.temporaryCachePath, screenshotName));
            
            float ratio = (float)Screen.width / Screen.height; 
            if (screenshotSAspectRatioFitter) screenshotSAspectRatioFitter.aspectRatio = ratio;
    
            _texture2D = new Texture2D(Screen.width, Screen.height);
        }
    
        private void OnValidate()
        {
            if (captureMode == eCaptureMode.camera && setupRenderTexture != null)
            {
//                if (setupRenderTexture.ca.targetTexture == null)
//                    Debug.LogError("The camera you capture with must render to a texture");
            }
        }
    
        protected bool CaptureModeIsCamera()
        {
            return captureMode == eCaptureMode.camera;
        }
        
        public void TakeScreenshot()
        {
            StartCoroutine(TakeAndSaveScreenshot());
        }
    
        public void OnEnable()
        {
            Init();
        }
    
    
        public void ShareScreenshot()
        {
            if (!_isSharing)
            {
                StartCoroutine(NativeSharingCoroutine());
            }
            else
            {
                Debug.Log("Sharing processing...");
            }
        }
    
        IEnumerator NativeSharingCoroutine()
        {
    #if UNITY_EDITOR
           yield return null;
    #endif
            _isSharing = true;

            int retries = 0;
            while (!File.Exists(screenShotPathSB.ToString()) )
            {
                yield return new WaitForSeconds(.1f);
                if(retries++>20)
                {
                    Debug.LogError("screen shot file not created. abort.");
                    yield break;
                }
            }

            Debug.Log($"File {screenShotPathSB.ToString()} created. sharing");

            NativeShare.Share("", screenShotPathSB.ToString(), "", "", "image/png", true, "");
            //GameEventSystem.Events.shareButtonPressed.Invoke();
            yield return null;
            _isSharing = false;
        }
    
    
        IEnumerator TakeAndSaveScreenshot()
        {
            if (File.Exists(screenShotPathSB.ToString()))
            {
                Debug.Log("Delete old screenshot");
                File.Delete(screenShotPathSB.ToString());
            }
            
            Debug.Log("entering saving screenshot coroutine : " + screenShotPathSB);
            OnScreenshotWillBeTaken.Invoke();
            SetObjectsVisibility(false);

            yield return new WaitForSeconds(waitDelay);

            SetWatermarksVisibility(true);

            yield return new WaitForEndOfFrame();

            if (captureMode == eCaptureMode.screen)
            {
//#if UNITY_EDITOR
                ScreenCapture.CaptureScreenshot(screenShotPathSB.ToString());
//#elif UNITY_ANDROID || UNITY_IOS
//                ScreenCapture.CaptureScreenshot(screenshotName);
//#endif
            }
            else
            {
                if (captureMode == eCaptureMode.camera)
                {
                    yield return SaveRenderTextureScreenshotCoroutine(screenShotPathSB.ToString(), setupRenderTexture.GetRenderTexture());
                }
            }
            
            yield return new WaitForEndOfFrame();
            SetObjectsVisibility(true);
            SetWatermarksVisibility(false);
            OnScreenshotHasBeenTaken.Invoke();
            
            Debug.Log("screenshot path after wait end of frame : " + screenShotPathSB);
    
            _time = Time.time + _waitTime;
            
            while (!File.Exists(screenShotPathSB.ToString()))
            {
                if (_time <= Time.time)
                {
                    Debug.Log("too much time to create a screenshot");
                    break;
                }
    
                yield return new WaitForEndOfFrame();
            }
    
            Debug.Log("image should be loaded file exists ? " + File.Exists(screenShotPathSB.ToString()));
    //        Debug.Log("file locked after cor : " + IsFileLocked(fileInfo));
#if UNITY_IOS
            Debug
                .Log("wait a few frames");
        for (int i = 0; i < 5; i++)
            {
                yield return null;
            }
#endif
    
            _byteData = File.ReadAllBytes(screenShotPathSB.ToString());
    
            Debug.Log("byte data size " + _byteData.Length);
    
    
            if (_byteData == null)
            {
                Debug.Log("file not exist after trying to take a screenshot");
                yield break;
            }
    
            yield return CreateSprite();
    
            OnScreenshotSaved.Invoke();
        }
    
        public  IEnumerator SaveRenderTextureScreenshotCoroutine(string path, RenderTexture renderTexture)
        {
            RenderTexture.active = renderTexture;
            Texture2D newTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
            newTexture.ReadPixels(new Rect(0,0,renderTexture.width, renderTexture.height), 0,0,false);
            RenderTexture.active = null;
            byte[] bytes = newTexture.EncodeToPNG();            
            File.WriteAllBytes(path, bytes);
            yield return null;
        }
    
        IEnumerator CreateSprite()
        {
            Debug.Log("creating sprite");
            _texture2D.LoadImage(_byteData);
            rawImage.texture = _texture2D;
            yield return null;
        }
    
        private void SetObjectsVisibility(bool visible)
        {
            foreach (var obj in objectsToHide)
            {
                obj.SetActive(visible);
            }        
        }
        
        private void SetWatermarksVisibility(bool visible)
        {
            foreach (var obj in screenshotWaterMarks)
            {
                obj.SetActive(visible);
            }        
        }
    }
       

}
