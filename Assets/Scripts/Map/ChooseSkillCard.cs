using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
public class ChooseSkillCard : MonoBehaviour
{
    public int SkillID => skillData.skillID;
    [SerializeField] Button btn_card;
    [SerializeField] Text text_skillTitle;
    [SerializeField] Text text_skillCondition_title;
    [SerializeField] Text text_skillCondition;
    [SerializeField] Text text_skillEffect;
    [SerializeField] Text text_skillEffect_title;
    [SerializeField] Image img_diceNum;
    SkillConfigData skillData;

    public async void SetData(SkillConfigData _skillData)
    {
        btn_card.onClick.AddListener(() => 
        {
             EventCenter.Dispatch(MapEvent.EVENT_UNCHOOSE_SKILL, skillData.skillID);
             Destroy(gameObject);
        });
        text_skillCondition_title.text = LanguageManager.GetText("T_skill_condition_title");
        text_skillEffect_title.text = LanguageManager.GetText("T_skill_effect_title");
        skillData = _skillData;
        text_skillTitle.text = skillData.skillName;
        text_skillCondition.text = skillData.conditionText;
        text_skillEffect.text = skillData.effectText;
        Debug.Log(skillData.GetNeedDiceNum());
        img_diceNum.sprite = await AddressableManager.LoadAssetAsync<Sprite>(ABconfig.GAME_SPRITES + "dice_" + skillData.GetNeedDiceNum() + ".png");
    }
    void OnEnable()
    {
        EventCenter.AddListener(MapEvent.EVENT_UNCHOOSE_SKILL, OnUnchooseSkill);
    }
    void OnDestroy()
    {
        EventCenter.RemoveListener(MapEvent.EVENT_UNCHOOSE_SKILL, OnUnchooseSkill);
    }
    void OnUnchooseSkill(object[] args)
    {
        int skillID = (int)args[0];
        if (skillID == this.SkillID)
        {
            Destroy(gameObject);
        }
    }
}
