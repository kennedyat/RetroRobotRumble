using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class VFXPlaceHolder : MonoBehaviour
{

    private TrailRenderer _trail;
    public GameObject playerLimb;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(playerLimb!=null)
            transform.position = playerLimb.transform.position;
         
    }
}
