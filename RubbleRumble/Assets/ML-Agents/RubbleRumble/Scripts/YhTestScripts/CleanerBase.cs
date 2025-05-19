using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CleanerBase : MonoBehaviour
{
    protected WorkBench workBench;
    protected TrashManager heldTrash;

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

        heldObject = null;
        nearObject = null;
        trashOnWorkbench = null;
    }

    // 도구 사용 기능을 담은 함수
    public void UseTool()
    {
        if (nearObject != null)
        {
            TrashManager nearTrash = nearObject.GetComponent<TrashManager>();

            // if (현재 들고 있는 도구 == 근처에 있는 쓰레기와 상호작용하는 도구)
            if (currentTool == nearTrash.trashData.interactTool)
            {
                // if (아무것도 들고있지 않은 맨손일 때)
                if (currentTool == 0 && heldObject == null)
                {
                    PickUpTrash(nearObject, rightHand);

                    heldObject = nearObject;
                    heldTrash = heldObject.GetComponent<TrashManager>();
                } 
                // if (빗자루)
                else if (currentTool == 1)
                {
                    // 코드 넣어야함.
                } 
                // if (대걸레)
                else if (currentTool == 2)
                {
                    Mop mop = FindObjectOfType<Mop>();

                    // if (대걸레의 할당량이 채워지지 않았다면)
                    if (mop.GetUseCount() < 2)
                    {
                        Obstacle dirt = nearObject.GetComponent<Obstacle>();
                        dirt.CleanObstacle();
                        nearObject = null;
                        mop.IncrementUseCount(); // useCount 증가
                        mop.UpdateMaterial(); // 재질 업데이트
                    }
                }
            } 
        }
    }

    public void TryThrowAway()
    {
        if (heldObject != null && isNearRecyclingBin)
        {
            if (heldTrash.trashData.readyToThrowAway)
            {
                ThrowTrashAway(heldObject);
                heldObject = null;
            }
        }
    }

    public void TryPlaceTrashOnTheWorkbench()
    {
        if (isNearWorkbench && heldObject != null && heldTrash.trashData.trashName == "Box")  
        {
            PlaceTrashOnWorkbench(workBench, heldObject);
            trashOnWorkbench = heldObject;
            heldObject = null;
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
        if (other.CompareTag("Can") || other.CompareTag("Box") || other.CompareTag("Dust")) // 프리팹 태그 다 Trash로 통일시켜도될듯?
        {
            nearObject = other.gameObject;
        }

        if (other.CompareTag("WorkbenchArea"))
        {
            if (!isNearWorkbench)
            {
                isNearWorkbench = true;
            }
            if (trashOnWorkbench != null) nearObject = trashOnWorkbench;
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
