using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using DG.Tweening;
using DecalSystem;

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
    protected Stack<GameObject> trashOnWorkbench;   // 작업대 위에 올라가 있는 상자
    protected GameObject currentRecyclebin;

    protected int currentTool = -1;
    protected int broomUsage;

    protected const float UNFOLD_DURATION = 2f;
    protected float qKeyHoldTime = 0f;

    protected void Awake()
    {
        SetRightHand();
        trashOnWorkbench = new Stack<GameObject>();
;    }

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
        //trashOnWorkbench = null;
    }

    protected void UseTool()
    {
        if (isNearObject)
        {
            TrashManager nearTrash = nearObject.GetComponent<TrashManager>();

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
                        {
                            if (trashOnWorkbench.Count > 0) // 작업대 위에 박스가 있으면
                                trashOnWorkbench.Pop(); // 작업대 위 최상단 박스 가져오기
                            if (trashOnWorkbench.Count <= 0)    // 작업대 위에 박스가 없으면
                            {
                                isTrashOnTheWorkbench = false;  // 작업대 비어있는 상태로 만들기
                            }
                        }
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
                trashOnWorkbench.Push(heldObject);
                Debug.Log("Box Pused");

                isTrashOnTheWorkbench = true;
                isHoldingTrash = false;
                heldObject = null;
            }
        }
    }

    protected void TryUnfoldBox()
    {
        if (isNearWorkbench && isTrashOnTheWorkbench)
        {
            qKeyHoldTime += Time.deltaTime;
            isUnfolding = true;

            if (qKeyHoldTime >= UNFOLD_DURATION)
            {
                TrashManager box = trashOnWorkbench.Peek().GetComponent<TrashManager>();

                if (!box.trashData.readyToThrowAway)
                {
                    qKeyHoldTime = 0f;
                    // 작업대 위에 접힌 상자를 없애고 펼친 상자 놓기
                    trashOnWorkbench.Push(interact.UnfoldBox(trashOnWorkbench.Pop()));
                    isUnfolding = false;
                }
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
                nearObject = trashOnWorkbench.Peek();
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
        if (heldTrash == null) { return 0; }
        else if (heldTrash.trashData.trashName == "Can") { return 1; }
        else if (heldObject.CompareTag("Box")) { return 2; }
        else if (heldObject.CompareTag("UnfoldedBox")) { return 3; }
        return 0;
    }

    public int GetMopUseCount()
    {
        Mop mop = FindObjectOfType<Mop>();
        if (mop == null) { return 0; }
        if (mop.GetUseCount() < 2) { return 0; }
        if (mop.GetUseCount() == 2) { return 1; }

        return 0;
    }
}
