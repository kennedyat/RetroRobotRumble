using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundLimiterManager : MonoBehaviour
{
    public static SoundLimiterManager Instance;
    private bool canPlaySound = true;
    public float cooldown = 0.25f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void PlayEnemyFireSound(GameObject source)
    {
        if (!canPlaySound) return;

        // Send to Wwise or any spatial audio system
        // AK.Wwise.Event.PostEvent("EnemyFire_Shot", source);

        canPlaySound = false;
        StartCoroutine(SoundCooldown());
    }

    private IEnumerator SoundCooldown()
    {
        yield return new WaitForSeconds(cooldown);
        canPlaySound = true;
    }
}