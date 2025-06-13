using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class AgentInputHandler : CleanerBase
{
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

        // ��� ��ó, �ݱ�, ������ ������ ��
        if (e)
        {
            // Debug.Log("in e");
            UseTool();
            TryThrowAway();
        }

        // �ڽ��� �۾��뿡 �ø�
        if (q)
        {
            // Debug.Log("in q");
            TryPlaceTrashOnTheWorkbench();
        }

        // �۾��뿡�� ������ ���� �и�(��ɷ� ��ô �޼���� Mop.cs�� ��ġ)
        if (qhold)
        {
            TryUnfoldBox();
        }
    }

    protected override void SetRightHand()
    {
        // �÷��̾��� Animator���� ������ ��(Bone)�� Transform�� ������
        rightHand = GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.RightHand);
        // ������ ��ġ�� �չٴ� �������� �ణ ���� (0.15 ���� �̵�)
        if (rightHand != null) // Null üũ �߰�
        {
            rightHand.position = rightHand.position + rightHand.forward * 0.15f;
        }
        else
        {
            Debug.LogError("RightHand Transform�� ã�� �� �����ϴ�. Animator�� HumanBodyBones ������ Ȯ���ϼ���.");
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
        // Mop3 ������ ��ġ ����
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

    public bool GetQPressed() { return qPressed; }
    public bool GetEPressed() { return ePressed; }
    public bool GetQHold() { return qHold; }
    public bool GetEHold() { return eHold; }
}
