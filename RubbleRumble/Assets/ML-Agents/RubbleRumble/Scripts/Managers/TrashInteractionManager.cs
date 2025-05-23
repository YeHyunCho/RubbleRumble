using System.Collections;
using System.Collections.Generic;
using Unity.Sentis.Layers;
using UnityEngine;

public class TrashInteractionManager : MonoBehaviour
{
    [SerializeField] private GameObject unfoldedBox;

    public void PickUpTrash(GameObject trash, Transform rightHand, GameObject player)
    {
        trash.transform.SetParent(rightHand);
        trash.transform.localPosition = Vector3.zero;
        trash.transform.localRotation = Quaternion.identity;

        Rigidbody trashRb = trash.GetComponent<Rigidbody>();
        if (trashRb != null)
        {
            trashRb.isKinematic = true;
            trashRb.velocity = Vector3.zero;
            trashRb.angularVelocity = Vector3.zero;
        }

        Collider trashCollider = trash.GetComponent<Collider>();
        Collider playerCollider = player.GetComponent<Collider>();

        if (trashCollider != null && playerCollider != null)
        {
            Physics.IgnoreCollision(trashCollider, playerCollider, true);
            trashCollider.enabled = false;
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

    public void CleanDirt(Mop mop, GameObject nearObject)
    {
        Obstacle dirt = nearObject.GetComponent<Obstacle>();
        dirt.CleanObstacle();
        mop.IncrementUseCount(); // useCount 증가
        mop.UpdateMaterial(); // 재질 업데이트
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
