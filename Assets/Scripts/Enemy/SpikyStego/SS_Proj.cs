using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SS_Proj : MonoBehaviour
{
    [SerializeField] GameObject hazardPrefab;
    GameObject thisHazard;
    Vector3 startPos, targetPos;
    int playerLayer;
    float duration, height;
    int damage;

    // hazard stats
    int hDamage;
    float hCooldown, hXScale, hZScale, hLife;
    public void Init(int damage, float height, float duration, float projScale, int pl, Vector3 player,
        int d, float cooldown, float xScale, float zScale, float lifetime)
    {
        startPos = transform.position;
        this.damage = damage;
        this.height = height;
        this.duration = duration;

        transform.localScale = Vector3.one * projScale;

        playerLayer = pl;
        targetPos = player;

        // in case we spawn a hazard, get the stats
        hDamage = d;
        hCooldown = cooldown;
        hXScale = xScale;
        hZScale = zScale;
        hLife = lifetime;

        StartCoroutine(BombSequence());
    }

    IEnumerator BombSequence()
    {
        // follow a parabolic trajectory
        float t = 0;
        while (t < 1f)
        {
            // horizontal movement
            Vector3 lerpPos = Vector3.Lerp(startPos, targetPos, t);

            // vertical arc
            float arc = 4 * height * t * (1 - t);
            lerpPos.y += arc;

            transform.position = lerpPos;

            t += Time.deltaTime / duration;
            yield return null;
        }
        transform.position = targetPos;

        // if we make it here, spawn a hazard
        thisHazard = Instantiate(hazardPrefab);
        thisHazard.GetComponent<SS_Hazard>().Init(hDamage, hCooldown, playerLayer, hXScale, hZScale, hLife);
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == playerLayer)
        {
            other.GetComponent<PlayerHealth>().TakeDamage(damage);
        }
    }
}
