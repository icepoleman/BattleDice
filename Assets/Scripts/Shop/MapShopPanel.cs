using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapShopPanel : MonoBehaviour
{
    [SerializeField] Transform itemParent;
    [SerializeField] GameObject mapShopItemPrefab;

    [SerializeField] Button btn_leave;

    string shopTag = "NoraShop";   // 野外商店技能的Tag
    List<SkillConfigData> shopSkills;

    void Start()
    {
        // 取得野外商店技能列表，並移除玩家已擁有的技能
        shopSkills = SkillDatabase.GetSkillsByTag(shopTag);
        shopSkills.RemoveAll(skill => GameDataManager.HasSkillIDs.Contains(skill.skillID));
        btn_leave.onClick.AddListener(() => Destroy(gameObject));
        GenerateShopItems(3);
    }

    void GenerateShopItems(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (shopSkills == null || shopSkills.Count == 0) break;
            CreateSkillItem();
        }
    }

    void CreateSkillItem()
    {
        if (shopSkills == null || shopSkills.Count == 0) return;

        // 隨機取得一個技能並從列表移除（避免重複）
        int randomIndex = UnityEngine.Random.Range(0, shopSkills.Count);
        SkillConfigData skillData = shopSkills[randomIndex];
        shopSkills.RemoveAt(randomIndex);

        GameObject itemObj = Instantiate(mapShopItemPrefab, itemParent);
        MapShopItem item = itemObj.GetComponent<MapShopItem>();

        string conditionText = string.Format(
            LanguageManager.GetText("T_SkillOnNoraShopCondition"),
            skillData.conditionText,
            skillData.effectText
        );

        item.SetUp(
            skillData.skillName,
            conditionText,
            skillData.price,
            () => OnBuySkill(skillData)
        );

        itemObj.SetActive(true);
    }

    void OnBuySkill(SkillConfigData skillData)
    {
        if (GameDataManager.Gold >= skillData.price)
        {
            GameDataManager.Gold -= skillData.price;
            // 將技能加入玩家擁有的技能列表
            EventCenter.Dispatch(MapEvent.EVENT_GET_SKILL, skillData.skillID); //取得技能
        }
        else
        {
            UIManager.ShowHintBubble(LanguageManager.GetText("T_NotEnoughGold"));
        }
    }

    void OnDestroy()
    {
        // 清理資源或取消訂閱事件（如果有的話）
    }
}
