using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CleanerBase : MonoBehaviour
{
    protected WorkBench workBench;
    protected TrashManager heldTrash;
    protected TrashInteractionManager interact;

    protected GameObject nearObject;
    public GameObject unfoldedBox;
    public Transform rightHand;

    protected bool isHoldingTrash = false;
    protected bool isNearObject = false;
    protected bool isTrashOnTheWorkbench = false;
    protected bool isNearWorkbench = false;
    protected bool isNearRecyclingBin = false;

    public GameObject[] toolPrefabs;
    protected GameObject[] tools;

    protected GameObject heldObject;
    protected GameObject trashOnWorkbench;

    protected int currentTool = -1;

    protected const float UNFOLD_DURATION = 2f;
    protected float qKeyHoldTime = 0f;

    protected void Awake()
    {
        SetRightHand();
    }

    protected void Start()
    {
        currentTool = -1;

        SetToolLocation();

        workBench = FindFirstObjectByType<WorkBench>();
        interact = GameObject.Find("Managers").GetComponent<TrashInteractionManager>();

        heldObject = null;
        nearObject = null;
        trashOnWorkbench = null;
    }

    protected void UseTool()
    {
        if (isNearObject)
        {
            TrashManager nearTrash = nearObject.GetComponent<TrashManager>();

            if (currentTool == nearTrash.trashData.interactTool)
            {
                if (currentTool == 0 && !isHoldingTrash)
                {
                    interact.PickUpTrash(nearObject, rightHand, gameObject);

                    heldObject = nearObject;
                    heldTrash = heldObject.GetComponent<TrashManager>();

                    if (isNearWorkbench) isTrashOnTheWorkbench = false;
                    
                    isNearObject = false;
                    isHoldingTrash = true;
                } 
                else if (currentTool == 1)
                {
                    // 코드 넣어야함.
                } 
                else if (currentTool == 2)
                {
                    Mop mop = FindObjectOfType<Mop>();

                    if (mop.GetUseCount() < 2)
                    {
                        interact.CleanDirt(mop, nearObject);

                        nearObject = null;
                        isNearObject = false;
                    }
                }
            } 
        }
    }

    protected void TryThrowAway()
    {
        if (isHoldingTrash && isNearRecyclingBin)
        {
            if (heldTrash.trashData.readyToThrowAway)
            {
                interact.ThrowTrashAway(heldObject);
                
                heldObject = null;
                isHoldingTrash = false;
            }
        }
    }

    protected void TryPlaceTrashOnTheWorkbench()
    {
        if (isNearWorkbench && isHoldingTrash)  
        {
            if (heldTrash.trashData.trashName == "Box")
            {
                interact.PlaceTrashOnWorkbench(workBench, heldObject, gameObject);
                trashOnWorkbench = heldObject;

                isTrashOnTheWorkbench = true;
                isHoldingTrash = false;
                heldObject = null;
            }
        }
    }

    protected void TryUnfoldBox()
    {
        qKeyHoldTime += Time.deltaTime;

        if (isNearWorkbench && isTrashOnTheWorkbench && qKeyHoldTime >= UNFOLD_DURATION)
        {
            TrashManager box = trashOnWorkbench.GetComponent<TrashManager>();

            if (!box.trashData.readyToThrowAway)
            {
                qKeyHoldTime = 0f;
                trashOnWorkbench = interact.UnfoldBox(trashOnWorkbench);
            }
        }
    }

    protected void EquipTool(int index)
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
            isNearObject = true;
        }

        if (other.CompareTag("WorkbenchArea"))
        {
            if (!isNearWorkbench)
            {
                isNearWorkbench = true;
            }
            if (isTrashOnTheWorkbench)
            {
                nearObject = trashOnWorkbench;
                isNearObject = true;
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
