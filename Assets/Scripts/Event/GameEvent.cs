public class GameEvent
{
    public const string EVENT_CLICK_ROLL = "GameEvent_ClickRoll";//擲骰子
    public const string EVENT_CLICK_TURN_END = "GameEvent_ClickTurnEnd";//結束回合
    public const string EVENT_CLICK_FIGHT = "GameEvent_ClickFight";//戰鬥
    public const string EVENT_CLICK_USE_DICE = "GameEvent_ClickUseDice";//使用骰子
    public const string EVENT_CLICK_CANCEL_SKILL = "GameEvent_ClickCancelSkill";//取消技能
    public const string EVENT_SELECT_SKILL = "GameEvent_SelectSkill";//選取技能
    public const string EVENT_CONFIRM_SELECT_SKILL = "GameEvent_ConfirmSelectSkill";//確認選取技能
    public const string EVENT_STOP_USE_DICE = "GameEvent_StopUseDice";//停止選取技能骰子
    public const string EVENT_CHANGE_STATE = "GameEvent_ChangeState";//改變狀態
    public const string EVENT_SKILL_ATTACK = "GameEvent_SkillAttack";//技能攻擊
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
