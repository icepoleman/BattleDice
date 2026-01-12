using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class EnemyView : CharacterView
{
    private Image img_character;
    private Transform skillBoxs;
    private List<littleSkillCard> skillCardViews = new List<littleSkillCard>();
    List<GameObject> diceObjects = new List<GameObject>();
    public override void Init()
    {
        img_character = gameObject.transform.Find("characterImage").GetComponent<Image>();
        skillBoxs = gameObject.transform.Find("skillBoxs");
        base.Init();
    }
    public override void BurnDice(int sideNum)
    {
        base.BurnDice(sideNum);
        GetDiceObjects();
    }
    void GetDiceObjects()
    {
        diceObjects.Clear();
        
        foreach (Transform child in diceBox.transform)
        {
            diceObjects.Add(child.gameObject);
        }
    }
    public void DestroyTargetDice(List<int> targetDicesIndex)
    {
        if (diceObjects.Count == 0 || targetDicesIndex.Count == 0) return;

        // 從大索引往小索引排序，避免刪除時索引位移問題
        List<int> sortedIndexes = new List<int>(targetDicesIndex);
        sortedIndexes.Sort((a, b) => b.CompareTo(a)); // 降序排列
        
        foreach (int index in sortedIndexes)
        {
            if (index < 0 || index >= diceObjects.Count) continue;
            
            GameObject diceToDestroy = diceObjects[index];
            
            // 先從列表中移除（避免動畫期間被重複操作）
            diceObjects.RemoveAt(index);
            
            // 播放銷毀動畫
            diceToDestroy.transform.DOScale(Vector3.zero, 0.5f).OnComplete(() =>
            {
                Destroy(diceToDestroy);
            });
        }
    }
    public void SetEnemySprite(Sprite enemySprite)
    {
        img_character.sprite = enemySprite;
        img_character.SetNativeSize();
        PlayAnim("idle");
    }
    public async void BornSkillCards(List<ISkillData> skills)
    {
        GameObject skillCardPrefab = await AddressableManager.LoadAssetAsync<GameObject>("enemySkillItem");
        foreach (ISkillData skill in skills)
        {
            GameObject skillCard = Instantiate(skillCardPrefab, skillBoxs);
            littleSkillCard skillCardView = skillCard.GetComponent<littleSkillCard>();
            skillCardView.skillID = skill.skillID;
            skillCardView.SetData(skill);
            skillCardViews.Add(skillCardView);
        }
    }
    public void UpdateSkillCards(List<ISkillData> skillsInUse)
    {
        foreach (littleSkillCard skillCardView in skillCardViews)
        {
            bool isInUse = skillsInUse.Exists(skill => skill.skillID == skillCardView.skillID);
            skillCardView.SkillSwitch(isInUse);
        }
    }
}
