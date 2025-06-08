using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
<<<<<<< HEAD

public class CleanerBase : MonoBehaviour
{
    //protected WorkBench workBench;
    public GameObject workBench;
    public GameObject sink;
    protected TrashManager heldTrash;
    protected TrashInteractionManager interact;

    protected GameObject nearObject;
    public GameObject unfoldedBox;
    public Transform rightHand;
=======
using DG.Tweening;

public class CleanerBase : MonoBehaviour
{
    [Header ("Player Tool")]
    public GameObject[] toolPrefabs;
    public Transform rightHand;

    [Header ("Get Game Object")]
    public GameObject workBench;
    public GameObject sink;
    public GameObject unfoldedBox;
    public GameObject trashBag;

    protected GameObject nearObject;
    protected TrashManager heldTrash;
    protected TrashManager trashBagData;
    protected TrashInteractionManager interact;
>>>>>>> seunghee

    protected bool isHoldingTrash = false;
    protected bool isNearObject = false;
    protected bool isTrashOnTheWorkbench = false;
    protected bool isNearWorkbench = false;
    protected bool isNearRecyclingBin = false;
    protected bool isUnfolding = false;
    protected bool readyToClean = false;

<<<<<<< HEAD
    public GameObject[] toolPrefabs;
=======
>>>>>>> seunghee
    protected GameObject[] tools;

    protected GameObject heldObject;
    protected GameObject trashOnWorkbench;
    protected GameObject currentRecyclebin;

    protected int currentTool = -1;
<<<<<<< HEAD
=======
    protected int broomUsage;
>>>>>>> seunghee

    protected const float UNFOLD_DURATION = 2f;
    protected float qKeyHoldTime = 0f;

    protected void Awake()
    {
        SetRightHand();
    }

    protected virtual void Start()
    {
        currentTool = -1;
<<<<<<< HEAD
=======
        broomUsage = 0;
>>>>>>> seunghee

        SetToolLocation();

        //workBench = FindFirstObjectByType<WorkBench>();
        interact = GameObject.Find("Managers").GetComponent<TrashInteractionManager>();
<<<<<<< HEAD
=======
        trashBagData = trashBag.GetComponent<TrashManager>();
>>>>>>> seunghee

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
                readyToClean = false;

                if (currentTool == 0 && !isHoldingTrash)
                {
                    interact.PickUpTrash(nearObject, rightHand, gameObject);

                    heldObject = nearObject;
<<<<<<< HEAD
=======
                    nearObject = null;
>>>>>>> seunghee
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
<<<<<<< HEAD
                    // 코드 넣어야함.
                } 
=======
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
>>>>>>> seunghee
                else if (currentTool == 2)
                {
                    Mop mop = FindObjectOfType<Mop>();

                    if (mop.GetUseCount() < 2)
                    {
<<<<<<< HEAD
                        interact.CleanDirt(mop, nearObject);
=======
                        interact.CleanWaterSpot(mop, nearObject);
>>>>>>> seunghee

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
<<<<<<< HEAD
            if (heldTrash.trashData.readyToThrowAway && currentRecyclebin.CompareTag(heldTrash.trashData.trashbin))
=======
            if (heldTrash.trashData.readyToThrowAway && currentRecyclebin.CompareTag(heldTrash.trashData.interactTrashbin))
>>>>>>> seunghee
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
        if (isNearWorkbench && isTrashOnTheWorkbench)
        {
            qKeyHoldTime += Time.deltaTime;
            isUnfolding = true;

            if (qKeyHoldTime >= UNFOLD_DURATION)
            {
                TrashManager box = trashOnWorkbench.GetComponent<TrashManager>();

                if (!box.trashData.readyToThrowAway)
                {
                    qKeyHoldTime = 0f;
                    trashOnWorkbench = interact.UnfoldBox(trashOnWorkbench);
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
<<<<<<< HEAD
        if (other.CompareTag("Can") || other.CompareTag("Box") || other.CompareTag("Dust") || other.CompareTag("UnfoldedBox")) // 프리팹 태그 다 Trash로 통일시켜도될듯?
=======
        if (other.CompareTag("Can") || other.CompareTag("Box") || other.CompareTag("Dust") || other.CompareTag("UnfoldedBox") || other.CompareTag("Water")) // 프리팹 태그 다 Trash로 통일시켜도될듯?
>>>>>>> seunghee
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
}
