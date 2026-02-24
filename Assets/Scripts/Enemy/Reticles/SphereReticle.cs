using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereReticle : MonoBehaviour
{
    float snapshotRadius;
    float time;
    [SerializeField] GameObject srBase;
    [SerializeField] GameObject srExpander;
    public void Init(float t, float r)
    {
        time = t;

        snapshotRadius = srBase.transform.localScale.x;
        transform.localScale *= r * 2;

        StartCoroutine(ExpandSequence());
    }

    IEnumerator ExpandSequence()
    {
        float t = 0;
        while (t < time)
        {
            srExpander.transform.localScale = Vector3.one * Mathf.Lerp(0, snapshotRadius, t / time);

            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        Destroy(gameObject);
    }
}
