using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereReticle : MonoBehaviour
{
    float time;
    float speed;
    float radius;
    [SerializeField] GameObject srBase;
    [SerializeField] GameObject srExpander;
    public void Init(float t, float r)
    {
        radius = r;
        time = t;
        speed = radius / time;

        StartCoroutine(ExpandSequence());
    }

    IEnumerator ExpandSequence()
    {
        float t = 0;
        while (t < time)
        {
            srExpander.transform.localScale = Vector3.one * Mathf.Lerp(0, srBase.transform.localScale.x, t / time);

            t += Time.deltaTime;            
            yield return new WaitForEndOfFrame();
        }

        Destroy(gameObject);
    }
}
