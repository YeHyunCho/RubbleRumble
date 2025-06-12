using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class Mop : MonoBehaviour
{
    public GameObject player;
    public GameObject agent;
    public GameObject sink;
    //private GameObject nearDust;
    public bool isPlayer;

    public Material[] mat = new Material[3];

    //private bool isTrigger;
    private Vector3 righthandPos;
    //private Vector3 offset = new Vector3(0.4f, 0.05f, -0.55f);

    //Mop 프리팹의 오프셋 수정
    private Vector3 offset = new Vector3(0.02f, -0.1f, 0.04f);

    private int useCount;
    //public float triggerDistance = 0.1f;
    public float triggerDistance = 5f;

    private float holdTime = 0f;


    private void Awake()
    {
        righthandPos = gameObject.GetComponentInParent<Transform>().localPosition;
        //sink = GameObject.FindWithTag("Sink");
    }

    private void Start()
    {
        player = GameObject.FindWithTag("Player");
        agent = GameObject.FindWithTag("Agent");

        // mop에서 플레이어 위치가 agent 위치보다 가까우면
        // 해당 mop이 플레이어가 들고 있는 것으므로 sink를 플레이어 맵의 sink와 연결
        if (Vector3.Distance(player.transform.position, transform.position) < Vector3.Distance(agent.transform.position, transform.position))
        {
            isPlayer = true;
            sink = GameObject.Find("PlayerMap/Sink");
        }
        
        else
        {
            isPlayer = false;
            sink = GameObject.Find("AIMap/Sink");
        }
        //isTrigger = false;
        //isTrigger = true;
        useCount = 0;
    }

    private void Update()
    {

        //if (isTrigger)
        //{
        //    transform.position = player.transform.position - offset;
        //}
        transform.localPosition = righthandPos + offset;
        
        //Mop 프리팹의 각도 수정
        transform.localRotation = Quaternion.Euler(60, 20, 40);

        /*if (Input.GetKeyDown(KeyCode.E) && nearDust != null)
        {
            if (useCount < 2)
            {
                // Destroy(nearDust);
                Obstacle dirt = nearDust.GetComponent<Obstacle>();
                dirt.CleanObstacle();
                nearDust = null;
                useCount++;
                gameObject.GetComponent<MeshRenderer>().material = mat[useCount];
            } else
            {
                Debug.Log("Wash Your Mop!");
            }
        }
        */

        if (useCount >= 2)
        {
            WashMopNearSink();
        }
    }

    //private void OnCollisionEnter(Collision collision)
    /*private void OnTriggerEnter(Collider other)
    {
        //if (collision.gameObject.CompareTag("Player"))
        //{
        //    isTrigger = true;
        //    transform.rotation = Quaternion.Euler(0, 0, -22f);
        //}

        //if (collision.gameObject.CompareTag("Dust") && isTrigger)
        //{
        //    nearDust = collision.gameObject;
        //    Debug.Log("Collision Detection");
        //}
        if (other.gameObject.CompareTag("Dust") && isTrigger)
        {
            nearDust = other.gameObject;
            Debug.Log("Collision Detection");
        }
    }

    //private void OnCollisionExit(Collision collision)
    private void OnTriggerExit(Collider other)
    {
        //if (collision.gameObject.CompareTag("Dust"))
        if (other.gameObject.CompareTag("Dust"))
        {
            nearDust = null;
        }
    }
    */

    private void WashMopNearSink()
    {
        float distance = Vector3.Distance(transform.position, sink.transform.position);

        if (distance <= triggerDistance)
        {
            if (isPlayer)          // ───── 사람이 조작할 때 (기존 방식 유지)
            {
                if (Input.GetKeyDown(KeyCode.Q))
                    holdTime = 0f;

                if (Input.GetKey(KeyCode.Q))
                {
                    holdTime += Time.deltaTime;
                    if (holdTime >= 2f)
                    {
                        useCount = 0;
                        GetComponent<MeshRenderer>().material = mat[useCount];
                    }
                }

                if (Input.GetKeyUp(KeyCode.Q))
                    holdTime = 0f;
            }
            /*
            else                   // ───── AI 모드 : 2초 버티면 자동 세척
            {
                holdTime += Time.deltaTime;

                if (holdTime >= 2f)
                {
                    useCount = 0;
                    GetComponent<MeshRenderer>().material = mat[useCount];
                    _washCalledThisFrame = true;
                    holdTime = 0f;            // 다음 세척을 위해 리셋
                }
            }
            */
        }
        else
        {
            holdTime = 0f; // 싱크에서 벗어나면 타이머 초기화
        }
    }


    // PlayerController


    public void IncrementUseCount()
    {
        useCount++;
    }
    public void UpdateMaterial()
    {
        gameObject.GetComponent<MeshRenderer>().material = mat[useCount];
    }
    public float GetHoldingTime() { return holdTime; }
    public int GetUseCount() { return useCount; }
    //public GameObject GetNearDust() { return nearDust; }

    public void SetUseCount(int a)
    {
        useCount = 0;
        GetComponent<MeshRenderer>().material = mat[useCount];
    }
    public bool IsNearSink()
    {
        transform.localPosition = righthandPos + offset;
        transform.localRotation = Quaternion.Euler(60, 20, 40);

        if (sink == null) return false;

        float distance = Vector3.Distance(transform.position, sink.transform.position);

        float looseDistance = triggerDistance + 0.5f;  // ← 여유 거리 추가
        return distance <= looseDistance;
    }

}