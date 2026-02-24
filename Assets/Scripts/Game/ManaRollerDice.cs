using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using DG.Tweening;

public class ManaRollerDice : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] Image diceImage;
    [SerializeField] Button diceButton;
    [SerializeField] Animator anim_lock;
    Action<ManaRollerDice> clickCallback;  // 改為傳遞自己
    Action<ManaRollerDice> rightClickCallback;
    public int sideNum;
    bool isFrozen = false;
    public bool isSelected = false;  // 選取狀態
    public Sprite[] diceSprites = null; // 骰子圖集
    bool isPointerDown = false; // 是否按住

    void Start()
    {
        Sequence seq = DOTween.Sequence();
        // 放大縮小彈跳動畫
        transform.DOKill();
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;
        seq.Append(transform.DOScale(1.2f, 0.1f).SetEase(Ease.OutQuad));
        seq.Append(transform.DOScale(1f, 0.9f).SetEase(Ease.OutElastic));

        // Z軸擺動動畫
        Sequence rotSeq = DOTween.Sequence();
        rotSeq.Append(transform.DORotate(new Vector3(0, 0, 10), 0.1f).SetEase(Ease.OutQuad));
        rotSeq.Append(transform.DORotate(new Vector3(0, 0, -10), 0.15f).SetEase(Ease.InOutQuad));
        rotSeq.Append(transform.DORotate(new Vector3(0, 0, 0), 0.15f).SetEase(Ease.OutQuad));
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isPointerDown||isFrozen) return;
        if (!isSelected)
        {
            transform.DOKill();
            transform.DOScale(1.1f, 0.15f).SetEase(Ease.OutQuad);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isPointerDown||isFrozen) return;
        transform.DOKill();
        transform.DOScale(1f, 0.15f).SetEase(Ease.OutQuad);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        isPointerDown = true;
        if (!isSelected)
        {
            transform.DOKill();
            transform.DOScale(0.9f, 0.1f).SetEase(Ease.OutQuad);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        isPointerDown = false;
        transform.DOKill();
        transform.DOScale(1f, 0.1f).SetEase(Ease.OutQuad);
    }

    public void SetDice(int _sideNum, Action<ManaRollerDice> _onClickCallback, Action<ManaRollerDice> _onRightClickCallback = null, Sprite[] _diceSprites = null, Material _diceMtl = null)
    {
        clickCallback = _onClickCallback;
        rightClickCallback = _onRightClickCallback;
        sideNum = _sideNum;
        diceSprites = _diceSprites;
        diceImage.sprite = diceSprites[_sideNum]; // 設定骰子圖像
        diceButton.onClick.RemoveAllListeners();
        diceButton.onClick.AddListener(() => clickCallback?.Invoke(this));
        diceImage.material = new Material(_diceMtl); // 使用新的材質實例
    }

    // 右鍵點擊處理
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            rightClickCallback?.Invoke(this);
        }
    }

    /// <summary>
    /// 切換選取狀態
    /// </summary>
    public void ToggleSelect()
    {
        SetSelected(!isSelected);
    }

    /// <summary>
    /// 設定選取狀態
    /// </summary>
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        diceImage.material.SetFloat("_OutlineEnabled", selected ? 1f : 0f);
    }

    public void RollDice(int _sideNum, float totalDuration)
    {
        Debug.Log(isFrozen);
        if (isFrozen) return;
        sideNum = _sideNum;
        // 重骰時取消選取
        SetSelected(false);
        // 開始滾動動畫
        PlayRollAnimation(_sideNum, totalDuration);
    }

    private void PlayRollAnimation(int targetSide, float totalDuration)
    {
        int switchCount = 15; // 總共切換幾次
        int currentSwitch = 0;
        int lastRandomSide = -1;

        // 建立動畫序列
        Sequence seq = DOTween.Sequence();

        // 放大縮小彈跳動畫
        transform.DOKill();
        transform.localScale = Vector3.one;
        seq.Append(transform.DOScale(1.2f, 0.1f).SetEase(Ease.OutQuad));
        seq.Append(transform.DOScale(1f, 0.9f).SetEase(Ease.OutElastic));

        // 使用 DOVirtual.Float 控制骰子切換
        DOVirtual.Float(0, 1, totalDuration, (progress) =>
        {
            // 計算當前應該切換到第幾次
            int targetSwitch = Mathf.FloorToInt(progress * switchCount);

            if (targetSwitch > currentSwitch && targetSwitch < switchCount)
            {
                currentSwitch = targetSwitch;

                // 隨機選擇一個不同的骰子面
                int randomSide;
                do
                {
                    randomSide = UnityEngine.Random.Range(1, 7);
                } while (randomSide == lastRandomSide && diceSprites.Length > 1);

                lastRandomSide = randomSide;
                diceImage.sprite = diceSprites[randomSide];
            }
        })
        .SetEase(Ease.OutQuad) // 緩出讓切換由快到慢
        .OnComplete(() =>
        {
            // 最後顯示目標點數
            diceImage.sprite = diceSprites[targetSide];
            Debug.Log("骰子擲出：" + targetSide);
        });
    }

    public void SetFrozen(bool frozen)
    {
        isFrozen = frozen;
        SetSelected(false);
        anim_lock.SetBool("lock", isFrozen);
        transform.DOKill();
        transform.localScale = Vector3.one;
    }

    public bool IsFrozen()
    {
        return isFrozen;
    }
}