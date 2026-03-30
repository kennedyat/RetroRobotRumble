using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereReticle : MonoBehaviour
{
    float snapshotRadius;
    float time;
    bool doExpand;
    [SerializeField] GameObject srBase;
    [SerializeField] GameObject srExpander;
    public void Init(float t, float r, bool doExpand = true)
    {
        time = t;

        snapshotRadius = srBase.transform.localScale.x;
        transform.localScale *= r * 2;

        this.doExpand = doExpand;
        if (!doExpand)
        {
            srBase.GetComponent<SpriteRenderer>().color = srExpander.GetComponent<SpriteRenderer>().color;
            Destroy(srExpander);
        }
        StartCoroutine(ExpandSequence());
    }

    IEnumerator ExpandSequence()
    {
        float t = 0;
        while (t < time)
        {
            if (doExpand)
                srExpander.transform.localScale = Vector3.one * Mathf.Lerp(0, snapshotRadius, t / time);

            t += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
