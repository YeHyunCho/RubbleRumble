using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleFlotingTrash : MonoBehaviour
{
    public float floatStrength = 0.5f;      
    public float rotationSpeed = 20f;      
    private Vector3 initialPos;
    private float randomOffset;

    void Start()
    {
        initialPos = transform.position;
        randomOffset = Random.Range(0f, 100f);  
    }

    void Update()
    {
        // 위아래로 흔들리기
        float newY = Mathf.Sin(Time.time + randomOffset) * floatStrength;
        transform.position = initialPos + new Vector3(0, newY, 0);

        // 천천히 회전
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        transform.Rotate(Vector3.right * (rotationSpeed / 3f) * Time.deltaTime);
    }
}
