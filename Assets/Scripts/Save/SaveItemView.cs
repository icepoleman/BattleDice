using UnityEngine;
using UnityEngine.UI;

public class SaveItemView : MonoBehaviour
{
    [SerializeField] Text txt_saveNum;
    [SerializeField] Text txt_playerName;
    [SerializeField] Text txt_gear;
    [SerializeField] Text txt_gold;
    [SerializeField] Text txt_saveTime;
    [SerializeField] Text txt_null;
    public void SetData(SaveSlotInfo info, bool isAutoSave = false)
    {
        if (isAutoSave)
        {
            txt_saveNum.text = LanguageManager.GetText("T_Menu_Save_Slot_Auto");
        }
        else
        {
            txt_saveNum.text = LanguageManager.GetFormat("T_Menu_Save_Slot_Number", (info.slotIndex + 1).ToString());
        }
        
        if (info.isEmpty)
        {
            // 设置为空槽位的显示
            txt_null.text = LanguageManager.GetText("T_Menu_Save_Slot_Empty");
        }
        else
        {
            // 设置为已保存槽位的显示
            txt_null.text = "";
            txt_playerName.text = info.playerName;
            txt_gear.text = LanguageManager.GetFormat("T_Menu_Save_Slot_Gear", info.gear.ToString());
            txt_gold.text = LanguageManager.GetFormat("T_Menu_Save_Slot_Gold", info.gold.ToString());
            txt_saveTime.text =  LanguageManager.GetFormat("T_Menu_Save_Slot_Time", info.saveTime);
        }
    }
}
