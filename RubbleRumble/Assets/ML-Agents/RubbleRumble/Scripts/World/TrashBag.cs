using UnityEngine;
using DG.Tweening;

public class TrashBag : MonoBehaviour
{
    [SerializeField] private GameObject trashBin;

    private TrashBin trashBinSc;
    private TrashManager trashBagData;

    private Vector3 firstPos;
    private Vector3 secondPos;
    private Vector3 thirdPos;
    private Vector3 trashBagScale;

    private void Start()
    {
        trashBagData = GetComponent<TrashManager>();
        trashBinSc = trashBin.GetComponent<TrashBin>();

        trashBagScale = transform.localScale;
        firstPos = transform.position;

        secondPos = new Vector3(-1.78f, -12f, 5.94f);
        thirdPos = new Vector3(-1.78f, -18f, 5.94f);
    }

    private void Update()
    {
        if(trashBagData.trashData.readyToThrowAway)
        {
            TrashBinInteraction();

            trashBagData.trashData.readyToThrowAway = false;
        }

    }

    private void TrashBinInteraction()
    {
        Sequence seq = DOTween.Sequence();
        Vector3[] path = new Vector3[] { secondPos, thirdPos };

        seq.Join(trashBinSc.OpenLid())
            .AppendInterval(0.1f)
            .Append(transform.DOLocalPath(path, 2f, PathType.CatmullRom)
            .SetEase(Ease.InOutQuad)
            .SetOptions(false))
            .AppendInterval(0.1f)
            .Append(transform.DOScale(trashBagScale, 0.3f))
            .Join(transform.DOMove(firstPos, 0.3f))
            .Join(trashBinSc.CloseLid());
    }
}
