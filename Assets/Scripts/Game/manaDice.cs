using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;

public class manaDice : MonoBehaviour
{
    [SerializeField] Image img_dice;
    RectTransform rect_dice;
    public int sideNum;
    bool isSelected = false;  // 選取狀態
    [SerializeField] Material diceMaterial;
    void Awake()
    {
        rect_dice = img_dice.GetComponent<RectTransform>();
        img_dice.material = new Material(diceMaterial);

        // Y軸彈跳動畫 0 -> 70 -> 0
        Sequence seq = DOTween.Sequence();
        seq.Append(rect_dice.DOAnchorPosY(70f, 0.2f).SetEase(Ease.OutQuad));
        seq.Append(rect_dice.DOAnchorPosY(0f, 0.4f).SetEase(Ease.OutBounce));
    }
    public void SetDiceFace(int _sideNum,Sprite diceSp)
    {
        sideNum = _sideNum;
        img_dice.sprite = diceSp;
    }
    public void OnChoose()
    {
        img_dice.material.SetFloat("_OutlineEnabled", 1f);
        isSelected = true;
    }
    public void ClearChoose()
    {
        if (isSelected)
            Destroy(gameObject, 0.3f);
    }
}
