using System;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    Skill,
    Gear,
    HealthPotion,
    GiftItem,//禮物道具(還沒實作)
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
        // 建立可用的 ItemType 列表
        List<ItemType> availableTypes = new List<ItemType>();
        
        // 如果還有技能可買，加入 Skill
        if (shopSkills != null && shopSkills.Count > 0)
        {
            availableTypes.Add(ItemType.Skill);
        }
        
        // 其他類型總是可用
        availableTypes.Add(ItemType.Gear);
        availableTypes.Add(ItemType.HealthPotion);
        availableTypes.Add(ItemType.GiftItem); // 還沒實作
        
        return availableTypes[UnityEngine.Random.Range(0, availableTypes.Count)];
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
                // TODO: 實作 Gear
                break;
            case ItemType.HealthPotion:
                // TODO: 實作 HealthPotion
                break;
            case ItemType.GiftItem:
                // TODO: 實作 GiftItem
                break;
        }
    }
    
    void SetupSkillItem(MapShopItem item)
    {
        if (shopSkills == null || shopSkills.Count == 0) return;
        
        // 隨機取得一個技能並從列表移除（避免重複）
        int randomIndex = UnityEngine.Random.Range(0, shopSkills.Count);
        SkillConfigData skillData = shopSkills[randomIndex];
        shopSkills.RemoveAt(randomIndex);
        
       /* item.SetUp(
            skillData.skillName,
            skillData.skillDescription,
            skillData.price,
            ItemType.Skill,
            () => OnBuySkill(skillData)
        );*/
    }
    
    void OnBuySkill(SkillConfigData skillData)
    {
        // 檢查金幣是否足夠
        if (GameDataManager.Gold < skillData.price)
        {
            Debug.Log("金幣不足");
            return;
        }
        
        // 扣除金幣
        GameDataManager.Gold -= skillData.price;
        
        // 將技能加入玩家擁有的技能列表
        GameDataManager.HasSkillIDs.Add(skillData.skillID);
        
        Debug.Log($"購買技能: {skillData.skillName}");
    }
}
