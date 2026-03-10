using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopPanel : MonoBehaviour
{
    string shopTag = "Shop";   // 商店技能的Tag
    [SerializeField] GameObject shopSkillCardPrefab;
    [SerializeField] Transform skillCardParent;
    [SerializeField] Text text_gold;
    [SerializeField] Button btn_close;
    
    List<ShopSkillCard> skillCards = new List<ShopSkillCard>();
    
    public void SetUp(string _shopTag)
    {
        shopTag = _shopTag;
        btn_close.onClick.AddListener(CloseShop);
        RefreshGoldDisplay();
        GenerateShopItems();
    }
    
    void RefreshGoldDisplay()
    {
        text_gold.text = LanguageManager.GetFormat("T_Gold", GameDataManager.Gold);
    }
    
    void GenerateShopItems()
    {
        // 清空現有卡片
        ClearChildren(skillCardParent);
        skillCards.Clear();
        
        // 取得商店技能列表
        List<SkillConfigData> shopSkills = SkillDatabase.GetSkillsByTag(shopTag);
        
        // 取得玩家已擁有的技能ID
        List<int> ownedSkillIDs = GameDataManager.HasSkillIDs;
        
        foreach (var skillConfig in shopSkills)
        {
            bool isOwned = ownedSkillIDs.Contains(skillConfig.skillID);
            CreateSkillCard(skillConfig, isOwned);
        }
    }
    
    void CreateSkillCard(SkillConfigData config, bool isOwned)
    {
        GameObject cardObj = Instantiate(shopSkillCardPrefab, skillCardParent);
        ShopSkillCard card = cardObj.GetComponent<ShopSkillCard>();
        card.SetData(config, isOwned, OnSkillCardClicked);
        skillCards.Add(card);
    }
    
    async void OnSkillCardClicked(ShopSkillCard card)
    {
        if (card.IsOwned)
        {
            await UIManager.ShowHintBubble(LanguageManager.GetText("T_Shop_AlreadyOwned"));
            return;
        }
        
        int price = card.Price;
        
        // 檢查金幣是否足夠
        if (GameDataManager.Gold < price)
        {
            await UIManager.ShowHintBubble(LanguageManager.GetText("T_Shop_NotEnoughGold"));
            return;
        }
        
        // 顯示購買確認
        string skillName = SkillDatabase.GetSkillConfig(card.SkillID).skillName;
        await UIManager.ShowConfirmPanel(
            LanguageManager.GetFormat("T_Shop_ConfirmBuy", skillName, price),
            () => PurchaseSkill(card)
        );
    }
    
    void PurchaseSkill(ShopSkillCard card)
    {
        int skillID = card.SkillID;
        int price = card.Price;
        
        // 扣除金幣
        GameDataManager.Gold -= price;
        
        // 添加技能到玩家擁有列表
        GameDataManager.HasSkillIDs.Add(skillID);
        
        // 自動存檔
        SaveManager.AutoSave();
        
        // 刷新顯示
        RefreshGoldDisplay();
        GenerateShopItems();
    }
    
    void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }
    
    void CloseShop()
    {
        Destroy(this.gameObject);
    }
}
