using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ChangeSkillView : MonoBehaviour
{
    const int MAX_CHOSEN_SKILLS = 4;

    [SerializeField] GameObject hasSkillCardPrefab;
    [SerializeField] GameObject chosenSkillCardPrefab;
    [SerializeField] Transform hasSkillCardParent;
    [SerializeField] Transform chosenSkillCardParent;
    [SerializeField] BaseMouseEffectShake mouseEffectShake;
    List<int> hasSkillsID = new List<int>();
    List<int> chosenSkillsID = new List<int>();
    [SerializeField] Button btn_back;

    void OnEnable()
    {
        EventCenter.AddListener(MapEvent.EVENT_UNCHOOSE_SKILL, OnUnchooseSkill);
    }
    void OnDestroy()
    {
        EventCenter.RemoveListener(MapEvent.EVENT_UNCHOOSE_SKILL, OnUnchooseSkill);
    }
    async void Start()
    {
        if(GameDataManager.TestMode)
        {
            for (int i = 1; i <= 34; i++)
            {
                GameDataManager.HasSkillIDs.Add(i);
            }
        }
        // 從GameDataManager取得擁有的技能ID
        hasSkillsID = new List<int>(GameDataManager.HasSkillIDs);
        chosenSkillsID = new List<int>(GameDataManager.PlayerData.skillIDs);
        btn_back.onClick.AddListener(() => SaveChosenSkills());
        GenerateSkillCards();
    }

    void GenerateSkillCards()
    {
        // 清空現有卡片
        ClearChildren(hasSkillCardParent);
        ClearChildren(chosenSkillCardParent);

        // 生成擁有的技能卡片
        foreach (var skillID in hasSkillsID)
        {
            var config = SkillDatabase.GetSkillConfig(skillID);
            bool isChosen = chosenSkillsID.Contains(skillID);

            if (isChosen)
            {
                CreateChosenSkillCard(config);
            }

            CreateHasSkillCard(config, isChosen);
        }
    }
    void CreateChosenSkillCard(SkillConfigData config)
    {
        mouseEffectShake.StartShake();
        GameObject cardObj = Instantiate(chosenSkillCardPrefab, chosenSkillCardParent);
        cardObj.SetActive(true);
        ChooseSkillCard card = cardObj.GetComponent<ChooseSkillCard>();
        card.SetData(config);
    }
    void OnUnchooseSkill(object[] args)
    {
        int skillID = (int)args[0];
        chosenSkillsID.Remove(skillID);
    }
    void CreateHasSkillCard(SkillConfigData config, bool isChosen)
    {
        GameObject cardObj = Instantiate(hasSkillCardPrefab, hasSkillCardParent);
        HasSkillCard card = cardObj.GetComponent<HasSkillCard>();
        cardObj.SetActive(true);
        card.SetData(config, OnHasSkillCardClicked);
        card.isChosen = isChosen;
    }

    async void OnHasSkillCardClicked(HasSkillCard card)
    {
        int skillID = card.SkillID;

        if (card.isChosen)
        {
            // 取消選擇：從已選擇列表移除
            card.isChosen = false;
            EventCenter.Dispatch(MapEvent.EVENT_UNCHOOSE_SKILL, skillID);
        }
        else
        {
            // 選擇技能：檢查是否已滿
            if (chosenSkillsID.Count >= MAX_CHOSEN_SKILLS)
            {
                await UIManager.ShowHintBubble(LanguageManager.GetFormat("T_ChangeSkill_SkillMax", MAX_CHOSEN_SKILLS));
                return;
            }

            card.isChosen = true;
            chosenSkillsID.Add(skillID);
            CreateChosenSkillCard(SkillDatabase.GetSkillConfig(skillID));
        }
    }

    void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }

    // 儲存已選擇的技能到 PlayerData
    public void SaveChosenSkills()
    {
        if (chosenSkillsID.Count == 0)
        {
            // 必須至少選擇一個技能
            UIManager.ShowHintBubble(LanguageManager.GetText("T_ChangeSkill_Limit"));
            return;
        }
        GameDataManager.PlayerData.skillIDs = new List<int>(chosenSkillsID);
        SaveManager.AutoSave();
        Destroy(this.gameObject);
        EventCenter.Dispatch(PreparationRoomEvent.OPEN_FIRE);
    }
}
