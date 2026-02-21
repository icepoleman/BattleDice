using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PortraitStageManager : MonoBehaviour
{
    float spacing = 350f; // 角色間距
    //字典 名稱 對應 RoleView
    Dictionary<string, RoleView> roleViews = new Dictionary<string, RoleView>();

    public async Task SetCharacter(string characterName, Sprite _newPortrait, string _animationName, string portraitPos)
    {
        string oldPosition = null;

        if (roleViews.ContainsKey(characterName))
        {
            //已經有這個角色 - 記錄原位置
            RoleView roleView = roleViews[characterName];
            oldPosition = roleView.GetPortraitPos();
            roleView.ShowCharacter(_newPortrait, _animationName, portraitPos);
        }
        else
        {
            //沒有這個角色 創建一個新的
            GameObject roleViewObj = await AddressableManager.LoadAssetAsync<GameObject>(ABconfig.AVG_PREFABS + "RolePrefab" + ".prefab");
            GameObject roleObj = Instantiate(roleViewObj,transform);
            roleObj.name = characterName;
            roleObj.transform.localScale = Vector3.one;
            roleObj.transform.localPosition = Vector3.zero;
            RoleView roleView = roleObj.GetComponent<RoleView>();
            roleViews[characterName] = roleView;

            if (portraitPos == "")
                portraitPos = "Center";
            // 先設置角色信息，但不執行位置動畫
            roleView.ShowCharacter(_newPortrait, _animationName, portraitPos);

            // 立即計算並設置正確的初始位置（無動畫）
            Vector2 initialPos = CalculateInitialPosition(characterName, portraitPos);
            roleView.SetPosition(initialPos);

            Debug.Log($"新角色 {characterName} 初始位置設置為: {initialPos}");
        }

        // 如果角色位置改變了，需要更新原位置和新位置
        if (oldPosition != null && oldPosition != portraitPos)
        {
            // 更新原位置的角色排列
            UpdateStagePos(oldPosition);
            Debug.Log($"角色 {characterName} 從 {oldPosition} 移動到 {portraitPos}");
        }

        if (_animationName == "hide")
        {
            // 移除角色
            HideCharacter(characterName);
        }
        else
        {
            UpdateStagePos(portraitPos);

            // 講話的角色移到最上層並高亮，其他角色變暗
            SetSpeakingCharacter(characterName);
        }
    }

    // 設置講話中的角色（高亮並置頂）
    void SetSpeakingCharacter(string characterName)
    {
        foreach (var kv in roleViews)
        {
            if (kv.Key == characterName)
            {
                // 講話的角色：移到最上層 + 高亮
                kv.Value.transform.SetAsLastSibling();
                kv.Value.SetHighlight(true);
            }
            else
            {
                // 其他角色：變暗
                kv.Value.SetHighlight(false);
            }
        }
    }
    // 隱藏角色
    public void HideCharacter(string characterName)
    {
        if (roleViews.ContainsKey(characterName))
        {
            RoleView roleView = roleViews[characterName];
            roleViews.Remove(characterName);

            // 更新原位置的角色排列
            UpdateStagePos(roleView.GetPortraitPos());
        }
    }

    void UpdateStagePos(string _targetPos)
    {
        // 獲取指定位置的所有角色
        List<RoleView> charactersAtPos = new List<RoleView>();

        foreach (var kv in roleViews)
        {
            if (kv.Value.GetPortraitPos() == _targetPos)
            {
                charactersAtPos.Add(kv.Value);
                Debug.Log($"找到角色在 {_targetPos}: {kv.Key}");
            }
        }

        // 根據角色數量重新計算位置
        int characterCount = charactersAtPos.Count;
        float baseX = GetBasePositionX(_targetPos);

        Debug.Log($"位置 {_targetPos} 有 {characterCount} 個角色，基準位置: {baseX}");

        if (characterCount == 1)
        {
            // 只有一個角色，放在基準位置
            charactersAtPos[0].MovePosition(new Vector2(baseX, charactersAtPos[0].transform.localPosition.y));
            Debug.Log($"單一角色移動到: {baseX}");
        }
        else if (characterCount > 1)
        {
            for (int i = 0; i < characterCount; i++)
            {
                float offsetX = CalculateOffsetX(i, characterCount, spacing);
                Vector2 targetPos = new Vector2(baseX + offsetX, charactersAtPos[i].transform.localPosition.y);
                charactersAtPos[i].MovePosition(targetPos);

                Debug.Log($"角色 {i} (總共{characterCount}個) 偏移: {offsetX}, 最終位置: {targetPos.x}");
            }
        }
    }

    // 獲取基準位置的 X 座標
    float GetBasePositionX(string position)
    {
        switch (position)
        {
            case "Left":
                return -600f;
            case "Center":
                return 0f;
            case "Right":
                return 600f;
            default:
                return 0f;
        }
    }
    // 獲取基準位置的 Y 座標(角色身高不同需要調整)
    float GetBasePositionY(string characterName)
    {
        switch (characterName)
        {
            case "WolfGirl":
                return -500;
            case "Witch":
                return -600;
            case "JailerGirl":
                return -432;
            default:
                return 0;
        }
    }
    // 計算角色的偏移量
    float CalculateOffsetX(int index, int totalCount, float spacing)
    {
        if (totalCount == 1)
            return 0f;

        // 計算總寬度
        float totalWidth = (totalCount - 1) * spacing;

        // 計算起始位置（讓角色們居中排列）
        float startOffset = -totalWidth / 2f;

        // 計算當前角色的偏移
        float result = startOffset + (index * spacing);

        Debug.Log($"計算偏移 - index:{index}, totalCount:{totalCount}, spacing:{spacing}");
        Debug.Log($"  totalWidth:{totalWidth}, startOffset:{startOffset}, result:{result}");

        return result;
    }

    // 計算角色初始位置（用於新創建的角色）
    Vector2 CalculateInitialPosition(string characterName, string portraitPos)
    {
        // 獲取該位置當前的角色數量（包括即將添加的這個）
        List<RoleView> charactersAtPos = new List<RoleView>();

        foreach (var kv in roleViews)
        {
            if (kv.Key != characterName && kv.Value.GetPortraitPos() == portraitPos)
            {
                charactersAtPos.Add(kv.Value);
            }
        }

        // 新角色是第 characterCount + 1 個
        int characterCount = charactersAtPos.Count + 1;
        int newCharacterIndex = charactersAtPos.Count; // 新角色的索引

        float baseX = GetBasePositionX(portraitPos);
        float baseY = GetBasePositionY(characterName);

        if (characterCount == 1)
        {
            return new Vector2(baseX, baseY);
        }
        else
        {
            float offsetX = CalculateOffsetX(newCharacterIndex, characterCount, spacing);
            return new Vector2(baseX + offsetX, baseY);
        }
    }
}
