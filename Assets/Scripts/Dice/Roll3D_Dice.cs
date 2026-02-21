using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Roll3D_Dice : MonoBehaviour
{
    int diceValue;
    Transform trans;
    public Animator anim;
    
    bool isRolling = false;
    bool isSettling = false;
    Quaternion targetRotation;
    
    // 快速旋轉的速度
    public float spinSpeed = 1000f;
    // 旋轉時間
    public float spinDuration = 1f;
    // 定位速度
    public float settleSpeed = 50f;
    
    Dictionary<int, Quaternion> faceRotation = new()
    {
        {1, Quaternion.Euler(-180, 0, 0)},
        {2, Quaternion.Euler(-90, 0, 0)},
        {3, Quaternion.Euler(-90, 0, -90)},
        {4, Quaternion.Euler(0, 90, 0)},
        {5, Quaternion.Euler(-90, 0, 0)},
        {6, Quaternion.Euler(180, 0, 0)},
    };
    
    void Start()
    {
        trans = transform;
    }

    public void RollDice()
    {
        if (!isRolling && !isSettling)
        {
            StartCoroutine(RollDiceCoroutine());
            anim.Play("RollDice");
        }
    }
    
    IEnumerator RollDiceCoroutine()
    {
        isRolling = true;
        isSettling = false;
        
        // 快速旋轉 1 秒，每 0.3 秒換一次方向
        float elapsedTime = 0f;
        float directionChangeInterval = 0.3f;
        float timeSinceDirectionChange = 0f;
        
        Vector3 randomAxis = GetRandomAxis();
        
        while (elapsedTime < spinDuration)
        {
            trans.Rotate(randomAxis * spinSpeed * Time.deltaTime);
            
            timeSinceDirectionChange += Time.deltaTime;
            if (timeSinceDirectionChange >= directionChangeInterval)
            {
                randomAxis = GetRandomAxis();
                timeSinceDirectionChange = 0f;
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // 設定目標旋轉為指定面
        diceValue = UnityEngine.Random.Range(1, 7);
        targetRotation = faceRotation[diceValue];
        
        isRolling = false;
        isSettling = true;
    }
    
    Vector3 GetRandomAxis()
    {
        return new Vector3(
            UnityEngine.Random.Range(-1f, 1f),
            UnityEngine.Random.Range(-1f, 1f),
            UnityEngine.Random.Range(-1f, 1f)
        ).normalized;
    }
    
    // Update is called once per frame
    void Update()
    {
        if (isSettling)
        {
            trans.rotation = Quaternion.Lerp(trans.rotation, targetRotation, Time.deltaTime * settleSpeed);
            
            // 當接近目標時停止
            if (Quaternion.Angle(trans.rotation, targetRotation) < 0.1f)
            {
                trans.rotation = targetRotation;
                isSettling = false;
            }
        }
        
        if (!isRolling && !isSettling)
        {
            diceValue = UnityEngine.Random.Range(1, 7);
        }
    }
}
