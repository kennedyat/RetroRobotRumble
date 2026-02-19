using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FB_Sector : MonoBehaviour
{
    [SerializeField] GameObject sModel;
    [SerializeField] GameObject sCollider;
    [SerializeField] OMEGA_2 data;
    [SerializeField] TextMeshPro TEMP_text;

    bool isSafe;

    public void Init(bool safe)
    {
        isSafe = safe;
        sModel.GetComponent<Renderer>().material.color = isSafe ? Color.green : Color.red;
        sCollider.GetComponent<Renderer>().material.color = isSafe ? Color.green : Color.red;

        TEMP_text.transform.localPosition = Vector3.up * 0.15f;
        TEMP_text.text = isSafe ? "SAFE" : "UNSAFE";

        sCollider.SetActive(false);
        StartCoroutine(EnableSequence());
    }

    IEnumerator EnableSequence()
    {
        yield return new WaitForSeconds(data.explosionDelay);
        sCollider.SetActive(true);

        yield return new WaitForSeconds(data.recoveryTime / 2);
        sCollider.SetActive(false);
        yield return null;
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
