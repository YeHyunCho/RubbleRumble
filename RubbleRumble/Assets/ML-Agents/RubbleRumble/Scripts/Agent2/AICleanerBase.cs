// =============================
// AICleanerBase.cs (updated)
// =============================
using UnityEngine;

/// <summary>
/// AI 전용 CleanerBase 래퍼 + 에피소드 리셋 지원
/// </summary>
public class AICleanerBase : CleanerBase
{
    // 래퍼 메서드 ---------------------------------------------------
    public void UseToolPublic()                    { UseTool(); }
    public void TryThrowAwayPublic()               { TryThrowAway(); }
    public void TryPlaceTrashOnWorkbenchPublic()   { TryPlaceTrashOnTheWorkbench(); }
    public void TryUnfoldBoxPublic()               { TryUnfoldBox(); }

    // 상태 프로퍼티 ------------------------------------------------
    public bool IsHoldingTrash     => isHoldingTrash;
    public bool IsNearWorkbench    => isNearWorkbench;
    public bool IsTrashOnWorkbench => isTrashOnTheWorkbench;
    public int  CurrentToolIndex   => currentTool;
    public GameObject HeldTrash => heldObject;
    // ★ 추가 : 에피소드 리셋용 초기화 ------------------------------
    public void ResetHeldTrash()
    {
        // 핸들/플래그 초기화 (protected 필드 접근)
        isHoldingTrash        = false;
        heldTrash             = null;

        heldObject            = null;
        nearObject            = null;
        isNearObject          = false;

        //trashOnWorkbench      = null;
        isTrashOnTheWorkbench = false;

        isNearWorkbench       = false;
        isNearRecyclingBin    = false;
        currentRecyclebin     = null;

        isUnfolding           = false;
        readyToClean          = false;
        qKeyHoldTime          = 0f;
    }
    protected override void Start()
    {
        base.Start();
        SetToolLocation(); // ← 여기서 호출하면 네가 만든 override가 호출됨!
        if (workBench == null)
        {
            var benchObj = GameObject.Find("AIMap/Workbench");
            if (benchObj == null)
                benchObj = GameObject.Find("Workbench");  // 예비 대안
            workBench = benchObj;
            Debug.Log("[AICleanerBase] workBench 자동 할당: " + (workBench != null ? workBench.name : "없음"));
        }
    }
    public void EquipToolPublic(int index)
    {
        if (tools == null || tools.Length == 0)
            SetToolLocation();
        EquipTool(index);
    }
    public void SetToolLocationPublic()
    {
        SetToolLocation();
    }
    public int GetCurrentTool()
    {
        return currentTool;
    }
    protected override void SetToolLocation()
    {
        currentTool = -1;

        if (rightHand == null)
        {
            var animator = GetComponentInChildren<Animator>();
            if (animator != null)
                rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
        }

        tools = new GameObject[toolPrefabs.Length];

        for (int i = 0; i < toolPrefabs.Length; i++)
        {
            if (toolPrefabs[i] != null)
            {
                tools[i] = Instantiate(toolPrefabs[i], rightHand.position, rightHand.rotation, rightHand);
                tools[i].transform.localRotation = Quaternion.Euler(30, 20, -60);
                tools[i].SetActive(false);
            }
        }

        // 예외 처리: Mop 프리팹 조정 (인덱스 2가 Mop이라면)
        if (tools.Length > 2 && tools[2] != null)
        {
            tools[2].transform.localPosition += Vector3.up * 0.1f;
            tools[2].transform.localPosition += Vector3.forward * 0.1f;
            tools[2].transform.localRotation = Quaternion.Euler(90, 0, 45);
        }
    }

}
