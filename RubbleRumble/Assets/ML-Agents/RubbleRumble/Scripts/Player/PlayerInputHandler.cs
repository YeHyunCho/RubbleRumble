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

        // ��� ��ó, �ݱ�, ������ ������ ��
        if (Input.GetKeyDown(KeyCode.E))
        {
            UseTool();
            TryThrowAway();
        }

        // �ڽ��� �۾��뿡 �ø�
        if (Input.GetKeyDown(KeyCode.Q))
        {
            TryPlaceTrashOnTheWorkbench();
        }

        // �۾��뿡�� ������ ���� �и�(��ɷ� ��ô �޼���� Mop.cs�� ��ġ)
        if (Input.GetKey(KeyCode.Q))
        {
            TryUnfoldBox();
        }

        // ��ȣ�ۿ� �ð� �ʱ�ȭ
        if (Input.GetKeyUp(KeyCode.Q))
        {
            qKeyHoldTime = 0f;
        }
    }

    /// ��������� �ڽ� Ʈ�������� �̸����� ã��.
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
            Debug.LogError("Animator ������Ʈ�� ã�� �� �����ϴ�. �ڽ� ������Ʈ�� Animator�� �ִ��� Ȯ���ϼ���.");
            this.rightHand = null; // rightHand�� null�� ��Ȯ�� �����Ͽ� ���� ����
            return;
        }

        Transform mainHandBone = animator.GetBoneTransform(HumanBodyBones.RightHand);
        if (mainHandBone == null)
        {
            Debug.LogError("HumanBodyBones.RightHand Transform�� ã�� �� �����ϴ�. Animator�� Humanoid Rig ������ Ȯ���ϼ���.");
            this.rightHand = null;
            return;
        }

        // "RightHandProp" ���� mainHandBone�� �ڽ� �߿��� Ž�� 
        Transform propBone = FindDeepChild(mainHandBone, "RightHandProp");

        if (propBone != null)
        {
            this.rightHand = propBone; // ã�Ҵٸ� RightHandProp�� ���
            Debug.Log("������ ������ ���� RightHandProp ���� ����մϴ�.");
        }
        else
        {
            this.rightHand = mainHandBone; // �� ã�Ҵٸ� ���� ��Ĵ�� �ո� ���� ���
            Debug.LogWarning("RightHandProp ���� ã�� ���߽��ϴ�. HumanBodyBones.RightHand�� ����մϴ�. ������ ��ġ�� ����� �� �ֽ��ϴ�.");
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
        // Mop3 ������ ��ġ ���� �ڵ� <- ������� �ʾƼ� �ּ�ó��
        /*tools[2].transform.localPosition += Vector3.up * 0.1f;
        tools[2].transform.localPosition += Vector3.forward * 0.1f;
        tools[2].transform.localRotation = Quaternion.Euler(90, 0, 45);
        */

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
