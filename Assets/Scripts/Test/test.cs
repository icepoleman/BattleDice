using UnityEngine;

public class test : MonoBehaviour
{
    int playerBlood = 100;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ChangeBlood(playerBlood, -10);
        Debug.Log(playerBlood);
    }

    void ChangeBlood(int _characterData, int value)
    {
        _characterData += value;
    }
}
