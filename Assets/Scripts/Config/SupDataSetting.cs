using UnityEngine;
using System.Collections.Generic;

public class SupDataSetting
{
    //單例模式
    private static SupDataSetting instance;
    public static SupDataSetting Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new SupDataSetting();
            }
            return instance;
        }
    }
    public Dictionary<string, SupData> supDataDic;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void SetUp()
    {
        supDataDic = CSVReader.Instance.LoadSupporterCSV();
    }

    public SupData GetSupDataByID(string supID)
    {
        if (supDataDic != null && supDataDic.ContainsKey(supID))
        {
            return supDataDic[supID];
        }
        else
        {
            Debug.LogWarning($"SupData with ID {supID} not found.");
            return null;
        }
    }
    public bool IsSupUnlocked(string supID)
    {
        int nowChapter = MapConfig.MapChapter[GameDataManager.CurrentMap];//依照章節進度解鎖
        int unlockLimit = 0;
        switch(supID)
        {
            case "Witch":
                unlockLimit = 3;
                break;
            case "WolfGirl":
                unlockLimit = 4;
                break;
            case "Idol":
                unlockLimit = 5;
                break;
        }
        return nowChapter >= unlockLimit;
    }
}
