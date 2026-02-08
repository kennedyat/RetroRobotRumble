using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FB_Sector : MonoBehaviour
{
    [SerializeField] GameObject sModel;
    [SerializeField] GameObject sCollider;
    [SerializeField] OMEGA_2 data;

    bool isSafe;

    protected void OnEnable()
    {
        sCollider.SetActive(false);

        StartCoroutine(EnableSequence());
    }

    public void Init(bool safe)
    {
        isSafe = safe;

        sModel.GetComponent<Renderer>().material.color = isSafe ? Color.green : Color.red;
    }

    IEnumerator EnableSequence()
    {
        yield return new WaitForSeconds(data.explosionDelay);
        sCollider.SetActive(true);

        yield return new WaitForSeconds(data.recoveryTime);
        sCollider.SetActive(false);
        gameObject.SetActive(false);
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player") && !isSafe)
        {
            other.GetComponent<PlayerHealth>().TakeDamage(data.damage);
        }
    }
}
