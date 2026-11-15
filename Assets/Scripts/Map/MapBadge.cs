using UnityEngine;

public class MapBadge : MonoBehaviour
{
    void OnEnable()
    {
        EventCenter.AddListener(MapEvent.EVENT_UPDATE_MAP_BADGE_POSITION, OnUpdatePosition);
    }

    void OnDisable()
    {
        EventCenter.RemoveListener(MapEvent.EVENT_UPDATE_MAP_BADGE_POSITION, OnUpdatePosition);
    }

    void OnUpdatePosition(object[] param)
    {
        if (param == null || param.Length == 0 || !(param[0] is Vector3 position))
        {
            Debug.LogWarning("[MapBadge] 收到無效的位置事件");
            return;
        }

        transform.position = position;
    }
}
