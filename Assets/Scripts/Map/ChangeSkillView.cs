using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class ChangeSkillView : MonoBehaviour
{
    const int MAX_CHOSEN_SKILLS = 4;

    [SerializeField] GameObject hasSkillCardPrefab;
    [SerializeField] GameObject chosenSkillCardPrefab;
    [SerializeField] Transform hasSkillCardParent;
    [SerializeField] Transform chosenSkillCardParent;
    [SerializeField] SkeletonGraphic penShake;
    List<int> hasSkillsID = new List<int>();
    List<int> chosenSkillsID = new List<int>();
    [SerializeField] Button btn_back;

    List<ChooseSkillCard> chosenSkillCards = new List<ChooseSkillCard>();
    List<HasSkillCard> hasSkillCards = new List<HasSkillCard>();

    async void Start()
    {
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
        penShake.AnimationState.SetAnimation(0, "pen", false);
        GameObject cardObj = Instantiate(chosenSkillCardPrefab, chosenSkillCardParent);
        cardObj.SetActive(true);
        ChooseSkillCard card = cardObj.GetComponent<ChooseSkillCard>();
        card.SetData(config, OnChosenSkillCardClicked);
        chosenSkillCards.Add(card);
    }
    void OnChosenSkillCardClicked(ChooseSkillCard card)
    {
        UnchooseSkill(card.SkillID);
    }
    void UnchooseSkill(int skillID)
    {
        penShake.AnimationState.SetAnimation(0, "pen", false);
        chosenSkillsID.RemoveAll(id => id == skillID);

        foreach (var chosenCard in chosenSkillCards)
        {
            if (chosenCard != null && chosenCard.SkillID == skillID)
            {
                Destroy(chosenCard.gameObject);
            }
        }
        chosenSkillCards.RemoveAll(chosenCard => chosenCard == null || chosenCard.SkillID == skillID);

        foreach (var hasSkillCard in hasSkillCards)
        {
            if (hasSkillCard != null && hasSkillCard.SkillID == skillID)
            {
                hasSkillCard.isChosen = false;
            }
        }
    }
    void CreateHasSkillCard(SkillConfigData config, bool isChosen)
    {
        GameObject cardObj = Instantiate(hasSkillCardPrefab, hasSkillCardParent);
        HasSkillCard card = cardObj.GetComponent<HasSkillCard>();
        cardObj.SetActive(true);
        card.SetData(config, OnHasSkillCardClicked);
        hasSkillCards.Add(card);
        card.isChosen = isChosen;
    }

    void OnHasSkillCardClicked(HasSkillCard card)
    {
        int skillID = card.SkillID;

        if (card.isChosen)
        {
            // 取消選擇：從已選擇列表移除
            UnchooseSkill(skillID);
        }
        else
        {
            // 選擇技能：檢查是否已滿
            if (chosenSkillsID.Count >= MAX_CHOSEN_SKILLS)
            {
                EventCenter.Dispatch(StateEvent.EVENT_SHOW_HINT_BUBBLE, LanguageManager.GetFormat("T_ChangeSkill_SkillMax", MAX_CHOSEN_SKILLS));
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
            EventCenter.Dispatch(StateEvent.EVENT_SHOW_HINT_BUBBLE, LanguageManager.GetText("T_ChangeSkill_Limit"));
            return;
        }
        GameDataManager.PlayerData.skillIDs = new List<int>(chosenSkillsID);
        SaveManager.AutoSave();
        Destroy(this.gameObject);
    }
}
