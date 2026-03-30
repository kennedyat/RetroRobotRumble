using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ER_BombProj : MonoBehaviour
{
    Vector3 startPos, playerPos;
    Vector3 spinAxis = Vector3.right;
    int playerLayer;
    float duration, height, spinSpeed;
    float explosionScale;
    int damage;
    public void Init(int damage, float height, float duration, float spin,
        float projScale, float explScale, int pl, Vector3 player)
    {
        startPos = transform.position;
        this.damage = damage;
        this.height = height;
        this.duration = duration;
        spinSpeed = spin;

        explosionScale = explScale * 2;
        transform.localScale = Vector3.one * projScale;

        playerLayer = pl;

        playerPos = player;

        StartCoroutine(BombSequence());
    }

    IEnumerator BombSequence()
    {
        // follow a parabolic trajectory
        float t = 0;
        while (t < 1f)
        {
            // horizontal movement
            Vector3 lerpPos = Vector3.Lerp(startPos, playerPos, t);

            // vertical arc
            float arc = 4 * height * t * (1 - t);
            lerpPos.y += arc;

            transform.position = lerpPos;

            // spin for fun
            transform.Rotate(spinAxis, spinSpeed * Time.deltaTime, Space.Self);

            t += Time.deltaTime / duration;
            yield return new WaitForEndOfFrame();
        }
        transform.position = playerPos;

        // make the bomb really big for a small amount of time
        transform.localScale = Vector3.one * explosionScale;
        Destroy(gameObject, 0.5f);
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == playerLayer)
        {
            other.GetComponent<PlayerHealth>().TakeDamage(damage);
        }
    }
}
