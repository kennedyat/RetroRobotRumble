using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineReticle : MonoBehaviour
{
    float scaleFactor = -1;
    float length;
    float time;
    float speed;
    float width;
    bool raycastToWall;
    [SerializeField] GameObject lrBase;
    [SerializeField] GameObject lrExpander;

    public void Init(float l, float t, float w, bool raycastToWall = false)
    {
        if (scaleFactor == -1) scaleFactor = transform.localScale.x / lrBase.transform.localScale.x;
        time = t;
        length = l;
        width = w;

        this.raycastToWall = raycastToWall;

        // PHYSICS IS USEFUL LETS GOOOOO SPEED = DISTANCE OVER TIME
        speed = length / time;

        // set the y scale of this object to half the length
        // width also needs scale
        lrBase.transform.localScale = new(width / scaleFactor, length / 2f / scaleFactor, 1f);

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

                // weird thing where its a little short so boost it a bit manually
                length = hit.distance * 1.11f;
                lrBase.transform.localScale = new(width / scaleFactor, length / 2f / scaleFactor, 1f);
            }
            
            lrExpander.transform.localScale = new(width / scaleFactor, Mathf.Lerp(0, length / 2f / scaleFactor, t / time));
            
            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        gameObject.SetActive(false);
    }
}
