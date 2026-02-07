using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public enum ItemType
{
    Skill,
    Gear,
    HealthPotion,
    ExItem,//額外劇情道具(之後實裝)
}
public class MapShopPanel : MonoBehaviour
{
    [SerializeField] Transform itemParent;
    [SerializeField] GameObject mapShopItemPrefab;

    string shopTag = "NoraShop";   // 野外商店技能的Tag
    List<SkillConfigData> shopSkills;

    void Start()
    {
        // 取得野外商店技能列表，並移除玩家已擁有的技能
        shopSkills = SkillDatabase.GetSkillsByTag(shopTag);
        shopSkills.RemoveAll(skill => GameDataManager.HasSkillIDs.Contains(skill.skillID));

        GenerateShopItems(3);
    }

    void GenerateShopItems(int count)
    {
        for (int i = 0; i < count; i++)
        {
            ItemType itemType = GetRandomItemType();
            CreateShopItem(itemType);
        }
    }

    ItemType GetRandomItemType()
    {
        // 建立加權機率列表 (ItemType, 權重)
        List<(ItemType type, int weight)> weightedTypes = new List<(ItemType, int)>();

        // 如果還有技能可買，加入 Skill (60%)
        if (shopSkills != null && shopSkills.Count > 0)
        {
            weightedTypes.Add((ItemType.Skill, 60));
        }

        // 其他類型總是可用
        weightedTypes.Add((ItemType.Gear, 20));
        weightedTypes.Add((ItemType.HealthPotion, 20));

        // 計算總權重
        int totalWeight = 0;
        foreach (var item in weightedTypes)
        {
            totalWeight += item.weight;
        }

        // 隨機選擇
        int randomValue = UnityEngine.Random.Range(0, totalWeight);
        int currentWeight = 0;

        foreach (var item in weightedTypes)
        {
            currentWeight += item.weight;
            if (randomValue < currentWeight)
            {
                return item.type;
            }
        }

        return weightedTypes[0].type;
    }

    void CreateShopItem(ItemType itemType)
    {
        GameObject itemObj = Instantiate(mapShopItemPrefab, itemParent);
        MapShopItem item = itemObj.GetComponent<MapShopItem>();

        switch (itemType)
        {
            case ItemType.Skill:
                SetupSkillItem(item);
                break;
            case ItemType.Gear:
                item.SetUp(
                    LanguageManager.GetText("T_GearPack"),
                    LanguageManager.GetFormat("T_GearPack_Desc", 1, 6),
                    ItemType.Gear,
                    () => OnBuyGear()
                );
                break;
            case ItemType.HealthPotion:
                item.SetUp(
                    LanguageManager.GetText("T_HealthPotion"),
                    LanguageManager.GetText("T_HealthPotion_Desc"),
                    ItemType.HealthPotion,
                    () => OnBuyHealthPotion()
                );
                break;
            case ItemType.ExItem:
                // TODO: 實作 ExItem
                break;
        }
        itemObj.SetActive(true);
    }

    void SetupSkillItem(MapShopItem item)
    {
        if (shopSkills == null || shopSkills.Count == 0) return;

        // 隨機取得一個技能並從列表移除（避免重複）
        int randomIndex = UnityEngine.Random.Range(0, shopSkills.Count);
        SkillConfigData skillData = shopSkills[randomIndex];
        shopSkills.RemoveAt(randomIndex);
        string conditionText = string.Format(
            LanguageManager.GetText("T_SkillOnNoraShopCondition"),
            skillData.conditionText,
            skillData.effectText
        );
        item.SetUp(
            skillData.skillName,
            conditionText,
            ItemType.Skill,
            () => OnBuySkill(skillData)
        );
    }

    async void OnBuySkill(SkillConfigData skillData)
    {
        // 將技能加入玩家擁有的技能列表
        EventCenter.Dispatch(MapEvent.EVENT_GET_SKILL, skillData.skillID); //取得技能
        Destroy(gameObject);
    }

    async void OnBuyGear()
    {
        // 隨機獲得1~6個齒輪
        int gearAmount = UnityEngine.Random.Range(1, 7);
        EventCenter.Dispatch(MapEvent.EVENT_GET_GEAR, gearAmount); //取得齒輪

        Debug.Log($"購買齒輪包，獲得 {gearAmount} 個齒輪");
        Destroy(gameObject);
    }
    void OnBuyHealthPotion()
    {
        // 回復100血
        EventCenter.Dispatch(MapEvent.EVENT_RECOVER_HEALTH, 100); //回血
        Debug.Log($"購買生命藥水，回復100血");
        Destroy(gameObject);
    }
}
