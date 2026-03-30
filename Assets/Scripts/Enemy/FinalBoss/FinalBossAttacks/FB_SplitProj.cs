using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FB_SplitProj : FB_Proj
{
    public enum SplitPattern { Cross = 0, X }
    SplitPattern pattern;
    [SerializeField] GameObject FB_origProj;

    Vector3 startPos;
    bool isSplit = false;
    Transform playerPos;
    float splitDistance, splitProjScale, splitProjLifetime, splitProjSpeed;
    int splitCount, splitProjDamage;

    public void Init(Vector3 dir, float spd, int dmg, float splitDist, int splitC, float splitLife,
        float splitProjSc, int splitDmg, float splitSpd, int pl, int ll, SplitPattern p, Transform player)
    {
        startPos = transform.position;
        direction = dir;
        playerLayer = pl;
        levelLayer = ll;
        pattern = p;
        playerPos = player;

        speed = spd;
        damage = dmg;

        splitDistance = splitDist;
        splitCount = splitC;
        splitProjScale = splitProjSc;
        splitProjLifetime = splitLife;
        splitProjDamage = splitDmg;
        splitProjSpeed = splitSpd;

        // DO NOT destroy this projectile, it destroys itself when it splits
    }

    protected new void Update()
    {
        transform.position += speed * Time.deltaTime * direction;
        float distance = Vector3.Distance(startPos, transform.position);

        if (!isSplit && distance >= splitDistance)
        {
            isSplit = true;

            Split();
        }
    }

    void Split()
    {
        // split 90 deg
        Vector3 toPlayer = playerPos.position - transform.position;
        toPlayer.y = 0;
        float angleDeg = Mathf.Atan2(toPlayer.x, toPlayer.z) * Mathf.Rad2Deg;
        float splitRotation = 360f / splitCount;

        if (pattern == SplitPattern.X)
        {
            // offset for X pattern
            angleDeg += 45;
        }

        // spawn 4 regular bullets here
        for (int i = 0; i < splitCount; i++)
        {
            GameObject reference = Instantiate(FB_origProj, transform.position, Quaternion.identity);

            // rotate this proj accordingly
            reference.transform.rotation = Quaternion.AngleAxis(angleDeg + i * splitRotation, Vector3.up);
            reference.GetComponent<FB_Proj>().Init(reference.transform.forward, splitProjSpeed, splitProjLifetime, splitProjDamage, playerLayer, levelLayer);

            reference.transform.localScale = Vector3.one * splitProjScale;
        }

        Destroy(gameObject);
    }

    // inherits the collision and update functions accordingly
}
