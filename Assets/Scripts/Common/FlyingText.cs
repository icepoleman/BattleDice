using UnityEngine;

public class FlyingText : MonoBehaviour
{
    private float speed;
    private float lifetime;
    private RectTransform rect;
    
    public void Setup(float speed, float lifetime)
    {
        this.speed = speed;
        this.lifetime = lifetime;
        rect = GetComponent<RectTransform>();
        Destroy(gameObject, lifetime);
    }
    
    void Update()
    {
        if (rect != null)
        {
            rect.anchoredPosition += Vector2.right * speed * Time.deltaTime;
        }
    }
}
