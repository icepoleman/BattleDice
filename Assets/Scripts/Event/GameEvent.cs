public class GameEvent
{
    //按鈕 裝置事件
    public const string EVENT_CLICK_ROLL = "GameEvent_ClickRoll";//刷新骰子
    public const string EVENT_CLICK_TURN_END = "GameEvent_ClickTurnEnd";//結束回合
    public const string EVENT_SELECT_SKILL = "GameEvent_SelectSkill";//選取技能
    public const string EVENT_CLEAR_CHOOSE_SKILL = "GameEvent_ClearChooseSkill";//停止選取技能

    //角色用
    public const string EVENT_SKILL_ATTACK = "GameEvent_SkillAttack";//使用攻擊技能
    public const string EVENT_SKILL_HEAL = "GameEvent_SkillHeal";//對角色回復血量
    public const string EVENT_ATTACK_CHARACTER = "GameEvent_AttackCharacter";//對角色造成傷害
    public const string EVENT_ADD_POWER_DICE = "GameEvent_AddPowerDice";//增加能量骰子
    public const string EVENT_PLAYER_USE_SKILL = "GameEvent_UseSkill";//使用技能
    public const string EVENT_ADD_BUFF = "GameEvent_AddBuff";//新增buff
    public const string EVENT_UPDATE_BUFF = "GameEvent_UpdateBuff";//更新buff

    //UI
    public const string EVENT_UPDATE_MANA_DICE = "GameEvent_UpdateManaDice";//更新能量骰子顯示
    public const string EVENT_UPDATE_BLOOD_UI = "GameEvent_UpdateBloodUI";//更新血量UI
}
public class StateEvent
{
    public const string EVENT_ENTER_DICEGAME = "StateEvent_EnterDiceGame";//進入骰子遊戲
    public const string EVENT_ENTER_AVG = "StateEvent_EnterAVG";//進入劇情
    public const string EVENT_ENTER_MAP = "StateEvent_EnterMap";//進入地圖
    public const string EVENT_ENTER_PREPARATION_ROOM = "StateEvent_EnterPreparationRoom";//進入整備室
    public const string EVENT_ENTER_SHOP = "StateEvent_EnterShop";//進入商店
    public const string EVENT_LOADING_SCREEN = "StateEvent_ShowLoadingScreen";//載入畫面開關
}
public class MapEvent
{
    public const string EVENT_OPEN_NEXT_STAGE_NODE = "MapEvent_OpenNextStageNode"; //開啟關卡節點事件
    public const string EVENT_RECOVER_HEALTH = "MapEvent_RecoverHealth"; //回血關卡通知
    public const string EVENT_GET_GOLD = "MapEvent_GetGold"; //取得金幣通知
    public const string EVENT_GET_GEAR = "MapEvent_GetGear"; //取得齒輪通知
    public const string EVENT_GET_ITEM = "MapEvent_GetItem";//取得道具通知(特殊道具)
}
public class AdvEvent
{
    public const string EVENT_CLICK_CHOICE = "AdvEvent_ClickChoice";//點擊選項
    public const string EVENT_SHAKE_CAMERA = "AdvEvent_ShakeCamera"; //震動camera
}
