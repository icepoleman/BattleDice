using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GameUiView : MonoBehaviour
{
    const int IdleSpriteIndex = 0;
    const int HurtSpriteIndex = 1;
    const int AttackSpriteIndex = 2;
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
    [SerializeField] Animator anim;
    [Header("攻擊表演相關")]
    [SerializeField] Image img_player;
    [SerializeField] Image img_enemy;
    [SerializeField] Image img_spSkill_player;
   // [SerializeField] Image img_spSkill_enemy;
    [SerializeField] Animator spSkillAnim_player;
  //  [SerializeField] Animator spSkillAnim_enemy;
    Transform trans_enemyPos;
    Transform trans_playerPos;
    Sprite[] playerSprites;
    Sprite[] enemySprites;

    GameObject prefab_manaDice;

    GameObject prefab_buffVFX, prefab_deBuffVFX, prefab_bloodVfx, prefab_buffCard;

    [SerializeField] Button btn_set;
    async void Start()
    {
        trans_enemyPos = img_enemy.transform.parent;
        trans_playerPos = img_player.transform.parent;
        prefab_manaDice = await AddressableManager.LoadAssetAsync<GameObject>(ABconfig.GAME_PREFABS + "manaDice.prefab");
        prefab_buffVFX = await AddressableManager.LoadAssetAsync<GameObject>(ABconfig.GAME_VFX + "vfx_buff.prefab");
        prefab_deBuffVFX = await AddressableManager.LoadAssetAsync<GameObject>(ABconfig.GAME_VFX + "vfx_debuff.prefab");
        prefab_bloodVfx = await AddressableManager.LoadAssetAsync<GameObject>(ABconfig.GAME_VFX + "vfx_blood.prefab");
        prefab_buffCard = await AddressableManager.LoadAssetAsync<GameObject>(ABconfig.GAME_PREFABS + "buffCard.prefab");
        btn_set.onClick.AddListener(async () =>
        {
            await UIManager.ShowCommonPanel("SetPanel");
        });
    }
    public async Task UpdatePlayerInfo(PlayerData playerData, EnemyData enemyData)
    {
        enemySprites = await LoadEnemySprites(enemyData.enemyId);
        img_enemy.sprite = enemySprites[IdleSpriteIndex];
        img_enemy.SetNativeSize();
        //img_spSkill_enemy.sprite = enemySprites[AttackSpriteIndex];

        img_enemy.transform.localPosition = new Vector3(img_enemy.transform.localPosition.x, GetEnemyYPosition(enemyData.enemyId), 1);
        playerSprites = await LoadPlayerSprites();
        img_player.sprite = playerSprites[IdleSpriteIndex];
        img_spSkill_player.sprite = playerSprites[AttackSpriteIndex];
        img_player.SetNativeSize();

        UpdateBlood(true, playerData.currentBlood, playerData.maxBlood);
        UpdateBlood(false, enemyData.currentBlood, enemyData.maxBlood);
        UpdateNames(LanguageManager.GetText("T_GirlName"), enemyData.enemyName);
        await BornEnemySkillCards(enemyData.skillData);
        anim.Play("enterPlace");
    }
    private async Task<Sprite[]> LoadEnemySprites(int enemyId)
    {
        Sprite defaultEnemy = await AddressableManager.LoadAssetAsync<Sprite>(ABconfig.GAME_SPRITES + "enemy0_0.png");
        return await LoadSpriteSet("enemy" + enemyId, defaultEnemy);
    }
    private Task<Sprite[]> LoadPlayerSprites()
    {
        return LoadSpriteSet("player");
    }
    private async Task<Sprite[]> LoadSpriteSet(string spritePrefix, Sprite fallbackSprite = null)
    {
        Sprite idleSprite = await LoadSprite(spritePrefix, IdleSpriteIndex, fallbackSprite);
        Sprite hurtSprite = await LoadSprite(spritePrefix, HurtSpriteIndex, idleSprite);
        Sprite attackSprite = await LoadSprite(spritePrefix, AttackSpriteIndex, idleSprite);
        return new[] { idleSprite, hurtSprite, attackSprite };
    }
    private async Task<Sprite> LoadSprite(string spritePrefix, int spriteIndex, Sprite fallbackSprite = null)
    {
        return await AddressableManager.LoadAssetAsync<Sprite>(ABconfig.GAME_SPRITES + spritePrefix + "_" + spriteIndex + ".png") ?? fallbackSprite;
    }
    private float GetEnemyYPosition(int enemyId)
    {
        // 根據敵人ID返回不同的Y位置，這裡只是示例，你可以根據實際需求調整
        switch (enemyId)
        {
            case 10: return 228f;
            default: return 158f;
        }
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
            UpdateBloodDisplay(slider_playerBlood, text_playerBlood, currentBlood, maxBlood);
        }
        else
        {
            UpdateBloodDisplay(slider_enemyBlood, text_enemyBlood, currentBlood, maxBlood);
        }
    }
    void UpdateBloodDisplay(Slider bloodSlider, Text bloodText, float currentBlood, float maxBlood)
    {
        bloodSlider.value = maxBlood <= 0 ? 0 : currentBlood / maxBlood;
        bloodText.text = $"{currentBlood}/{maxBlood}";
    }
    public void UpdateBuffs(bool isPlayer, IBuffData[] buffs)
    {
        Transform buffParent = isPlayer ? trans_playerBuffParent : trans_enemyBuffParent;
        ClearBuffs(buffParent);
        if (buffs == null)
        {
            return;
        }
        foreach (var buff in buffs)
        {
            GameObject buffIcon = Instantiate(prefab_buffCard, buffParent);
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
    public async Task BornEnemySkillCards(List<ISkillData> skills)
    {
        ClearEnemySkillCards();
        GameObject skillCardPrefab = await AddressableManager.LoadAssetAsync<GameObject>(ABconfig.GAME_PREFABS + "littleSkillItem" + ".prefab");
        foreach (ISkillData skill in skills)
        {
            GameObject skillCard = Instantiate(skillCardPrefab, trans_enemySkillBoxs);
            littleSkillCard skillCardView = skillCard.GetComponent<littleSkillCard>();
            skillCardView.SetData(skill);
            enemy_skillCardViews.Add(skillCardView);
        }
    }
    void ClearEnemySkillCards()
    {
        foreach (littleSkillCard skillCardView in enemy_skillCardViews)
        {
            if (skillCardView != null)
            {
                Destroy(skillCardView.gameObject);
            }
        }
        enemy_skillCardViews.Clear();
    }
    public void UpdateEnemySkillCards(List<ISkillData> skillsInUse)
    {
        foreach (littleSkillCard skillCardView in enemy_skillCardViews)
        {
            bool isInUse = skillsInUse.Exists(skill => skill.skillID == skillCardView.GetSkillID());
            skillCardView.SkillSwitch(isInUse);
            /*if (isInUse && SkillDatabase.SpEnemySkillIDs.Contains(skillCardView.GetSkillID()))
            {
                spSkillAnim_enemy.Play("spSkill");
                Debug.LogError("觸發怪物技能特效，技能ID：" + skillCardView.GetSkillID());
            }*/
        }
    }
    public void ClearUsedEnemySkillCards()
    {
        foreach (littleSkillCard skillCardView in enemy_skillCardViews)
        {
            skillCardView.SkillSwitch(false);
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
        ClearDiceBox();

        await Task.Delay(500);// 等待0.5秒後開始顯示動畫

        //生成骰子物件在diceBox下
        for (int i = 0; i < rollResults.Count; i++)
        {
            BurnDice(rollResults[i], isPlayer);
            await Task.Delay(100); // 每個骰子間隔0.3秒
        }
        await Task.Delay(500);
    }
    public void ClearDiceBox()
    {
        foreach (Transform child in trans_playerDiceBox.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in trans_enemyDiceBox.transform)
        {
            Destroy(child.gameObject);
        }
    }
    public void BurnDice(int sideNum, bool isPlayer)//之後改用ab
    {
        Transform diceBox = isPlayer ? trans_playerDiceBox : trans_enemyDiceBox;
        GameObject dice = Instantiate(prefab_manaDice, diceBox);
        dice.transform.localScale = Vector3.one;
        dice.transform.localPosition = Vector3.zero;
        manaDice diceView = dice.GetComponent<manaDice>();
        diceView.SetDiceFace(sideNum);
    }
    //生成飛行文字
    public async void CreateFlyBloodText(int damage, bool isPlayer, bool isbig)
    {
        float xPos = isbig ? 370 : 675;
        if (isPlayer) xPos = -xPos;
        GameObject damageText = await AddressableManager.LoadAssetAsync<GameObject>(ABconfig.GAME_PREFABS + "flyText.prefab");
        GameObject _instFlyText = Instantiate(damageText);
        RectTransform _instRect = _instFlyText.GetComponent<RectTransform>();
        _instFlyText.transform.SetParent(transform);
        _instRect.localPosition = new Vector3(xPos, 400, 0); // 往上偏移一點位置
        _instFlyText.transform.localScale = Vector3.one;

        Text textMesh = _instFlyText.GetComponent<Text>();
        if (damage > 0)
            textMesh.text = "-" + damage.ToString();
        else
            textMesh.text = "+" + (-damage).ToString();

        // 往上飛行
        _instRect.DOMoveY(_instRect.position.y + 1f, 1f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            Destroy(_instFlyText);
        });
    }
    public async void PlayFightAnim(bool isPlayer)
    {
        AudioManager.Instance.PlaySFX("Sound_Hit");
        // 被攻擊的角色移到最底層顯示
        if (isPlayer)
            trans_enemyPos.SetAsFirstSibling();
        else
            trans_playerPos.SetAsFirstSibling();

        string _animName = isPlayer ? "playerAtk" : "enemyAtk";
        anim.Play(_animName);

        img_player.sprite = isPlayer ? playerSprites[AttackSpriteIndex] : playerSprites[HurtSpriteIndex];
        img_enemy.sprite = isPlayer ? enemySprites[HurtSpriteIndex] : enemySprites[AttackSpriteIndex];
        /*await Task.Delay(1000);
        img_player.sprite = playerSprites[IdleSpriteIndex];
        img_enemy.sprite = enemySprites[IdleSpriteIndex];*/
    }
    public void FightEndAnim(bool playerIsDead, bool enemyIsDead)
    {
        if (playerIsDead)
        {
            img_player.DOKill();
            img_player.sprite = playerSprites[HurtSpriteIndex];
            img_player.DOFade(0f, 2f).SetEase(Ease.OutQuad);
            return;
        }

        if (enemyIsDead)
        {
            img_enemy.DOKill();
            img_enemy.sprite = enemySprites[HurtSpriteIndex];
            img_enemy.DOFade(0f, 2f).SetEase(Ease.OutQuad);
            return;
        }

        img_player.DOKill();
        img_enemy.DOKill();

        Color playerColor = img_player.color;
        img_player.color = new Color(playerColor.r, playerColor.g, playerColor.b, 1f);
        Color enemyColor = img_enemy.color;
        img_enemy.color = new Color(enemyColor.r, enemyColor.g, enemyColor.b, 1f);

        img_player.sprite = playerSprites[IdleSpriteIndex];
        img_enemy.sprite = enemySprites[IdleSpriteIndex];
    }

    public void PlayBuffVfx(bool isPlayer)
    {
        GameObject buffVFX = Instantiate(prefab_buffVFX);
        Transform targetTrans = isPlayer ? img_player.transform : img_enemy.transform;
        buffVFX.transform.position = targetTrans.position;
        Destroy(buffVFX, 2f);

        // 往上跳一下
        Vector3 originalPos = targetTrans.localPosition;
        Sequence seq = DOTween.Sequence();
        seq.Append(targetTrans.DOLocalMoveY(originalPos.y + 30f, 0.1f).SetEase(Ease.OutQuad));
        seq.Append(targetTrans.DOLocalMoveY(originalPos.y, 0.1f).SetEase(Ease.InQuad));
    }
    public void PlayDebuffVfx(bool isPlayer)
    {
        GameObject debuffVFX = Instantiate(prefab_deBuffVFX);
        Transform targetTrans = isPlayer ? img_player.transform : img_enemy.transform;
        debuffVFX.transform.position = targetTrans.position;
        Destroy(debuffVFX, 2f);

        // 俐落的左右晃動
        Vector3 originalPos = targetTrans.localPosition;
        Sequence seq = DOTween.Sequence();
        seq.Append(targetTrans.DOLocalMoveX(originalPos.x + 20f, 0.05f).SetEase(Ease.OutSine));
        seq.Append(targetTrans.DOLocalMoveX(originalPos.x - 20f, 0.05f).SetEase(Ease.InOutSine));
        seq.Append(targetTrans.DOLocalMoveX(originalPos.x, 0.05f).SetEase(Ease.OutSine));
    }
    public async void PlayBloodVfx(bool isPlayer)
    {
        if (isPlayer)
            img_player.sprite = playerSprites[HurtSpriteIndex];
        else
            img_enemy.sprite = enemySprites[HurtSpriteIndex];
        GameObject bloodVFX = Instantiate(prefab_bloodVfx);
        bloodVFX.transform.position = isPlayer ? img_player.transform.position : img_enemy.transform.position;
        float yRotation = isPlayer ? 0 : 180; // 根據角色方向決定是否翻轉特效
        bloodVFX.transform.rotation = Quaternion.Euler(0, yRotation, 0); // 根據角色方向旋轉血液特效
        Destroy(bloodVFX, 2f);
        await Task.Delay(1000);
        img_player.sprite = playerSprites[IdleSpriteIndex];
        img_enemy.sprite = enemySprites[IdleSpriteIndex];
    }
}
