using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using DG.Tweening;

public class CleanerBase : MonoBehaviour
{
    [Header("Player Tool")]
    public GameObject[] toolPrefabs;
    public Transform rightHand;

    [Header("Get Game Object")]
    public GameObject workBench;
    public GameObject sink;
    public GameObject unfoldedBox;
    public GameObject trashBag;

    protected GameObject nearObject;
    protected TrashManager heldTrash;
    protected TrashManager trashBagData;
    protected TrashInteractionManager interact;

    protected bool isHoldingTrash = false;
    protected bool isNearObject = false;
    protected bool isTrashOnTheWorkbench = false;
    protected bool isNearWorkbench = false;
    protected bool isNearRecyclingBin = false;
    protected bool isUnfolding = false;
    protected bool readyToClean = false;

    protected GameObject[] tools;

    protected GameObject heldObject;
    protected GameObject trashOnWorkbench;
    protected GameObject currentRecyclebin;

    protected int currentTool = -1;
    protected int broomUsage;

    protected const float UNFOLD_DURATION = 2f;
    protected float qKeyHoldTime = 0f;
    private float addscoreCB = 0f;

    protected void Awake()
    {
        SetRightHand();
    }

    protected virtual void Start()
    {
        currentTool = -1;
        broomUsage = 0;

        SetToolLocation();

        //workBench = FindFirstObjectByType<WorkBench>();
        interact = GameObject.Find("Managers").GetComponent<TrashInteractionManager>();
        trashBagData = trashBag.GetComponent<TrashManager>();

        heldObject = null;
        nearObject = null;
        trashOnWorkbench = null;
    }

    protected void UseTool()
    {
        if (isNearObject)
        {
            if (nearObject == null) { return; }
            TrashManager nearTrash = nearObject.GetComponent<TrashManager>();
            if (nearTrash == null) { return; }

            if (currentTool == nearTrash.trashData.interactTool)
            {
                readyToClean = false;

                if (currentTool == 0 && !isHoldingTrash)
                {
                    interact.PickUpTrash(nearObject, rightHand, gameObject);

                    heldObject = nearObject;
                    nearObject = null;
                    heldTrash = heldObject.GetComponent<TrashManager>();

                    if (isNearWorkbench)    // 작업대 근처에서 쓰레기를 주운 경우
                    {
                        // 들고 있는 쓰레기가 박스라면
                        if (heldObject.CompareTag("Box") || heldObject.CompareTag("UnfoldedBox"))
                            // 작업대 위에 박스가 없는 상태로 설정
                            isTrashOnTheWorkbench = false;
                        trashOnWorkbench = null;
                    }

                    isNearObject = false;
                    isHoldingTrash = true;
                }
                else if (currentTool == 1)
                {
                    if (!trashBagData.trashData.readyToThrowAway)
                    {
                        broomUsage += 1;
                        bool readyToThrowAwayTrashBag = interact.ThrowDustInTrashBag(nearObject, trashBag, broomUsage);

                        if (readyToThrowAwayTrashBag)
                        {
                            broomUsage = 0;
                            trashBagData.trashData.readyToThrowAway = true;
                        }

                        isNearObject = false;
                        nearObject = null;
                    }

                }
                else if (currentTool == 2)
                {
                    Mop mop = FindObjectOfType<Mop>();

                    if (mop.GetUseCount() < 2)
                    {
                        interact.CleanWaterSpot(mop, nearObject);

                        nearObject = null;
                        isNearObject = false;
                    }
                }
            }

            readyToClean = false;
        }
    }

    protected void TryThrowAway()
    {
        if (isHoldingTrash && isNearRecyclingBin)
        {
            if (heldTrash.trashData.readyToThrowAway && currentRecyclebin.CompareTag(heldTrash.trashData.interactTrashbin))
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

                addscoreCB += 2f;
            }
        }
    }

    protected void TryUnfoldBox()
    {
        if (isNearWorkbench && isTrashOnTheWorkbench)
        {
            qKeyHoldTime += Time.deltaTime;
            isUnfolding = true;
            Debug.Log("TryUnfoldBox");
            if (qKeyHoldTime >= UNFOLD_DURATION)
            {
                TrashManager box = trashOnWorkbench.GetComponent<TrashManager>();

                if (!box.trashData.readyToThrowAway)
                {
                    qKeyHoldTime = 0f;
                    trashOnWorkbench = interact.UnfoldBox(trashOnWorkbench);
                    isUnfolding = false;
                }
                addscoreCB += 5f;
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
        if (other.CompareTag("Can") || other.CompareTag("Box") || other.CompareTag("Dust") || other.CompareTag("UnfoldedBox") || other.CompareTag("Water")) // 프리팹 태그 다 Trash로 통일시켜도될듯?
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

        if (other.CompareTag("TBdust") || other.CompareTag("TBpaper") || other.CompareTag("TBcan"))
        {
            if (!isNearRecyclingBin)
            {
                isNearRecyclingBin = true;
                currentRecyclebin = other.gameObject;
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

        if (other.CompareTag("TBdust") || other.CompareTag("TBpaper") || other.CompareTag("TBcan"))
        {
            isNearRecyclingBin = false;
            currentRecyclebin = null;
        }
    }

    protected virtual void SetRightHand() { }

    protected virtual void SetToolLocation() { }

    public bool GetisHoldingTrash() { return isHoldingTrash; }
    public int GetHoldingTrashName()
    {
        // No object held
        if (heldObject == null || heldTrash == null || heldTrash.trashData == null)
        {
            return 0;
        }
        // Determine trash type by name
        string name = heldTrash.trashData.trashName;
        switch (name)
        {
            case "Can":
                return 1;
            case "Box":
                return 2;
            case "UnfoldedBox":
                return 3;
            default:
                return 0;
        }
    }

    public int GetMopUseCount()
    {
        Mop mop = FindObjectOfType<Mop>();
        if (mop == null) { return 0; }
        if (mop.GetUseCount() < 2) { return 0; }
        if (mop.GetUseCount() == 2) { return 1; }

        return 0;
    }

    public bool GetisNearWorkbench() { return isNearWorkbench; }
    public bool GetisNearRecyclingBin() { return isNearRecyclingBin; }

    public float GetCBAddScore() { return addscoreCB; }
    public void Clear_CBAddscore() { addscoreCB = 0f; }
}
