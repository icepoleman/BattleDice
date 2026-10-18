using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;

public class TreasureBoxGame : MonoBehaviour
{
    Dictionary<int, List<TreasureBoxReward>> levelRewardDict = new Dictionary<int, List<TreasureBoxReward>>();
    [SerializeField] private Button btn_up;
    [SerializeField] private Button btn_down;
    [SerializeField] private GameObject[] lastPointList;

    [Header("獎勵")]
    [SerializeField] private takaraItem[] takaraItems;
    [SerializeField] private Button btn_getReward;

    [Header("玩家資源")]
    [SerializeField] private TextMeshProUGUI txt_gold;
    [SerializeField] private TextMeshProUGUI txt_gear;

    [Header("LevelKey")]
    [SerializeField] private RectTransform trans_levelKey;
    [SerializeField] private Transform backupKeyRoot;
    [SerializeField] private GameObject backupKeyPrefab;
    [SerializeField] private ParticleSystem brokenKeyParticle;
    [Header("Anim")]
    [SerializeField] private Animator anim_takara;
    private int backupKey = 0;
    private Vector3[] levelKeyPositions = new Vector3[5]
    {
        new Vector3(-343f,240f, 0f),
        new Vector3(-40f, 240f, 0f),
        new Vector3(260f, 240f, 0f),
        new Vector3(560f, 240f, 0f),
        new Vector3(860f, 240f, 0f)
    };

    [Header("骰子")]
    [SerializeField] private Image[] dices;
    [SerializeField] private float diceSpinForce = 18f;
    [SerializeField] private float diceDirectionForce = 2f;
    private List<Rigidbody2D> diceRigidbodies = new List<Rigidbody2D>();
    private List<Sprite> diceSprites = new List<Sprite>();

    private int lastPoint;
    private bool isRolling = false;
    private int currentLevel = 1;
    private Vector3[] originalItemScales;



    bool isDone;

    private void RefreshLastPointDisplay()
    {
        if (lastPointList == null || lastPointList.Length == 0)
            return;

        int targetIndex = Mathf.Clamp(lastPoint - 1, 0, lastPointList.Length - 1);

        for (int i = 0; i < lastPointList.Length; i++)
        {
            if (lastPointList[i] != null)
            {
                lastPointList[i].SetActive(i == targetIndex);
            }
        }
    }

    private void RandomizeLastPoint()
    {
        int pointCount = lastPointList == null ? 0 : lastPointList.Length;
        lastPoint = pointCount > 0 ? Random.Range(1, pointCount + 1) : 1;
    }

    private void SetInitialRollPoint()
    {
        int rollResult = 0;
        for (int i = 0; i < dices.Length; i++)
        {
            if (diceSprites == null || diceSprites.Count == 0)
                continue;

            int diceValue = Random.Range(1, 7);
            rollResult += diceValue;
            dices[i].sprite = diceSprites[diceValue];
        }

        lastPoint = rollResult;
        RefreshLastPointDisplay();
    }

    public void ResetGame()
    {
        isDone = false;
        currentLevel = 1;
        SetInitialRollPoint();
        SetRewardItemsForCurrentLevel();
        ResetRewardItemScales();
        RefreshRewardItemScale();
        UpdateLevelKeyPosition();
        btn_getReward.interactable = true;
    }
    void Start()
    {
        SceneLoader.HideLoadingScreen();
        GameDataManager.BackUpKey = 1;//增加備用鑰匙數量
        UpdateBackupKeyDisplay();
        diceSprites = AtlasLoader.Instance.GetAllDiceSprites();
        for (int i = 0; i < dices.Length; i++)
        {
            diceRigidbodies.Add(dices[i].GetComponent<Rigidbody2D>());
        }
        SetInitialRollPoint();
        btn_up.onClick.AddListener(() => RollAndJudge(true));
        btn_down.onClick.AddListener(() => RollAndJudge(false));
        btn_getReward.onClick.AddListener(ClaimReward);
        SetLevelRewardDict();
        currentLevel = 1;
        SetRewardItemsForCurrentLevel();
        CaptureOriginalItemScales();
        RefreshRewardItemScale();
        UpdateLevelKeyPosition();
        txt_gold.text = GameDataManager.Gold.ToString();
        txt_gear.text = GameDataManager.Gear.ToString();
    }
    void UpdateBackupKeyDisplay()
    {
        brokenKeyParticle.Play();
        // 清除現有的備用鑰匙顯示
        foreach (Transform child in backupKeyRoot)
        {
            Destroy(child.gameObject);
        }

        // 重新生成備用鑰匙顯示
        for (int i = 0; i < GameDataManager.BackUpKey; i++)
        {
            GameObject keyObj = Instantiate(backupKeyPrefab, backupKeyRoot);
            keyObj.SetActive(true);
        }
    }

