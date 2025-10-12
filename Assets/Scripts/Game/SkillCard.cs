using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillCard : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI skillInfoText;
    [SerializeField] GameObject obj_choose;
    [SerializeField] Button btn_choose;
    ISkillData skillData;
    public void SetData(ISkillData _skillData)
    {
        skillData = _skillData;
        skillInfoText.text = skillData.cardTitle;
        EventCenter.AddListener(GameEvent.EVENT_STOP_USE_DICE, StopUseDiceEvent);
        btn_choose.onClick.AddListener(() => { EventCenter.Dispatch(GameEvent.EVENT_SELECT_SKILL, skillData); });
        EventCenter.AddListener(GameEvent.EVENT_CONFIRM_SELECT_SKILL, SkillChoosenEvent);
    }
    void OnDisable()
    {
        EventCenter.RemoveListener(GameEvent.EVENT_STOP_USE_DICE, StopUseDiceEvent);
        EventCenter.RemoveListener(GameEvent.EVENT_CONFIRM_SELECT_SKILL, SkillChoosenEvent);
    }
    void SkillChoosenEvent(object[] args)
    {
        ISkillData chosenSkill = (ISkillData)args[0];
        obj_choose.SetActive(chosenSkill == skillData);
    }
    void StopUseDiceEvent(object[] args)
    {
        obj_choose.SetActive(false);
    }
}
