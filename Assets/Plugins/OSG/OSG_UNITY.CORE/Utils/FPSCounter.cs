#if TM_PRO
using OSG;
using UnityEngine;
using TMPro;

//[RequireComponent(typeof(TextMeshProUGUI))]
public class FPSCounter : Singleton<FPSCounter>
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private float _timeSpan = 1.0f;
    private int _frameCount;
    private float _deltaTime;
    private float _averageFPS;

    private char[] charArray = new char[4];

    private void OnEnable()
    {
        if(text)
            text.enabled = true;
    }

    private void OnDisable()
    {
        if(text)
            text.enabled = false;
    }

    private void Update()
    {
        _deltaTime += Time.deltaTime;
        _frameCount++;

        if (_deltaTime > _timeSpan)
        {
            _averageFPS = _frameCount / _timeSpan;

            int averageInt = Mathf.CeilToInt(_averageFPS);
            if(averageInt<10000 && text)
            {
                for(int i = 4; --i >=0;)
                {
                    charArray[i] = (char) ('0' + (averageInt % 10));
                    averageInt = averageInt/10;
                }

                for(int i = 0; i < 3; ++i)
                {
                    if(charArray[i] == '0')
                    {
                        charArray[i] = ' ';
                    }
                    else
                    {
                        break;
                    }
                }

                text.SetCharArray(charArray);
                text.SetVerticesDirty();
                text.SetLayoutDirty();
            }
            
         //   text.SetText(displayString, _averageFPS);
            _frameCount = 0;
            _deltaTime = 0;
        }
    }

    protected override void Awake()
    {
        base.Awake();
#if !DISABLE_CHEAT_MANAGER
        DontDestroyOnLoad(this.gameObject);
        if (!text)
            text = GetComponent<TextMeshProUGUI>();
        if (!text)
            text = GetComponentInChildren<TextMeshProUGUI>();
        if (!text)
            Debug.LogError("Missing Component 'TextMeshProUGUI' for FPSCounter!", this);
#else
        enabled = false;
#endif
    }

}
#endif