    private void UpdateLevelKeyPosition()
    {
        if (trans_levelKey == null || levelKeyPositions == null || levelKeyPositions.Length == 0)
            return;

        int index = Mathf.Clamp(currentLevel - 1, 0, levelKeyPositions.Length - 1);
        trans_levelKey.DOAnchorPos(levelKeyPositions[index], 0.2f).SetEase(Ease.OutQuad);
    }

    private void CaptureOriginalItemScales()
    {
        if (takaraItems == null)
            return;

        originalItemScales = new Vector3[takaraItems.Length];
        for (int i = 0; i < takaraItems.Length; i++)
        {
            if (takaraItems[i] != null)
                originalItemScales[i] = takaraItems[i].transform.localScale;
            else
                originalItemScales[i] = Vector3.one;
        }
    }

    private void RefreshRewardItemScale()
    {
        if (takaraItems == null || originalItemScales == null)
            return;

        if (currentLevel > 0 && currentLevel <= takaraItems.Length && takaraItems[currentLevel - 1] != null)
        {
            Vector3 targetScale = currentLevel - 1 < originalItemScales.Length && originalItemScales[currentLevel - 1] != Vector3.zero
                ? originalItemScales[currentLevel - 1]
                : Vector3.one;

            takaraItems[currentLevel - 1].transform.DOScale(targetScale * 1.5f, 0.2f).SetEase(Ease.OutQuad);
        }
    }

    private void ResetRewardItemScales()
    {
        if (takaraItems == null || originalItemScales == null)
            return;

        for (int i = 0; i < takaraItems.Length; i++)
        {
            if (takaraItems[i] != null)
            {
                takaraItems[i].transform.localScale = i < originalItemScales.Length && originalItemScales[i] != Vector3.zero
                    ? originalItemScales[i]
                    : Vector3.one;
            }
        }
    }
    void SetLevelRewardDict()
    {
        levelRewardDict[1] = new List<TreasureBoxReward>
        {
            new TreasureBoxReward { rewardType = TreasureBoxRewardType.Gold, rewardValue = 10, probability = 4.5f },
            new TreasureBoxReward { rewardType = TreasureBoxRewardType.Gold, rewardValue = 20, probability = 1f },
            new TreasureBoxReward { rewardType = TreasureBoxRewardType.Gold, rewardValue = 30, probability = 0.5f },
            new TreasureBoxReward { rewardType = TreasureBoxRewardType.Gear, rewardValue = 1, probability = 2f },
            new TreasureBoxReward { rewardType = TreasureBoxRewardType.Gear, rewardValue = 2, probability = 1.5f },
            new TreasureBoxReward { rewardType = TreasureBoxRewardType.Gear, rewardValue = 3, probability = 0.5f },
        };
        levelRewardDict[2] = new List<TreasureBoxReward>
        {
            new TreasureBoxReward { rewardType = TreasureBoxRewardType.Gold, rewardValue = 30, probability = 4.5f },
            new TreasureBoxReward { rewardType = TreasureBoxRewardType.Gold, rewardValue = 50, probability = 1.5f },
            new TreasureBoxReward { rewardType = TreasureBoxRewardType.Gold, rewardValue = 80, probability = 0.5f },
            new TreasureBoxReward { rewardType = TreasureBoxRewardType.Gear, rewardValue = 3, probability = 2f },
            new TreasureBoxReward { rewardType = TreasureBoxRewardType.Gear, rewardValue = 4, probability = 1f },
            new TreasureBoxReward { rewardType = TreasureBoxRewardType.Gear, rewardValue = 5, probability = 0.5f },
        };
        levelRewardDict[3] = new List<TreasureBoxReward>
        {
           new TreasureBoxReward { rewardType = TreasureBoxRewardType.Gold, rewardValue = 80, probability = 4.5f },
            new TreasureBoxReward { rewardType = TreasureBoxRewardType.Gold, rewardValue = 90, probability = 1.5f },
            new TreasureBoxReward { rewardType = TreasureBoxRewardType.Gold, rewardValue = 100, probability = 0.5f },
            new TreasureBoxReward { rewardType = TreasureBoxRewardType.Gear, rewardValue = 5, probability = 2f },
            new TreasureBoxReward { rewardType = TreasureBoxRewardType.Gear, rewardValue = 7, probability = 1f },
            new TreasureBoxReward { rewardType = TreasureBoxRewardType.Skill, rewardValue = 1, probability = 0.5f },
        };
        levelRewardDict[4] = new List<TreasureBoxReward>
        {
            new TreasureBoxReward { rewardType = TreasureBoxRewardType.Gold, rewardValue = 100, probability = 4.5f },
            new TreasureBoxReward { rewardType = TreasureBoxRewardType.Gold, rewardValue = 120, probability = 1.5f },
            new TreasureBoxReward { rewardType = TreasureBoxRewardType.Gold, rewardValue = 150, probability = 0.5f },
            new TreasureBoxReward { rewardType = TreasureBoxRewardType.Gear, rewardValue = 7, probability = 2f },
            new TreasureBoxReward { rewardType = TreasureBoxRewardType.Gear, rewardValue = 10, probability = 1f },
            new TreasureBoxReward { rewardType = TreasureBoxRewardType.Skill, rewardValue = 1, probability = 0.5f },
        };
        levelRewardDict[5] = new List<TreasureBoxReward>
        {
            new TreasureBoxReward { rewardType = TreasureBoxRewardType.Gear, rewardValue = 15, probability = 0.5f },
            new TreasureBoxReward { rewardType = TreasureBoxRewardType.Skill, rewardValue = 1, probability = 9.5f },
        };
    }

