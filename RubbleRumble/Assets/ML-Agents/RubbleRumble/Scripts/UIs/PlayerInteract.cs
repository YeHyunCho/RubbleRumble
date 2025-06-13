using UnityEngine;

// ��ȣ�ۿ� UI ���¸� ���������� ����
public enum InteractUIState
{
    None = 0,           // ��Ȱ��ȭ ����
    PressE = 1,         // EŰ ������ (���, ������ ��)
    PressQ = 2,         // QŰ ������ (��Ȱ�� ����, ��ô ���� ��)
    Holding = 3         // ���൵ ǥ�� (��Ȱ����, ��ô��)
}

public class PlayerInteract : MonoBehaviour
{

    [SerializeField] private PlayerInputHandler playerInputHandler;
    public Mop mop { get; private set; }
    public InteractUIState CurrentUIState { get; private set; }

    private void Awake()
    {
        //interactRange = 3;
    }

    private void Start()
    {
        if (playerInputHandler == null)
            playerInputHandler = GetComponent<PlayerInputHandler>();
    }

    private void Update()
    {
        UpdateInteractUIState();
    }

    // ��� ��ȣ�ۿ� ������ üũ�ϰ� ���� UI ���¸� �����ϴ� �޼���
    private void UpdateInteractUIState()
    {
        // �⺻ ����: ��Ȱ��ȭ
        InteractUIState newState = InteractUIState.None;

        // ���ڷ� ��� ���� ���� üũ
        if (playerInputHandler.GetCurrentTool() == 1) // ���� �ε��� 1�� ���ڷ��� ���
        {
            newState = CheckBroomInteract();
            CurrentUIState = newState;
            return;
        }

        // ��ɷ� ��� ���� ���� üũ
        if (playerInputHandler.GetCurrentTool() == 2) // ���� �ε��� 2�� ��ɷ��� ���
        {
            newState = CheckMopInteract();
            CurrentUIState = newState;
            return;
        }

        // �۾��� ��ó������ ��ȣ�ۿ� üũ
        if (playerInputHandler.GetIsNearWorkbench())
        {
            newState = CheckWorkbenchInteract();
            CurrentUIState = newState;
            return;
        }

        // �� ������ ������ ���� �� �ִ� ���� üũ
        if (playerInputHandler.GetCurrentTool() == 0 && !playerInputHandler.GetIsHoldingTrash())
        {
            newState = CheckHandInteract();
            CurrentUIState = newState;
            return;
        }

        // �����⸦ ��� �ִ� ���¿����� ��ȣ�ۿ� üũ
        if (playerInputHandler.GetCurrentTool() == 0 && playerInputHandler.GetIsHoldingTrash())
        {
            newState = CheckTrashInteract();
            CurrentUIState = newState;
            return;
        }

        CurrentUIState = newState;
    }


    // ���ڷ� ���� ��ȣ�ۿ� üũ
    private InteractUIState CheckBroomInteract()
    {
        // �÷��̾� ��ó�� ������ �ְ�, ���ڷ� ����� �� ������ ��ȣ�ۿ� E Ȱ��ȭ 
        if (playerInputHandler.GetIsNearObject())
        {
            if (playerInputHandler.GetNearObject().CompareTag("Dust"))
            {
                return InteractUIState.PressE;
            }
        }

        return InteractUIState.None;
    }
    // ��ɷ� ���� ��ȣ�ۿ� üũ
    private InteractUIState CheckMopInteract()
    {
        // ��ɷ� ���� ����
        if (mop == null)
        {
            Mop[] mops = FindObjectsOfType<Mop>();
            if (mops[0].isPlayer) { mop = mops[0]; }
            else { mop = mops[1]; }
        }

        // �����뿡�� ��ɷ� ��ô ���� üũ
        float sinkDistance = Vector3.Distance(mop.transform.position, mop.sink.transform.position);
        if (sinkDistance <= mop.triggerDistance && mop.GetUseCount() >= 2)
        {
            if (Input.GetKey(KeyCode.Q) && mop.GetHoldingTime() > 0f && mop.GetHoldingTime() < 2f)
            {
                return InteractUIState.Holding; // Ȧ���� Ȱ��ȭ
            }

            return InteractUIState.PressQ; // ��ȣ�ۿ� Q Ȱ��ȭ (��ô)
        }

        // �÷��̾� ��ó�� ������� �ְ�, ��ɷ� ����� �� ������ ��ȣ�ۿ� E Ȱ��ȭ 
        if (playerInputHandler.GetIsNearObject())
        {
            // TODO: Water prefab �±� Water�� ���� �� �ּ� ���� �� ���
            //if (playerInputHandler.GetNearObject().CompareTag("Water") && mop.GetUseCount() < 2)
            if (playerInputHandler.GetNearObject().CompareTag("Dust") && mop.GetUseCount() < 2)
            {
                return InteractUIState.PressE;
            }
        }

        return InteractUIState.None;
    }

