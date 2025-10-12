using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Collections.Generic;

public class ManaRollerDice : MonoBehaviour
{
    private Image diceImage;
    private Button diceButton;
    private Button skillDiceButton;
    private GameObject obj_onChoose;
    private bool isChosen = false;
    private int sideNum;
    private static Sprite[] diceSprites;

    void Awake()
    {
        diceSprites = Resources.LoadAll<Sprite>("dice/dice_base");
        skillDiceButton = transform.Find("btn_skillon").GetComponent<Button>();
        obj_onChoose = transform.Find("img_onChoose").gameObject;
        skillDiceButton.gameObject.SetActive(false);
        obj_onChoose.SetActive(false);
        diceImage = GetComponent<Image>();
        diceButton = GetComponent<Button>();
        skillDiceButton.onClick.AddListener(() => OnSkillDiceClick());
        AddEvent();
    }
    void AddEvent()
    {
        EventCenter.AddListener(GameEvent.EVENT_CONFIRM_SELECT_SKILL, CheckIsSkillDice);
        EventCenter.AddListener(GameEvent.EVENT_STOP_USE_DICE, StopUseDice);
    }
    void OnDisable()
    {
        EventCenter.RemoveListener(GameEvent.EVENT_CONFIRM_SELECT_SKILL, CheckIsSkillDice);
        EventCenter.RemoveListener(GameEvent.EVENT_STOP_USE_DICE, StopUseDice);
    }
    void CheckIsSkillDice(object[] args)
    {
        StopUseDice(null);
        ISkillData chosenSkill = (ISkillData)args[0];
        List<int> skillNums = chosenSkill.GetNeedDices();
        if (skillNums.Contains(sideNum))
        {
            IsSkillDice();
        }
        else
        {
            UnSkillDice();
        }
    }
    //取消選取技能骰
    void StopUseDice(object[] args)
    {
        diceButton.interactable = true;
        Color color = diceImage.color;
        color.a = 1f; // 不透明
        diceImage.color = color;
        skillDiceButton.gameObject.SetActive(false);
        isChosen = false;
        obj_onChoose.SetActive(false);
    }
    public void SetDice(int _sideNum, Action<int> onClickCallback)
    {
        sideNum = _sideNum;
        diceImage.sprite = diceSprites[_sideNum - 1];
        
        diceButton.onClick.RemoveAllListeners();
        diceButton.onClick.AddListener(() => onClickCallback?.Invoke(sideNum));
    }
    public void UnSkillDice()
    {
        Debug.Log("UnSkillDice");
        diceButton.interactable = false;
        Color color = diceImage.color;
        color.a = 0.5f; // 半透明
    }
    public void IsSkillDice()
    {
        Debug.Log("IsSkillDice");
        skillDiceButton.gameObject.SetActive(true);
    }
    public void UseDice()
    {
        if (isChosen)
        {
            Destroy(gameObject);
        }    
    }
    void OnSkillDiceClick()
    {
        isChosen = !isChosen;
        obj_onChoose.SetActive(isChosen);
        EventCenter.Dispatch(GameEvent.EVENT_CLICK_USE_DICE, sideNum, isChosen);
    }
    public int GetSideNum() => sideNum;
}