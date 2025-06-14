using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class AgentInputHandler : CleanerBase
{
    private float addscore = 0f;
    private bool qPressed, ePressed, qHold, eHold;
    public void HandleInput(int key, bool q, bool e, bool qhold, bool ehold)
    {
        qPressed = q;
        ePressed = e;
        qHold = qhold;
        eHold = ehold;

        if (key == 0) EquipTool(0);
        else if (key == 1) EquipTool(1);
        else if (key == 2) EquipTool(2);

        // 얼룩 근처, 줍기, 쓰레기 버리기 등
        if (e)
        {
            // Debug.Log("in e");
            UseTool();
            TryThrowAway();
            addscore = 3f;
        }

        // 박스를 작업대에 올림
        if (q)
        {
            // Debug.Log("in q");
            TryPlaceTrashOnTheWorkbench();
            addscore = 1f;
        }

        // 작업대에서 누르면 상자 분리(대걸레 세척 메서드는 Mop.cs에 위치)
        if (qhold)
        {
            TryUnfoldBox();
            addscore = 5f;
        }
    }

    protected override void SetRightHand()
    {
        // 플레이어의 Animator에서 오른손 뼈(Bone)의 Transform을 가져옴
        rightHand = GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.RightHand);
        // 오른손 위치를 손바닥 방향으로 약간 조정 (0.15 유닛 이동)
        if (rightHand != null) // Null 체크 추가
        {
            rightHand.position = rightHand.position + rightHand.forward * 0.15f;
        }
        else
        {
            Debug.LogError("RightHand Transform을 찾을 수 없습니다. Animator와 HumanBodyBones 설정을 확인하세요.");
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
                tools[i].transform.localRotation = Quaternion.Euler(30, 20, -60);
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
    public GameObject GetTrashOnWorkbench()
    {
        // 작업대 위에 상자가 있으면 최상단에 있는 상자를 반환
        if (trashOnWorkbench.Count > 0) return trashOnWorkbench.Peek();
        else return null;
    }
    public bool GetIsHoldingTrash() { return isHoldingTrash; }
    public bool GetIsNearWorkbench() { return isNearWorkbench; }
    public bool GetIsNearRecyclingBin() { return isNearRecyclingBin; }
    public bool GetIsUnfolding() { return isUnfolding; }
    public int GetCurrentTool() { return currentTool; }
    public bool GetReadyToClean() { return readyToClean; }

    public bool GetQPressed() { return qPressed; }
    public bool GetEPressed() { return ePressed; }
    public bool GetQHold() { return qHold; }
    public bool GetEHold() { return eHold; }
    public float GetAddScore() { return addscore; }
    public void Clear_Addscore() { addscore = 0f; }
}
