// Old Skull Games
// Bernard Barthelemy
// Monday, June 25, 2018

using System.Collections;
using OSG.Core.EventSystem;
using UnityEngine;

namespace OSG
{
    public abstract class CameraShaker : OSGMono
    {
        [SerializeField] protected ShakeParameters defaultShake;
        private EventSystemRef<CameraShakeEventContainer, CoreEventSystem> eventSystem = new EventSystemRef<CameraShakeEventContainer, CoreEventSystem>();
        private Vector3? baseShakePosition;
        protected Vector3 currentShake;
        private int _sk;
        protected int shakeCount
        {
            get
            {
                return _sk;
            }
            set
            {
                _sk = value;
                enabled = _sk>0;
            }
        }

        private void Awake()
        {
            using (var events = eventSystem.RegisterContext(this))
            {
                InitEvents(events);
            }

            currentShake = Vector3.zero;
            shakeCount = 0;
        }

        protected virtual void InitEvents(CameraShakeEventContainer events)
        {
            events.cameraShake.AddListener(Shake);
            events.cameraStopShake.AddListener(StopShake);
        }

        protected void StopShake()
        {
            if (baseShakePosition.HasValue)
            {
                transform.position = baseShakePosition.Value;
            }
            shakeCount = 0;
            baseShakePosition = null;
            currentShake = Vector3.zero;
            StopAllCoroutines();
        }

        
        private void OnDisable()
        {
            StopShake();
        }

        private void OnDestroy()
        {
            eventSystem.UnregisterFromAllEvents(this);
        }

        private void Update()
        {
            if(!baseShakePosition.HasValue)
                return;
            transform.position = baseShakePosition.Value + currentShake;
            currentShake = Vector3.zero;
        }

        protected void Shake(ShakeParameters parameters)
        {
            if(!parameters)
                parameters = defaultShake;

            if (!baseShakePosition.HasValue)
                baseShakePosition = transform.position;

            StartCoroutine(ShakeCoroutine(parameters));
        }

        protected virtual IEnumerator WaitForDelay(int timeInMili)
        {
            yield return new WaitForSeconds(timeInMili/1000f);
        }


        protected IEnumerator ShakeCoroutine(ShakeParameters parameters)
        {
            ++shakeCount;
            float horizontalAmplitude = parameters.horizontalRange.Value;
            float verticalAmplitude = parameters.verticalRange.Value;

            float progress = 1000f/parameters.duration;
            
            for(;;)
            {
                for(float time = 0; time < 1; time += progress*Time.deltaTime)
                {
                    Vector3 v = new Vector2(
                        horizontalAmplitude * parameters.horizontalMove.Evaluate(time),
                        verticalAmplitude * parameters.verticalMove.Evaluate(time));
                    currentShake += v;
                    yield return null;
                }
                if(parameters.delayBeforeRepeat > 0)
                {
                    yield return WaitForDelay(parameters.delayBeforeRepeat);
                }
                else
                {
                    break;
                }
            }
            --shakeCount;
        }
    }
}
