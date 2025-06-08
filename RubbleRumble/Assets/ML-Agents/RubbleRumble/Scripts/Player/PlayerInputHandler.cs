using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerInputHandler : CleanerBase
{
    protected override void Start()
    {
        base.Start();

        if (workBench == null)
            workBench = GameObject.Find("PlayerMap/Workbench");
        if (sink == null)
            sink = GameObject.Find("PlayerMap/Sink");
    }
    protected void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) EquipTool(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) EquipTool(1);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) EquipTool(2);

        // 얼룩 근처, 줍기, 쓰레기 버리기 등
        if (Input.GetKeyDown(KeyCode.E))
        {
            UseTool();
            TryThrowAway();
        }

        // 박스를 작업대에 올림
        if (Input.GetKeyDown(KeyCode.Q))
        {
            TryPlaceTrashOnTheWorkbench();
        }

        // 작업대에서 누르면 상자 분리(대걸레 세척 메서드는 Mop.cs에 위치)
        if (Input.GetKey(KeyCode.Q))
        {
            TryUnfoldBox();
        }

        // 상호작용 시간 초기화
        if (Input.GetKeyUp(KeyCode.Q))
        {
            qKeyHoldTime = 0f;
        }
    }

    /// 재귀적으로 자식 트랜스폼을 이름으로 찾기.
    private Transform FindDeepChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child;
            Transform result = FindDeepChild(child, childName);
            if (result != null)
                return result;
        }
        return null;
    }

    protected override void SetRightHand()
    {
        Animator animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator 컴포넌트를 찾을 수 없습니다. 자식 오브젝트에 Animator가 있는지 확인하세요.");
            this.rightHand = null; // rightHand를 null로 명확히 설정하여 오류 방지
            return;
        }

        Transform mainHandBone = animator.GetBoneTransform(HumanBodyBones.RightHand);
        if (mainHandBone == null)
        {
            Debug.LogError("HumanBodyBones.RightHand Transform을 찾을 수 없습니다. Animator의 Humanoid Rig 설정을 확인하세요.");
            this.rightHand = null;
            return;
        }

        // "RightHandProp" 뼈를 mainHandBone의 자식 중에서 탐색 
        Transform propBone = FindDeepChild(mainHandBone, "RightHandProp");

        if (propBone != null)
        {
            this.rightHand = propBone; // 찾았다면 RightHandProp을 사용
            Debug.Log("아이템 부착을 위해 RightHandProp 뼈를 사용합니다.");
        }
        else
        {
            this.rightHand = mainHandBone; // 못 찾았다면 기존 방식대로 손목 뼈를 사용
            Debug.LogWarning("RightHandProp 뼈를 찾지 못했습니다. HumanBodyBones.RightHand를 사용합니다. 아이템 위치가 어색할 수 있습니다.");
        }
    }

    protected override void SetToolLocation()
    {
        currentTool = -1;
        tools = new GameObject[toolPrefabs.Length];

        for (int i = 0; i < toolPrefabs.Length; i++)
        {
            if (toolPrefabs[i] != null)
            {
                tools[i] = Instantiate(toolPrefabs[i], rightHand.position, rightHand.rotation, rightHand);
                tools[i].transform.localRotation = Quaternion.Euler(60, 20, 0);
                tools[i].SetActive(false);
            }
        }
        // Mop3 프리팹 위치 조정
        tools[2].transform.localPosition += Vector3.up * 0.1f;
        tools[2].transform.localPosition += Vector3.forward * 0.1f;
        tools[2].transform.localRotation = Quaternion.Euler(90, 0, 45);
    }

    public float GetHoldingTime() { return qKeyHoldTime; }
    public float GetUnfoldDuration() { return UNFOLD_DURATION; }
    public GameObject GetHeldObject() { return heldObject; }
    public GameObject GetTrashOnWorkbench() { return trashOnWorkbench; }
    public bool GetIsHoldingTrash() { return isHoldingTrash; }
    public bool GetIsNearWorkbench() { return isNearWorkbench; }
    public bool GetIsNearRecyclingBin() { return isNearRecyclingBin; }
    public bool GetIsUnfolding() { return isUnfolding; }
    public int GetCurrentTool() { return currentTool; }
    public bool GetReadyToClean() { return readyToClean; }
    public bool GetIsNearObject() { return isNearObject; }
    public GameObject GetNearObject() { return nearObject; }
    public GameObject GetCurRecycleBin() { return currentRecyclebin; }
}
