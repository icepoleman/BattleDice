using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GameUiView : MonoBehaviour
{
    [Header("血量")]
    [SerializeField] Slider slider_playerBlood;
    [SerializeField] Text text_playerBlood;
    [SerializeField] Slider slider_enemyBlood;
    [SerializeField] Text text_enemyBlood;
    [Header("Buffs")]
    [SerializeField] Transform trans_playerBuffParent;
    [SerializeField] Transform trans_enemyBuffParent;
    [Header("名稱顯示")]
    [SerializeField] Text text_playerName;
    [SerializeField] Text text_enemyName;

    [Header("戰鬥相關")]
    [SerializeField] Transform trans_enemySkillBoxs;
    List<littleSkillCard> enemy_skillCardViews = new List<littleSkillCard>();
    [SerializeField] Transform trans_enemyDiceBox;
    [SerializeField] Transform trans_playerDiceBox;
    [SerializeField] Animator fightAnim;

    Sprite[] diceSprites;
    GameObject prefab_manaDice;

    async void Start()
    {
        var spriteList = await AddressableManager.LoadLabelAsync<Sprite>("Dice");
        // 依照名稱排序 (dice_0, dice_1, dice_2...)
        spriteList.Sort((a, b) =>
        {
            int aNum = int.Parse(a.name.Replace("dice_", ""));
            int bNum = int.Parse(b.name.Replace("dice_", ""));
            return aNum.CompareTo(bNum);
        });
        diceSprites = spriteList.ToArray();
        prefab_manaDice = await AddressableManager.LoadAssetAsync<GameObject>(ABconfig.GAME_PREFABS + "manaDice.prefab");
    }

    public void UpdateNames(string playerName, string enemyName)
    {
        text_playerName.text = playerName;
        text_enemyName.text = enemyName;
    }
    public void UpdateBlood(bool isPlayer, float currentBlood, float maxBlood)
    {
        if (isPlayer)
        {
            slider_playerBlood.value = currentBlood / maxBlood;
            text_playerBlood.text = $"{currentBlood}/{maxBlood}";
        }
        else
        {
            slider_enemyBlood.value = currentBlood / maxBlood;
            text_enemyBlood.text = $"{currentBlood}/{maxBlood}";
        }
    }
    public async void UpdateBuffs(bool isPlayer, IBuffData[] buffs)
    {
        Transform buffParent = isPlayer ? trans_playerBuffParent : trans_enemyBuffParent;
        ClearBuffs(buffParent);
        if (buffs == null)
        {
            return;
        }
        foreach (var buff in buffs)
        {
            GameObject buffIcon = Instantiate(AddressableManager.GetLoadedAsset<GameObject>(ABconfig.GAME_PREFABS + "buffCard" + ".prefab"));
            buffIcon.transform.SetParent(buffParent);
            buffIcon.transform.localScale = Vector3.one;
            buffIcon.transform.localPosition = Vector3.zero;
            buffIcon.GetComponent<BuffCard>().SetBuffInfo(buff);
        }
    }
    void ClearBuffs(Transform buffParent)
    {
        foreach (Transform child in buffParent)
        {
            Destroy(child.gameObject);
        }
    }
    //生成飛行文字
    public async Task CreateFlyText(string _flyTxt)
    {
        GameObject damageText = await AddressableManager.LoadAssetAsync<GameObject>(ABconfig.GAME_PREFABS + "flyText.prefab");
        GameObject _instFlyText = Instantiate(damageText);
        _instFlyText.transform.SetParent(transform);
        _instFlyText.transform.position = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);
        _instFlyText.transform.localScale = new Vector3(1, 1, 1);

        Text textMesh = _instFlyText.GetComponent<Text>();
        textMesh.text = _flyTxt;
        textMesh.fontSize = 28;
        // 添加飛行動畫
        //dottween動畫
        _instFlyText.GetComponent<RectTransform>().DOMoveY(_instFlyText.transform.position.y + 0.5F, 1).SetEase(Ease.Linear).OnComplete(() =>
        {
            Destroy(_instFlyText);
        });
    }
    public async void BornEnemySkillCards(List<ISkillData> skills)
    {
        GameObject skillCardPrefab = await AddressableManager.LoadAssetAsync<GameObject>(ABconfig.GAME_PREFABS + "enemySkillItem" + ".prefab");
        foreach (ISkillData skill in skills)
        {
            GameObject skillCard = Instantiate(skillCardPrefab, trans_enemySkillBoxs);
            littleSkillCard skillCardView = skillCard.GetComponent<littleSkillCard>();
            skillCardView.skillID = skill.skillID;
            skillCardView.SetData(skill);
            enemy_skillCardViews.Add(skillCardView);
        }
    }
    public void UpdateEnemySkillCards(List<ISkillData> skillsInUse)
    {
        foreach (littleSkillCard skillCardView in enemy_skillCardViews)
        {
            bool isInUse = skillsInUse.Exists(skill => skill.skillID == skillCardView.skillID);
            skillCardView.SkillSwitch(isInUse);
        }
    }

    public async Task ShowDice(List<int> rollResults, bool isPlayer)
    {
        if (rollResults == null || rollResults.Count == 0)
        {
            Debug.LogWarning("骰子結果為空，無法顯示動畫");
            return;
        }
        Debug.Log("顯示擲骰子動畫");
        ClearDiceBox(isPlayer);

        await Task.Delay(500);// 等待0.5秒後開始顯示動畫

        //生成骰子物件在diceBox下
        for (int i = 0; i < rollResults.Count; i++)
        {
            BurnDice(rollResults[i], isPlayer);
            await Task.Delay(100); // 每個骰子間隔0.3秒
        }
        await Task.Delay(500);
    }
    public void ClearDiceBox(bool isPlayer)
    {
        Transform diceBox = isPlayer ? trans_playerDiceBox : trans_enemyDiceBox;
        foreach (Transform child in diceBox.transform)
        {
            Destroy(child.gameObject);
        }
    }
    public void BurnDice(int sideNum, bool isPlayer)//之後改用ab
    {
        Transform diceBox = isPlayer ? trans_playerDiceBox : trans_enemyDiceBox;
        GameObject dice = Instantiate(prefab_manaDice);
        dice.transform.SetParent(diceBox.transform);
        dice.transform.localScale = Vector3.one;
        manaDice diceView = dice.GetComponent<manaDice>();
        diceView.SetDiceFace(sideNum, diceSprites[sideNum]);
    }
    public void ChooseDice(int[] sideNum, bool isPlayer)
    {
       
    }
    public void PlayFightAnim(string _animName)
    {
        fightAnim.Play(_animName);
    }
}
