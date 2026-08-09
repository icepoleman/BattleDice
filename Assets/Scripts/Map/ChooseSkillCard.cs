using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
public class ChooseSkillCard : MonoBehaviour
{
    public int SkillID => skillData.skillID;
    [SerializeField] Button btn_card;
    [SerializeField] Text text_skillTitle;
    [SerializeField] Image img_skillIcon;
    SkillConfigData skillData;

    public async void SetData(SkillConfigData _skillData)
    {
        btn_card.onClick.AddListener(() => 
        {
             EventCenter.Dispatch(MapEvent.EVENT_UNCHOOSE_SKILL, skillData.skillID);
             Destroy(gameObject);
        });
        skillData = _skillData;
        text_skillTitle.text = skillData.skillName;
        img_skillIcon.sprite = AtlasLoader.Instance.GetSkillSprite(skillData.iconPath);
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