    // �� ���� �� ��ȣ�ۿ� üũ
    private InteractUIState CheckHandInteract()
    {
        GameObject nearObject = playerInputHandler.GetNearObject();
        if (nearObject != null)
        {
            if (nearObject.CompareTag("Can") || nearObject.CompareTag("Box") || nearObject.CompareTag("UnfoldedBox"))
            {
                return InteractUIState.PressE;
            }
        }

        return InteractUIState.None;
    }

    // �����⸦ ��� ���� �� ��ȣ�ۿ� üũ
    private InteractUIState CheckTrashInteract()
    {
        GameObject heldObject = playerInputHandler.GetHeldObject();
        TrashManager heldTrash = heldObject.GetComponent<TrashManager>();

        // �и������� ��ó������ ��ȣ�ۿ�
        if (playerInputHandler.GetIsNearRecyclingBin())
        {
            // ��� �ִ� �����⸦ ���� �� �ְ� ���� �� �ִ� ���������� ��ó�� ������
            if (heldTrash.trashData.readyToThrowAway && playerInputHandler.GetCurRecycleBin().CompareTag(heldTrash.trashData.interactTrashbin))
            {
                return InteractUIState.PressE; // ��ȣ�ۿ� E Ȱ��ȭ (������)
            }

            return InteractUIState.None; // Box �±״� ���� �� ����
        }

        return InteractUIState.None;
    }

    // �۾��� ��ó������ ��ȣ�ۿ� üũ
    private InteractUIState CheckWorkbenchInteract()
    {
        // �տ� �����⸦ ��� �ִ� ���
        if (playerInputHandler.GetHeldObject() != null)
        {
            if (!playerInputHandler.GetHeldObject().CompareTag("Box"))  // ��� �ִ� ������Ʈ�� �ڽ��� �ƴϸ�
            {
                return InteractUIState.None;    // ��ȣ�ۿ� ��Ȱ��ȭ
            }
            else // ��� �ִ� ������Ʈ�� �ڽ��̸�
            {
                return InteractUIState.PressQ;    // ��ȣ�ۿ� QȰ��ȭ (�۾��� ���� �ø���)
            }
        }
        
        GameObject trashOnWorkbench = playerInputHandler.GetTrashOnWorkbench();

        if (trashOnWorkbench == null)   // �۾��� ���� �����Ⱑ ������
        {
            return InteractUIState.None;    // ��ȣ�ۿ� ��Ȱ��ȭ
        }

        if (trashOnWorkbench.CompareTag("Box")) // �۾��� ���� ���ڰ� ������
        {
            if (Input.GetKey(KeyCode.Q))
            {
                return InteractUIState.Holding; // Ȧ���� Ȱ��ȭ
            }

            return InteractUIState.PressQ; // ��ȣ�ۿ� Q Ȱ��ȭ (��Ȱ�� ����)
        }

        if (trashOnWorkbench.CompareTag("UnfoldedBox")) // �۾��� ���� ������ ���ڰ� ������
        {
            return InteractUIState.PressE; // ��ȣ�ۿ� E Ȱ��ȭ (����)
        }

        return InteractUIState.None;
    }
}