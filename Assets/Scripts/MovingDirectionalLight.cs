using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingDirectionalLight : MonoBehaviour
{
    public GameObject mainDirectionalLight;
    [Range (0,0.1f)]
    public float translateX = 1f;
    [Range(0, 0.1f)]
    public float translateY = 1f;


    // Start is called before the first frame update
    void Start()
    {
        mainDirectionalLight = this.gameObject;
    }

    void Update()
    {
        mainDirectionalLight.transform.Translate(translateX, translateY, 0);

    }
}
