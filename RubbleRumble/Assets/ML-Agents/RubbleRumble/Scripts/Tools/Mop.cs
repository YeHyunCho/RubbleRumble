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

    //Mop �������� ������ ����
    private Vector3 offset = new Vector3(0.02f, -0.1f, 0.04f);

    private int useCount;
    //public float triggerDistance = 0.1f;
    public float triggerDistance = 5f;

    private float holdTime = 0f;

    float reward = 0f;

    private void Awake()
    {
        righthandPos = gameObject.GetComponentInParent<Transform>().localPosition;
        //sink = GameObject.FindWithTag("Sink");
    }

    private void Start()
    {
        player = GameObject.FindWithTag("Player");
        agent = GameObject.FindWithTag("Agent");

        // mop���� �÷��̾� ��ġ�� agent ��ġ���� ������
        // �ش� mop�� �÷��̾ ��� �ִ� �����Ƿ� sink�� �÷��̾� ���� sink�� ����
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

        //Mop �������� ���� ����
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
            if (isPlayer)          // ���������� ����� ������ �� (���� ��� ����)
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
            else                   // ���������� AI ��� : 2�� ��Ƽ�� �ڵ� ��ô
            {
                holdTime += Time.deltaTime;

                if (holdTime >= 2f)
                {
                    useCount = 0;
                    GetComponent<MeshRenderer>().material = mat[useCount];
                    _washCalledThisFrame = true;
                    holdTime = 0f;            // ���� ��ô�� ���� ����
                }
            }
            */
        }
        else
        {
            holdTime = 0f; // ��ũ���� ����� Ÿ�̸� �ʱ�ȭ
        }
    }

    public void WashMopNearSink_Agent(bool qhold)
    {
        Vector3 Sink_xz = new Vector3(-2.72f, -15.72f, -4.62f);
        float distance = Vector3.Distance(transform.position, Sink_xz);

        if (distance <= triggerDistance)
        {
            if (qhold)
            {
                reward += 2f;
                useCount = 0;
                gameObject.GetComponent<MeshRenderer>().material = mat[useCount];
            }
        }
    }

    // PlayerController�� ���� �޼���
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
    public float Getreward() { return reward; }
    public void Setreward() { reward = 0f; }
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

        float looseDistance = triggerDistance + 0.5f;  // �� ���� �Ÿ� �߰�
        return distance <= looseDistance;
    }

}