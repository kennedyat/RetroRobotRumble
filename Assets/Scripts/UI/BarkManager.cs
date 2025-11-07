using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(VerticalLayoutGroup))]
public class BarkManager : MonoBehaviour
{
    private VerticalLayoutGroup layoutGroup;

    void Start()
    {
        layoutGroup = GetComponent<VerticalLayoutGroup>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("ONE");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("TWO");
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("THREE");
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Debug.Log("FOUR");
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            Debug.Log("FIVE");
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            Debug.Log("SIX");
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            Debug.Log("SEVEN");
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            Debug.Log("EIGHT");
        }
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            Debug.Log("NINE");
        }
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            Debug.Log("TEN");
        }        
    }
}
