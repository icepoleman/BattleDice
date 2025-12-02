using UnityEngine;

public class test : MonoBehaviour
{
    int playerBlood = 100;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ChangeBlood(playerBlood, -10);
        Debug.Log(playerBlood);
        BaseBuff baseBuff = new BaseBuff();
        baseBuff.ApplyBuff(3, 2);

        Debug.Log(LanguageManager.GetFormat("T_AutoSit_cardTypeHint", 6,1));
    }

    void ChangeBlood(int _characterData, int value)
    {
        _characterData += value;
    }
}
