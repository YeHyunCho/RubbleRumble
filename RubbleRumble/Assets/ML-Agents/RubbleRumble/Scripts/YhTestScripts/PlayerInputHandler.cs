using System.Collections;
using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerInputHandler : CleanerBase
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) EquipTool(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) EquipTool(1);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) EquipTool(2);

        // 얼룩 근처, 줍기, 쓰레기 버리기 등
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryPickUp();
            TryThrowAway();
        }

        // 박스를 작업대에 올림
        if (Input.GetKeyDown(KeyCode.Q))
        {
            TryPlaceTrashOnTheWorkbench();
        }

        // 작업대에서 누르면 상자 분리, 개수대에서 누르면 대걸레 세척
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

    protected override void SetRightHand()
    {
        rightHand = GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.RightHand);
        rightHand.position = rightHand.position + rightHand.forward * 0.15f;
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
}
