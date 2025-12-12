using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeSkillView : MonoBehaviour
{
    const int MAX_CHOSEN_SKILLS = 4;
    
    [SerializeField] GameObject skillCardPrefab;
    [SerializeField] Transform hasSkillCardParent;
    [SerializeField] Transform chosenSkillCardParent;
    List<int> hasSkillsID = new List<int>();//TODO 之後從GameDataManager拿
    List<int> chosenSkillsID = new List<int>();
    [SerializeField] Button btn_save;

    void Start()
    {
        hasSkillsID = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 };
        chosenSkillsID = new List<int>(GameDataManager.PlayerData.skillIDs);
        btn_save.onClick.AddListener(SaveChosenSkills);
        
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
            Transform parent = isChosen ? chosenSkillCardParent : hasSkillCardParent;
            
            CreateSkillCard(config, parent, isChosen);
        }
    }
    
    void CreateSkillCard(SkillConfigData config, Transform parent, bool isChosen)
    {
        GameObject cardObj = Instantiate(skillCardPrefab, parent);
        ChooseSkillCard card = cardObj.GetComponent<ChooseSkillCard>();
        card.SetData(config);
        card.isChosen = isChosen;
        card.OnCardClicked = OnSkillCardClicked;
    }
    
    void OnSkillCardClicked(ChooseSkillCard card)
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
                Debug.Log($"已選擇技能已滿（最多 {MAX_CHOSEN_SKILLS} 個）");
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
    public void SaveChosenSkills()
    {
        GameDataManager.PlayerData.skillIDs = new List<int>(chosenSkillsID);
    }
}
