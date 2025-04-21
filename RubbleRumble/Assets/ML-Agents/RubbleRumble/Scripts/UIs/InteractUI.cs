using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractUI : MonoBehaviour
{
    private Transform camTransform;
    private Transform playerTransform;
    private float fixedYPosition;
    private Quaternion fixedRotation;

    [SerializeField] private GameObject interactTxtObj;
    [SerializeField] private GameObject holdingBarObj;
    [SerializeField] private TextMeshProUGUI interactTxt;
    [SerializeField] private Image holdingBarImg;

    [SerializeField] private PlayerController playerController ;
    [SerializeField] private PlayerInteract playerInteract ;
    [SerializeField] private Mop mop;
    private float unfoldDuration; // 박스를 펼치는 데 필요한 시간

    private void Awake()
    {
        SetUITransform();

        holdingBarImg.fillAmount = 0;   // 홀딩바 초기화
        
        // 상호작용 UI 비활성화
        interactTxtObj.SetActive(false);
        holdingBarObj.SetActive(false);
    }

    private void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        playerInteract = player.GetComponent<PlayerInteract>();
        playerController = player.GetComponent<PlayerController>();
        unfoldDuration = playerController.GetUnfoldDuration();
    }

    private void Update()
    {
        // 플레이어 이동에 따라 UI 이동
        float playerX = playerTransform.position.x;
        float playerZ = playerTransform.position.z;
        transform.position = new Vector3(playerX, fixedYPosition, playerZ);

        transform.rotation = fixedRotation; // 회전값 고정

        UpdateInteractUI(playerInteract.InteractUIState);
    }

    // 상호작용 UI의 위치와 회전값 초기화
    private void SetUITransform()
    {
        camTransform = Camera.main.transform;   // 메인 카메라 기준으로 카메라 위치 설정
        playerTransform = GameObject.FindWithTag("Player").transform;

        fixedYPosition = transform.position.y;  // 고정할 y축 값

        // 회전값 고정
        fixedRotation = camTransform.rotation;
        transform.rotation = fixedRotation;
    }

    // UI 상태에 따라 상호작용 UI 갱신
    private void UpdateInteractUI(int interactUIState)
    {
        switch (interactUIState)
        {
            case 3: // Q를 홀딩하고 있는 경우
                {
                    interactTxtObj.SetActive(false);    // 상호작용 텍스트 비활성화
                    holdingBarObj.SetActive(true);      // 홀딩바 활성화

                    if (ToolManager.Instance.currentTool == 0)  // 맨손으로 작업하고 있으면
                        holdingBarImg.fillAmount = playerController.GetHoldingTime() / unfoldDuration;  // 홀딩바 갱신
                    else    // 대걸레를 세척하고 있으면
                    {
                        if (mop == null)
                            mop = FindObjectOfType<Mop>();
                        holdingBarImg.fillAmount = mop.GetHoldingTime() / unfoldDuration;  // 홀딩바 갱신
                    }
                    break;
                }
            case 2: // Q를 눌러야 하는 경우
                {
                    interactTxtObj.SetActive(true);     // 상호작용 텍스트 활성화
                    holdingBarObj.SetActive(false);     // 홀딩바 비활성화
                    interactTxt.text = "[Q]";           // Q키 누르도록 안내
                    break;
                }
            case 1: // E를 눌러야 하는 경우
                {
                    interactTxtObj.SetActive(true);     // 상호작용 텍스트 활성화
                    holdingBarObj.SetActive(false);     // 홀딩바 비활성화
                    interactTxt.text = "[E]";           // E키 누르도록 안내
                    break;
                }
            case 0:
                {
                    interactTxtObj.SetActive(false);     // 상호작용 텍스트 비활성화
                    holdingBarObj.SetActive(false);     // 홀딩바 비활성화
                    break;
                }
            default:
                {
                    Debug.Log("InteractUI Error");
                    break;
                }
        }
    }
}
