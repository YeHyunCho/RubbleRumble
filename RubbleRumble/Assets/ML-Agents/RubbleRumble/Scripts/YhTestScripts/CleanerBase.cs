using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CleanerBase : MonoBehaviour
{
    protected WorkBench workBench;

    public GameObject nearObject;
    public GameObject unfoldedBox;
    public Transform rightHand;

    public bool isHoldingTrash = false;
    public bool isNearWorkbench = false;
    public bool isNearRecyclingBin = false;

    public GameObject[] toolPrefabs;
    protected GameObject[] tools;

    protected GameObject heldObject;
    protected GameObject trashOnWorkbench;

    public int currentTool = -1;

    protected const float UNFOLD_DURATION = 2f;
    protected float qKeyHoldTime = 0f;

    protected void Awake()
    {
        SetRightHand();
    }

    protected void Start()
    {
        SetToolLocation();

        workBench = FindFirstObjectByType<WorkBench>();
    }

    public void TryPickUp()
    {
        if (currentTool == 0 && !isHoldingTrash)
        {
            if (isNearWorkbench && trashOnWorkbench != null)
            {
                nearObject = trashOnWorkbench;
                trashOnWorkbench = null;
            }

            if (nearObject != null)
            {
                PickUpTrash(nearObject, rightHand);
                heldObject = nearObject;
                isHoldingTrash = true;
            }
        }
    }

    public void TryThrowAway()
    {
        if (isHoldingTrash && isNearRecyclingBin)
        {
            TrashManager trash = heldObject.GetComponent<TrashManager>();

            if (trash.trashData.readyToThrowAway)
            {
                ThrowTrashAway(heldObject);
                heldObject = null;
                isHoldingTrash = false;
            }
        }
    }

    public void TryPlaceTrashOnTheWorkbench()
    {
        if (isHoldingTrash && heldObject.CompareTag("Box"))
        {
            PlaceTrashOnWorkbench(workBench, heldObject);
            trashOnWorkbench = heldObject;
            heldObject = null;
            isHoldingTrash = false;
        }
    }

    public void TryUnfoldBox()
    {
        qKeyHoldTime += Time.deltaTime;

        if (isNearWorkbench && trashOnWorkbench != null && qKeyHoldTime >= UNFOLD_DURATION)
        {
            TrashManager box = trashOnWorkbench.GetComponent<TrashManager>();

            if (!box.trashData.readyToThrowAway)
            {
                qKeyHoldTime = 0f;

                GameObject oldBox = trashOnWorkbench;

                trashOnWorkbench = Instantiate(unfoldedBox, oldBox.transform.position, oldBox.transform.rotation);
                Destroy(oldBox);
            }
        }
    }

    public void PickUpTrash(GameObject trash, Transform rightHand)
    {
        trash.transform.SetParent(rightHand);
        trash.transform.localPosition = Vector3.zero; 
        trash.transform.localRotation = Quaternion.identity; 

        Rigidbody trashRb = trash.GetComponent<Rigidbody>();
        if (trashRb != null)
        {
            trashRb.isKinematic = true; 
            trashRb.velocity = Vector3.zero; 
            trashRb.angularVelocity = Vector3.zero; 
        }

        Collider trashCollider = trash.GetComponent<Collider>();
        Collider playerCollider = GetComponent<Collider>();

        if (trashCollider != null && playerCollider != null)
        {
            Physics.IgnoreCollision(trashCollider, playerCollider, true); 
            trashCollider.enabled = false;
        }
    }

    public void PlaceTrashOnWorkbench(WorkBench workbench, GameObject trash)
    {
        Vector3 workbenchTop = workbench.transform.position;
        trash.transform.SetParent(null); 
        trash.transform.position = workbenchTop; 
        trash.transform.rotation = Quaternion.identity; 

        Rigidbody objRb = trash.GetComponent<Rigidbody>();
        if (objRb != null)
        {
            objRb.isKinematic = false; 
            objRb.velocity = Vector3.zero; 
        }
        Collider objCollider = trash.GetComponent<Collider>();
        Collider playerCollider = GetComponent<Collider>();
        if (objCollider != null && playerCollider != null)
        {
            Physics.IgnoreCollision(objCollider, playerCollider, false); 
            objCollider.enabled = true;
        }
    }
    
    public void ThrowTrashAway(GameObject trash)
    {
        Destroy(trash);
    }

    public void EquipTool(int index)
    {
        if (currentTool != -1) tools[currentTool].SetActive(false);

        tools[index].SetActive(true);
        currentTool = index;
    }

    protected void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Can") || other.CompareTag("Box"))
        {
            nearObject = other.gameObject;
        }

        if (other.CompareTag("WorkbenchArea"))
        {
            if (!isNearWorkbench)
            {
                isNearWorkbench = true;
            }
        }

        if (other.CompareTag("RecyclingBin"))
        {
            if (!isNearRecyclingBin)
            {
                isNearRecyclingBin = true;
            }
        }
    }

    protected void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("WorkbenchArea"))
        {
            isNearWorkbench = false;
            qKeyHoldTime = 0f;
        }

        if (other.CompareTag("RecyclingBin"))
        {
            isNearRecyclingBin = false;
        }
    }

    protected virtual void SetRightHand() { }

    protected virtual void SetToolLocation() { }
}
