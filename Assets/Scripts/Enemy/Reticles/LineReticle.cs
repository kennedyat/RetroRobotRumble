using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineReticle : MonoBehaviour
{
    float scaleFactor;
    float snapshotScale = .138f;
    float length, currentLength;
    float time;
    float width;
    bool raycastToWall;
    [SerializeField] GameObject lrBase;
    [SerializeField] GameObject lrExpander;

    public void Init(float l, float t, float w, bool raycastToWall = false)
    {
        snapshotScale = lrBase.transform.localScale.x;

        time = t;
        if (l < 0)
            l = 1000;
        length = currentLength = l;
        width = w;

        this.raycastToWall = raycastToWall;

        // set the scale according to the scale of the parent
        // lossy scale gives world scale, so get that
        scaleFactor = transform.lossyScale.x;
        transform.localScale = new Vector3(width / scaleFactor, 1f, length / scaleFactor);

        // set the local position
        transform.localPosition = new(0, transform.parent.position.y / -2f, 0);

        // call the coroutine which makes the line go
        StartCoroutine(ExpandSequence());
    }

    IEnumerator ExpandSequence()
    {
        float t = 0;
        while (t < time)
        {
            if (raycastToWall)
            {
                // resize this depending on where the next wall in sight is
                LayerMask mask = LayerMask.GetMask("Level");
                Vector3 adjustedSpawnPos = transform.position + Vector3.up * 0.5f;
                Physics.Raycast(adjustedSpawnPos, transform.forward,
                    out RaycastHit hit, 1000, mask, QueryTriggerInteraction.Collide);

                //Debug.DrawRay(adjustedSpawnPos, hit.point - adjustedSpawnPos, Color.red);
                //Debug.DrawRay(transform.position, hit.point - transform.position, Color.green);

                // update the length and the scale if needed
                currentLength = length > hit.distance ? hit.distance : length;
                transform.localScale = new Vector3(width / scaleFactor, 1f, currentLength / scaleFactor);
            }

            lrExpander.transform.localScale = new Vector3(snapshotScale, Mathf.Lerp(0, snapshotScale, t / time), 1f);

            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        Destroy(gameObject);
    }
}
