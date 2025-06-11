// HoldableItem.cs
using UnityEngine;

public class HoldableItem : MonoBehaviour
{
    [Tooltip("Offset from the parent hand bone (e.g., RightHandProp) to position this item correctly when held.")] // 영어 툴팁
    public Vector3 holdPositionOffset = Vector3.zero; // 손에 쥐었을 때 적용될 로컬 위치 오프셋

    [Tooltip("Local Euler rotation to apply to this item when held, relative to the hand bone. Easier for Inspector editing.")] // 영어 툴팁
    public Vector3 holdEulerRotationOffset = Vector3.zero; // 손에 쥐었을 때 적용될 로컬 회전 오프셋 (오일러 각도)

}