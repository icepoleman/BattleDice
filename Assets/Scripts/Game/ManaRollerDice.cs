using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class ManaRollerDice : MonoBehaviour, IPointerClickHandler
{
    private Image diceImage;
    private Button diceButton;
    private int sideNum;
    Action<ManaRollerDice> clickCallback;  // 改為傳遞自己
    Action<ManaRollerDice> rightClickCallback;
    [SerializeField] GameObject obj_onChoose;
    bool isFrozen = false;
    bool isSelected = false;  // 選取狀態
    
    public int SideNum => sideNum;
    public bool IsSelected => isSelected;
    
    void Awake()
    {
        diceImage = transform.GetComponent<Image>();
        diceButton = transform.GetComponent<Button>();
        if (obj_onChoose != null) obj_onChoose.SetActive(false);
    }
    
    public void SetDice(int _sideNum, Action<ManaRollerDice> _onClickCallback, Action<ManaRollerDice> _onRightClickCallback = null)
    {
        clickCallback = _onClickCallback;
        rightClickCallback = _onRightClickCallback;
        sideNum = _sideNum;
        diceImage.sprite = ResourcesLoader.GetDiceSprite(_sideNum);

        diceButton.onClick.RemoveAllListeners();
        diceButton.onClick.AddListener(() => clickCallback?.Invoke(this));
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
        if (obj_onChoose != null)
        {
            obj_onChoose.SetActive(isSelected);
        }
    }
    
    public void RollDice(int _sideNum)
    {
        Debug.Log(isFrozen);
        if (isFrozen) return;
        sideNum = _sideNum;
        diceImage.sprite = ResourcesLoader.GetDiceSprite(_sideNum);
        Debug.Log("骰子擲出：" + _sideNum);
        // 重骰時取消選取
        SetSelected(false);
    }
    
    public void SetFrozen(bool frozen)
    {
        isFrozen = frozen;
        // 可以在這裡改變骰子的外觀以表示凍結狀態
        diceImage.color = isFrozen ? Color.blue : Color.white;
    }
    
    public bool IsFrozen()
    {
        return isFrozen;
    }
}