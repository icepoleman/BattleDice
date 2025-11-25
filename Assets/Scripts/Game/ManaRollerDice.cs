using UnityEngine;
using UnityEngine.UI;
using System;

public class ManaRollerDice : MonoBehaviour
{
    private Image diceImage;
    private Button diceButton;
    private int sideNum;
    void Awake()
    {
        diceImage = transform.GetComponent<Image>();
        diceButton = transform.GetComponent<Button>();
    }
    public void SetDice(int _sideNum, Action<int> onClickCallback)
    {
        sideNum = _sideNum;
        diceImage.sprite = ResourcesLoader.GetDiceSprite(_sideNum);

        diceButton.onClick.RemoveAllListeners();
        diceButton.onClick.AddListener(() => onClickCallback?.Invoke(sideNum));
    }
}