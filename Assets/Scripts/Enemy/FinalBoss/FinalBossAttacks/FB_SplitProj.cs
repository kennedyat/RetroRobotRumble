using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FB_SplitProj : FinalBossProj
{
    public enum SplitPattern { Cross = 0, X }
    SplitPattern pattern;
    [SerializeField] GameObject FB_origProj;
    [SerializeField] Trishula_R2 data;

    Vector3 startPos;
    bool isSplit = false;
    Transform playerPos;

    public void Init(Vector3 dir, int pl, int ll, SplitPattern p, Transform player)
    {
        startPos = transform.position;
        direction = dir;
        playerLayer = pl;
        levelLayer = ll;
        pattern = p;
        playerPos = player;

        speed = data.projectileSpeed;
        damage = data.damage;
        lifetime = data.allProjLifetime;

        Destroy(gameObject, lifetime);
    }

    protected new void Update()
    {
        transform.position += speed * Time.deltaTime * direction;
        float distance = Vector3.Distance(startPos, transform.position);

        if (!isSplit && distance >= data.splitDistance)
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
        float splitRotation = 360f / data.splitCount;

        if (pattern == SplitPattern.X)
        {
            // offset for X pattern
            angleDeg += 45;
        }

        // spawn 4 regular bullets here
        for (int i = 0; i < data.splitCount; i++)
        {
            GameObject reference = Instantiate(FB_origProj, transform.position, Quaternion.identity);
        
            // rotate this proj accordingly
            reference.transform.rotation = Quaternion.AngleAxis(angleDeg + i * splitRotation, Vector3.up);
            reference.GetComponent<FinalBossProj>().Init(reference.transform.forward, data.splitProjSpeed, data.allProjLifetime, data.splitProjDamage, playerLayer, levelLayer);

            reference.transform.localScale = Vector3.one * data.splitProjScale;
        }

        Destroy(gameObject);
    }
    
    // inherits the collision and update functions accordingly
}
