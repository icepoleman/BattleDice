public class GameEvent
{
    public const string EVENT_RESTART_GAME = "GameEvent_RestartGame";//重新開始遊戲
    public const string EVENT_ESCAPE_BATTLE = "GameEvent_EscapeBattle";//逃離戰鬥通知
    //按鈕 裝置事件
    public const string EVENT_CLICK_TURN_END = "GameEvent_ClickTurnEnd";//結束回合
    public const string EVENT_SELECT_SKILL = "GameEvent_SelectSkill";//選取技能
    public const string EVENT_CLEAR_CHOOSE_SKILL = "GameEvent_ClearChooseSkill";//停止選取技能

    //怪物 玩家通用
    //技能通知
    public const string EVENT_USE_SKILL = "GameEvent_UseSkill";//使用技能通知
    public const string EVENT_USE_BUFF = "GameEvent_UseBuff";//使用buff通知
    //角色用
    public const string EVENT_ATTACK_CHARACTER = "GameEvent_AttackCharacter";//對角色造成傷害
    public const string EVENT_BUFF_EFFECT_BLOOD = "GameEvent_BuffEffectBlood";//Buff效果造成的血量變化
    public const string EVENT_DICE_SELECTION_CHANGED = "GameEvent_DiceSelectionChanged";//骰子選取狀態變更
    public const string EVENT_ADD_BUFF = "GameEvent_AddBuff";//新增buff
    public const string EVENT_UPDATE_BUFF = "GameEvent_UpdateBuff";//更新buff
    public const string EVENT_DESTROY_ENEMY_DICE = "GameEvent_DestroyEnemyDice";//破壞敵人骰子
    public const string EVENT_GENERATE_MANA_DICE = "GameEvent_GenerateManaDice";//生成能量骰子給裝置
    public const string EVENT_ENEMY_REROLL = "GameEvent_EnemyReroll";//敵人重新擲骰並再次攻擊
    public const string EVENT_CLEAR_NEGATIVE_BUFFS = "GameEvent_ClearNegativeBuffs";//清除所有負面 Buff

    //UI
    public const string EVENT_UPDATE_BLOOD_UI = "GameEvent_UpdateBloodUI";//更新血量UI
}
public class StateEvent
{
    public const string EVENT_ENTER_MENU = "StateEvent_EnterMenu";//進入主選單
    public const string EVENT_ENTER_DICEGAME = "StateEvent_EnterDiceGame";//進入骰子遊戲
    public const string EVENT_ENTER_AVG = "StateEvent_EnterAVG";//進入劇情
    public const string EVENT_TEST_AVGMENU = "StateEvent_TestAVGMenu";//測試劇情menu
    public const string EVENT_ENTER_MAP = "StateEvent_EnterMap";//進入地圖
    public const string EVENT_ENTER_PREPARATION_ROOM = "StateEvent_EnterPreparationRoom";//進入整備室
    public const string EVENT_ENTER_SHOP = "StateEvent_EnterShop";//進入商店
    public const string EVENT_LOADING_SCREEN = "StateEvent_ShowLoadingScreen";//載入畫面開關
    public const string EVENT_BACK_PREVIOUS_SCENE = "StateEvent_BackPreviousScene";//返回前一場景
    public const string EVENT_SETTING_CHANGED = "StateEvent_SettingChanged";//設定變更後通知
    public const string EVENT_ENTER_END_SCENE = "StateEvent_EnterEndScene";//進入結算畫面
}
public class MapEvent
{
    public const string EVENT_COMPLETE_MAP = "MapEvent_CompleteMap"; //完成當前地圖事件
    public const string EVENT_OPEN_NEXT_STAGE_NODE = "MapEvent_OpenNextStageNode"; //開啟關卡節點事件
    public const string EVENT_RECOVER_HEALTH = "MapEvent_RecoverHealth"; //回血關卡通知
    public const string EVENT_GET_GOLD = "MapEvent_GetGold"; //取得金幣通知
    public const string EVENT_SPEND_GOLD = "MapEvent_SpendGold"; //花費金幣通知
    public const string EVENT_GET_GEAR = "MapEvent_GetGear"; //取得齒輪通知
    public const string EVENT_GET_ITEM = "MapEvent_GetItem";//取得道具通知(特殊道具)
    public const string EVENT_GET_SKILL = "MapEvent_GetSkill";//取得技能通知
    public const string EVENT_OPEN_MAP_SHOP = "MapEvent_OpenMapShop";//開啟地圖商店
    public const string EVENT_UNCHOOSE_SKILL = "MapEvent_UnchooseSkill";//取消選取技能
}
public class AdvEvent
{
    public const string EVENT_CLICK_CHOICE = "AdvEvent_ClickChoice";//點擊選項
}