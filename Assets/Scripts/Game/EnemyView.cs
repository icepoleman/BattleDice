using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyView : CharacterView
{
    private Image img_character;
    private Transform skillBoxs;
    public override void Init()
    {
        img_character = gameObject.transform.Find("characterImage").GetComponent<Image>();
        skillBoxs = gameObject.transform.Find("skillBoxs");
        base.Init();
    }
    public void SetEnemySprite(Sprite enemySprite)
    {
        img_character.sprite = enemySprite;
        img_character.SetNativeSize();
        PlayAnim("idle");
    }
    public void BornSkillCards(GameObject cardPrefab, List<ISkillData> skills)
    {
        foreach (ISkillData skill in skills)
        {
            GameObject skillCard = Instantiate(cardPrefab, skillBoxs);
            littleSkillCard skillCardView = skillCard.GetComponent<littleSkillCard>();
            skillCardView.SetData(skill);
        }
    }
}