    private TreasureBoxReward SelectRewardByProbability(List<TreasureBoxReward> rewards)
    {
        if (rewards == null || rewards.Count == 0)
            return null;

        float totalProbability = 0f;
        for (int i = 0; i < rewards.Count; i++)
        {
            totalProbability += Mathf.Max(0f, rewards[i].probability);
        }

        if (totalProbability <= 0f)
        {
            return rewards[Random.Range(0, rewards.Count)];
        }

        float randomPoint = Random.value * totalProbability;
        float cumulative = 0f;

        for (int i = 0; i < rewards.Count; i++)
        {
            cumulative += Mathf.Max(0f, rewards[i].probability);
            if (randomPoint <= cumulative)
            {
                return rewards[i];
            }
        }

        return rewards[rewards.Count - 1];
    }

    private void SetRewardItemsForCurrentLevel()
    {
        if (takaraItems == null || takaraItems.Length == 0)
            return;

        for (int i = 0; i < takaraItems.Length; i++)
        {
            int rewardLevel = i + 1;
            if (!levelRewardDict.TryGetValue(rewardLevel, out var rewards) || rewards == null || rewards.Count == 0)
            {
                takaraItems[i].gameObject.SetActive(false);
                continue;
            }

            var rewardPool = new List<TreasureBoxReward>(rewards);
            TreasureBoxReward selectedReward = SelectRewardByProbability(rewardPool);
            takaraItems[i].SetData(selectedReward);
            takaraItems[i].gameObject.SetActive(true);
        }

        UpdateLevelKeyPosition();
    }

    void RollAndJudge(bool isUp)
    {
        if (isRolling || isDone)
            return;

        StartCoroutine(RollDiceRoutine(isUp));
    }

