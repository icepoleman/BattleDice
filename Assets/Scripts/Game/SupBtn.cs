using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
public class SupBtn : MonoBehaviour
{
    [SerializeField] private TMP_Text chargeTxt;
    [SerializeField] private Slider chargeSlider;
    [SerializeField] private Button btn_use;

    private SupData supData;
    private int currentCharge = 0;
    void Start()
    {
        btn_use.onClick.AddListener(OnUseButtonClick);
    }
    public void SetSupData(SupData data)
    {
        supData = data;
        currentCharge = supData != null && supData.supID != null &&
            GameDataManager.supChargeList.TryGetValue(supData.supID, out int savedCharge)
                ? savedCharge
                : 0;
        chargeSlider.maxValue = supData.skillPoint;
        chargeSlider.value = currentCharge;
        chargeTxt.text = $"{currentCharge}/{supData.skillPoint}";
        btn_use.interactable = currentCharge >= supData.skillPoint;
    }
    void OnEnable()
    {
        EventCenter.AddListener(GameEvent.EVENT_SUPPORTER_CHARGE, ChargeUpdate);
    }

    void OnDisable()
    {
        EventCenter.RemoveListener(GameEvent.EVENT_SUPPORTER_CHARGE, ChargeUpdate);
    }
    void ChargeUpdate(object[] args)
    {
        if (supData.needDicesData == null || supData.supID == null ||
            args == null || args.Length == 0 || !(args[0] is List<int> chargeValues))
        {
            return;
        }

        foreach (int value in chargeValues)
        {
            // 萬用骰 0 直接充能，其他點數需符合需求
            if (value == BaseSkill.WILD_DICE || Array.IndexOf(supData.needDicesData, value) >= 0)
            {
                if (!GameDataManager.supChargeList.TryGetValue(supData.supID, out currentCharge))
                {
                    currentCharge = 0;
                }

                currentCharge++;
                GameDataManager.supChargeList[supData.supID] = currentCharge;
            }
        }
        chargeSlider.value = currentCharge;
        chargeTxt.text = $"{currentCharge}/{supData.skillPoint}";
        btn_use.interactable = currentCharge >= supData.skillPoint;
    }
    void OnUseButtonClick()
    {
        EventCenter.Dispatch(GameEvent.EVENT_USE_SUPPORTER, supData);
        GameDataManager.supChargeList[supData.supID] = 0;
        btn_use.interactable = false;
        chargeSlider.value = 0;
        chargeTxt.text = $"0/{supData.skillPoint}";
    }
    public void CanUseBtn(bool canUse)
    {
        if (btn_use == null)
        {
            return;
        }

        if (!canUse || supData == null)
        {
            btn_use.interactable = false;
            return;
        }

        btn_use.interactable = currentCharge >= supData.skillPoint;
    }
}
