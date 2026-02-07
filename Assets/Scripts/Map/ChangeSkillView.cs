using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ChangeSkillView : MonoBehaviour
{
    const int MAX_CHOSEN_SKILLS = 4;

    [SerializeField] GameObject skillCardPrefab;
    [SerializeField] Transform hasSkillCardParent;
    [SerializeField] Transform chosenSkillCardParent;
    List<int> hasSkillsID = new List<int>();
    List<int> chosenSkillsID = new List<int>();
    [SerializeField] Button btn_save;
    [SerializeField] Button btn_back;

    void Start()
    {
        // 從GameDataManager取得擁有的技能ID
        hasSkillsID = new List<int>(GameDataManager.HasSkillIDs);
        chosenSkillsID = new List<int>(GameDataManager.PlayerData.skillIDs);
        btn_save.onClick.AddListener(SaveChosenSkills);
        btn_back.onClick.AddListener(() => Destroy(gameObject));
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
            if (skillID > 100) continue; // 跳過怪物技能ID
            var config = SkillDatabase.GetSkillConfig(skillID);
            bool isChosen = chosenSkillsID.Contains(skillID);
            Transform parent = isChosen ? chosenSkillCardParent : hasSkillCardParent;

            CreateSkillCard(config, parent, isChosen);
        }
    }

    void CreateSkillCard(SkillConfigData config, Transform parent, bool isChosen)
    {
        GameObject cardObj = Instantiate(skillCardPrefab, parent);
        ChooseSkillCard card = cardObj.GetComponent<ChooseSkillCard>();
        card.SetData(config, OnSkillCardClicked);
        card.isChosen = isChosen;
    }

    async void OnSkillCardClicked(ChooseSkillCard card)
    {
        int skillID = card.SkillID;

        if (card.isChosen)
        {
            // 取消選擇：從已選擇列表移除，移動到擁有列表
            card.isChosen = false;
            chosenSkillsID.Remove(skillID);
            card.transform.SetParent(hasSkillCardParent);
        }
        else
        {
            // 選擇技能：檢查是否已滿
            if (chosenSkillsID.Count >= MAX_CHOSEN_SKILLS)
            {
                await CommonUIManager.ShowHintBubble(LanguageManager.GetFormat("T_ChangeSkill_SkillMax", MAX_CHOSEN_SKILLS));
                return;
            }

            card.isChosen = true;
            chosenSkillsID.Add(skillID);
            card.transform.SetParent(chosenSkillCardParent);
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
    public async void SaveChosenSkills()
    {
        if (chosenSkillsID.Count == 0)
        {
            // 必須至少選擇一個技能
            await CommonUIManager.ShowHintBubble(LanguageManager.GetText("T_ChangeSkill_Limit"));
            return;
        }
        GameDataManager.PlayerData.skillIDs = new List<int>(chosenSkillsID);
        SaveManager.AutoSave();
        Destroy(this.gameObject);
    }
}
