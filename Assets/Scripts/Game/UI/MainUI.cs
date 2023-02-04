using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainUI : MonoBehaviour
{
    [SerializeField] private Button _nextTurn;
    
    void Start()
    {
        _nextTurn.onClick.AddListener(FindObjectOfType<HumanPlayer>().NextTurn);
    }

    void Update()
    {
        
    }
}
