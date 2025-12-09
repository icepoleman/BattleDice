using UnityEngine;
using UnityEngine.UI;
using System;

public class ManaRollerDice : MonoBehaviour
{
    private Image diceImage;
    private Button diceButton;
    private int sideNum;
    Action<int> clickCallback;
    bool isFrozen = false;
    void Awake()
    {
        diceImage = transform.GetComponent<Image>();
        diceButton = transform.GetComponent<Button>();
    }
    public void SetDice(int _sideNum, Action<int> _onClickCallback)
    {
        clickCallback = _onClickCallback;
        sideNum = _sideNum;
        diceImage.sprite = ResourcesLoader.GetDiceSprite(_sideNum);

        diceButton.onClick.RemoveAllListeners();
        diceButton.onClick.AddListener(() => clickCallback?.Invoke(sideNum));
    }
    public void RollDice(int _sideNum)
    {
        Debug.Log(isFrozen);
        if (isFrozen) return;
        sideNum = _sideNum;
        diceImage.sprite = ResourcesLoader.GetDiceSprite(_sideNum);
        Debug.Log("骰子擲出：" + _sideNum);
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