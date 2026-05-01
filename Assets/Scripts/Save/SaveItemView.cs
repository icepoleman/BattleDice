using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveItemView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI txt_saveTitle;
    [SerializeField] TextMeshProUGUI txt_playerName;
    [SerializeField] TextMeshProUGUI txt_gear;
    [SerializeField] TextMeshProUGUI txt_gold;
    [SerializeField] TextMeshProUGUI txt_saveTime;
    [SerializeField] TextMeshProUGUI txt_stageName;
    public void SetData(SaveSlotInfo info, bool isAutoSave = false)
    {
        if (isAutoSave)
        {
            txt_saveTitle.text = LanguageManager.GetText("T_Menu_Save_Slot_Auto");
        }
        else
        {
            txt_saveTitle.text = LanguageManager.GetFormat("T_Menu_Save_Slot_Number", (info.slotIndex + 1).ToString());
        }
        
        if (info.isEmpty)
        {
            // 设置为空槽位的显示
            //txt_null.text = LanguageManager.GetText("T_Menu_Save_Slot_Empty");
        }
        else
        {
            txt_playerName.text = info.playerName;
            txt_stageName.text = MapConfig.StageNames[info.currentMap];
            txt_gear.text = LanguageManager.GetFormat("T_Menu_Save_Slot_Gear", info.gear.ToString());
            txt_gold.text = LanguageManager.GetFormat("T_Menu_Save_Slot_Gold", info.gold.ToString());
            txt_saveTime.text =  LanguageManager.GetFormat("T_Menu_Save_Slot_Time", info.saveTime);
        }
    }
}
