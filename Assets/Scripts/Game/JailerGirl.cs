using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class JailerGirl : MonoBehaviour
{
    List<int> diceResults;//擲骰子結果 給DiceGame參考 本身不改變
    GameObject diceBox = null;
    PlayerData playerData = new PlayerData();
    Sprite[] diceSprites;

    public void Awake()
    {
        diceBox = gameObject.transform.Find("diceBox").gameObject;
        diceSprites = Resources.LoadAll<Sprite>("dice/dice_base");
    }
    public void SetData(ICharacterData _playerData)
    {
        playerData.maxBlood = _playerData.maxBlood;
        playerData.currentBlood = _playerData.currentBlood;
        playerData.diceSides = _playerData.diceSides;
        playerData.diceCount = _playerData.diceCount;
        playerData.keepDiceCount = _playerData.keepDiceCount;

        diceResults = new List<int>();
    }
    public IEnumerator Roll(int diceCount)
    {
        Debug.Log("擲骰子");
        diceResults.Clear();
        diceResults = playerData.RollDice(diceCount);
        yield return new WaitForSeconds(0.5f);
        //生成骰子物件在diceBox下
        ClearDiceBox();
        for (int i = 0; i < diceResults.Count; i++)
        {
            GameObject dice = new GameObject("dice");
            dice.transform.SetParent(diceBox.transform);
            RectTransform rt = dice.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(25, 25);
            rt.localScale = new Vector3(1, 1, 1);
            Image sr = dice.AddComponent<Image>();
            sr.sprite = diceSprites[diceResults[i] - 1];
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(0.5f);
        EventCenter.Dispatch(GameEvent.EVENT_CHANGE_STATE, TurnState.playerTurn);
        ClearDiceBox();
    }
    //取得擲骰子結果
    public List<int> GetDiceResults()
    {
        return diceResults;
    }
    //取得角色數據
    public ICharacterData GetCharacterData()
    {
        return playerData;
    }
    public void Hurt(float damage)
    {
        Debug.Log("受傷" + damage);
        playerData.Hurt(damage);
        //動畫表演
    }
    public void Heal(float heal)
    {
        Debug.Log("治療" + heal);
        playerData.Heal(heal);
        //動畫表演
    }
    public void Attack()
    {
        Debug.Log("攻擊");
        //攻擊動畫
    }
    public bool IsDead()
    {
        if (playerData.IsDead())
            Debug.Log("dead");
        return playerData.IsDead();
    }
    void ClearDiceBox()
    {
        foreach (Transform child in diceBox.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
