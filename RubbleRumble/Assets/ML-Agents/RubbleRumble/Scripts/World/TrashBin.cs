using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TrashBin : MonoBehaviour
{
    private Transform lid;

    private void Start()
    {
        lid = transform.Find("TrashbinLid");
    }

    public Tween OpenLid()
    {
        return lid.DOLocalRotateQuaternion(Quaternion.Euler(-80f, 0, 0), 1f);
    }

    public Tween CloseLid()
    {
        return lid.DOLocalRotateQuaternion(Quaternion.Euler(0f, 0, 0), 1f);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Agent"))
        {
            OpenLid();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Agent"))
        {
            CloseLid();
        }
    }
}
