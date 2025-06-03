using System.Collections;
using System.Collections.Generic;
using Unity.Sentis.Layers;
using UnityEngine;

public class TrashInteractionManager : MonoBehaviour
{
    [SerializeField] private GameObject unfoldedBox;

    public void PickUpTrash(GameObject trash, Transform rightHand, GameObject player)
    {
        trash.transform.SetParent(rightHand); // 쓰레기를 전달받은 rightHand의 자식으로 설정

        // 쓰레기 오브젝트에서 HoldableItem 컴포넌트를 가져옴
        HoldableItem holdableInfo = trash.GetComponent<HoldableItem>();

        if (holdableInfo != null)
        {
            // HoldableItem 컴포넌트가 있다면, 거기에 설정된 오프셋 값들을 적용
            trash.transform.localPosition = holdableInfo.holdPositionOffset;
            trash.transform.localRotation = Quaternion.Euler(holdableInfo.holdEulerRotationOffset);
            // 만약 HoldableItem 스크립트에서 Quaternion을 직접 사용한다면:
            // trash.transform.localRotation = holdableInfo.holdRotationQuaternionOffset;
        }
        else
        {
            // HoldableItem 컴포넌트가 없다면, 기존처럼 기본 위치/회전 값 사용
            // (RightHandProp의 원점에 위치)
            trash.transform.localPosition = Vector3.zero;
            trash.transform.localRotation = Quaternion.identity;
            // 경고 메시지를 출력하여 HoldableItem 추가를 권장할 수 있습니다.
            Debug.LogWarning($"쓰레기 아이템 '{trash.name}'에 HoldableItem 컴포넌트가 없습니다. 기본 홀드 변환을 사용합니다. 더 나은 시각적 표현을 위해 HoldableItem 추가를 고려하세요.");
        }

        Rigidbody trashRb = trash.GetComponent<Rigidbody>();
        if (trashRb != null)
        {
            trashRb.isKinematic = true; // 물리 효과 비활성화 (손에 고정)
            trashRb.velocity = Vector3.zero;
            trashRb.angularVelocity = Vector3.zero;
        }

        Collider trashCollider = trash.GetComponent<Collider>();
        Collider playerCollider = player.GetComponent<Collider>(); // 'player'는 플레이어 캐릭터 GameObject로 가정

        if (trashCollider != null && playerCollider != null)
        {
            Physics.IgnoreCollision(trashCollider, playerCollider, true); // 들고 있는 동안 플레이어와 충돌 무시
            trashCollider.enabled = false; // 선택 사항: 들고 있는 동안 콜라이더를 비활성화하여 다른 물리 문제 방지
        }
    }


    //public void PlaceTrashOnWorkbench(WorkBench workbench, GameObject trash, GameObject player)
    public void PlaceTrashOnWorkbench(GameObject workbench, GameObject trash, GameObject player)
    {
        Vector3 workbenchTop = workbench.transform.position;
        trash.transform.SetParent(null);
        trash.transform.position = workbenchTop;
        trash.transform.rotation = Quaternion.identity;

        Rigidbody objRb = trash.GetComponent<Rigidbody>();
        if (objRb != null)
        {
            objRb.isKinematic = false;
            objRb.velocity = Vector3.zero;
        }
        Collider objCollider = trash.GetComponent<Collider>();
        Collider playerCollider = player.GetComponent<Collider>();
        if (objCollider != null && playerCollider != null)
        {
            Physics.IgnoreCollision(objCollider, playerCollider, false);
            objCollider.enabled = true;
        }
    }

    public void ThrowTrashAway(GameObject trash)
    {
        //Destroy(trash);
        // 풀로 반환하며 점수 획득
        Obstacle obstacle = trash.GetComponent<Obstacle>();
        if (obstacle != null)
        {
            obstacle.CleanObstacle();
        }
    }

    public void CleanWaterSpot(Mop mop, GameObject nearObject)
    {
        Obstacle WaterSpot = nearObject.GetComponent<Obstacle>();
        WaterSpot.CleanObstacle();
        mop.IncrementUseCount(); // useCount 증가
        mop.UpdateMaterial(); // 재질 업데이트
    }

    public bool ThrowDustInTrashBag(GameObject trash, GameObject trashBag, int broomUsage)
    {
        bool readyToThrowTrashBag = false;

        Vector3 trashBagScale = trashBag.transform.localScale;
        trashBag.transform.localScale = new Vector3(trashBagScale.x, trashBagScale.y + 1, trashBagScale.z);

        Obstacle obstacle = trash.GetComponent<Obstacle>();
        if (obstacle != null)
        {
            if (broomUsage < 3)
            {
                obstacle.RemoveObstacle();
                readyToThrowTrashBag = false;
            }
            else
            {
                obstacle.CleanObstacle();
                readyToThrowTrashBag = true;
            }
        }

        return readyToThrowTrashBag;
    }

    public GameObject UnfoldBox(GameObject trashOnWorkbench)
    {
        //GameObject oldBox = trashOnWorkbench;

        //trashOnWorkbench = Instantiate(unfoldedBox, oldBox.transform.position, oldBox.transform.rotation);
        //Destroy(oldBox);

        //return trashOnWorkbench;

        Transform boxPos = trashOnWorkbench.transform;  // 박스 위치 설정
        bool isPlayer = false;  // 박스가 플레이어 소유인지 확인하는 플래그
        Obstacle foldedBox = trashOnWorkbench.GetComponent<Obstacle>();
        if (foldedBox != null)
        {
            isPlayer = foldedBox.IsPlayer;  // 소유권 설정
            foldedBox.RemoveObstacle(); // 접힌 박스를 풀로 반환, 점수 부여 X
        }

        Obstacle unfoldedBox = PoolManager.Instance.SpawnFromPool<Obstacle>("UnfoldedBox"); // 풀에서 펼쳐진 박스 가져오기
        unfoldedBox.transform.position = boxPos.position;
        unfoldedBox.IsPlayer = isPlayer;    // 소유권 설정
        
        return unfoldedBox.gameObject;  // 펼쳐진 박스 오브젝트를 반환
    }
}
