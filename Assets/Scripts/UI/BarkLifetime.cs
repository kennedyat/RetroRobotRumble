using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarkLifetime : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<RectTransform>().anchoredPosition = new Vector2(500, 50);
    }
}