    private IEnumerator RollDiceRoutine(bool isUp)
    {
        isRolling = true;
        btn_up.interactable = false;
        btn_down.interactable = false;

        int switchCount = 14;
        int currentSwitch = 0;

        DOVirtual.Float(0f, 1f, 0.9f, progress =>
        {
            int targetSwitch = Mathf.FloorToInt(progress * switchCount);
            if (targetSwitch <= currentSwitch)
                return;

            currentSwitch = targetSwitch;
            for (int i = 0; i < dices.Length; i++)
            {
                int randomSide = Random.Range(1, 7);
                dices[i].sprite = diceSprites[randomSide];
            }
        }).SetEase(Ease.OutQuad);

        ApplyRandomDiceForces();
        yield return new WaitForSeconds(0.9f);

        int rollResult = 0;
        for (int i = 0; i < dices.Length; i++)
        {
            int diceValue = Random.Range(1, 7);
            rollResult += diceValue;
            dices[i].sprite = diceSprites[diceValue];
        }

        // Stop all dice rigidbodies movement
        for (int i = 0; i < diceRigidbodies.Count; i++)
        {
            if (diceRigidbodies[i] != null)
            {
                diceRigidbodies[i].linearVelocity = Vector2.zero;
                diceRigidbodies[i].angularVelocity = 0f;
            }
        }

        int previousPoint = lastPoint;
        lastPoint = rollResult;
        RefreshLastPointDisplay();

        bool isWin = (isUp && rollResult >= previousPoint) || (!isUp && rollResult <= previousPoint);

        if (isWin)
        {
            OnWin();
        }
        else
        {
            OnLose();
        }

        isRolling = false;
        btn_up.interactable = true;
        btn_down.interactable = true;
    }

    private void ApplyRandomDiceForces()
    {
        if (diceRigidbodies == null)
            return;

        for (int i = 0; i < diceRigidbodies.Count; i++)
        {
            if (diceRigidbodies[i] == null)
                continue;

            var rb = diceRigidbodies[i];
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;

            Vector2 forceDir = new Vector2(
                Random.Range(-1f, 1f),
                Random.Range(-2.5f, 2.5f)
            ).normalized;

            rb.AddForce(forceDir * diceDirectionForce, ForceMode2D.Impulse);
            rb.AddTorque(Random.Range(-diceSpinForce, diceSpinForce), ForceMode2D.Impulse);
        }
    }

    private void OnWin()
    {
        currentLevel++;
        RefreshRewardItemScale();
        UpdateLevelKeyPosition();

        if (currentLevel >= 5)
        {
            ClaimReward();
            return;
        }
    }

    private async void ClaimReward()
    {
        btn_getReward.interactable = false;
        anim_takara.Play("GetReward");
        var rewards = new List<TreasureBoxReward>();
        int rewardCount = Mathf.Min(currentLevel, takaraItems.Length);
        for (int i = 0; i < rewardCount; i++)
        {
            rewards.Add(takaraItems[i].GetReward());
        }

        await Task.Delay(1000); // 等待動畫播放完成

        foreach (var reward in rewards)
        {
            switch (reward.rewardType)
            {
                case TreasureBoxRewardType.Gold:
                    EventCenter.Dispatch(StateEvent.EVENT_GET_GOLD, reward.rewardValue);
                    break;
                case TreasureBoxRewardType.Gear:
                    EventCenter.Dispatch(StateEvent.EVENT_GET_GEAR, reward.rewardValue);
                    break;
                case TreasureBoxRewardType.Skill:
                    EventCenter.Dispatch(StateEvent.EVENT_GET_SKILL, reward.rewardValue);
                    break;
            }
        }
        await Task.Delay(1000);
        EventCenter.Dispatch(StateEvent.EVENT_ENTER_MAP);
    }

    private async void OnLose()
    {
        if (GameDataManager.BackUpKey > 0)
        {
            GameDataManager.BackUpKey--;
            EventCenter.Dispatch(StateEvent.EVENT_SHOW_HINT_BUBBLE, LanguageManager.GetText("T_BrokenKey"));
            UpdateBackupKeyDisplay();
            return;
        }
        btn_getReward.interactable = false;
        EventCenter.Dispatch(StateEvent.EVENT_SHOW_HINT_BUBBLE, LanguageManager.GetText("T_UnLock_Fail"));
        isDone = true;
        await Task.Delay(1000);
        EventCenter.Dispatch(StateEvent.EVENT_ENTER_MAP);
    }
}
public class TreasureBoxReward
{
    public TreasureBoxRewardType rewardType;
    public int rewardValue;
    public float probability = 1f;
}
public enum TreasureBoxRewardType
{
    Gold,
    Gear,
    Skill,
    Key
}
