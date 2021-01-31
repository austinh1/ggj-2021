using UnityEngine;

public class LockedDoor : MonoBehaviour
{
    [SerializeField] private Sprite m_OpenedDoor;
    [SerializeField] private Sprite m_ClosedDoor;
    
    private SpriteRenderer m_SpriteRenderer;

    private SpriteRenderer SpriteRenderer
    {
        get
        {
            if (m_SpriteRenderer == null)
                m_SpriteRenderer = GetComponent<SpriteRenderer>();

            return m_SpriteRenderer;
        }
    }
    
    private Collider2D m_Collider2D;

    private Collider2D Collider2D
    {
        get
        {
            if (m_Collider2D == null)
                m_Collider2D = GetComponent<Collider2D>();

            return m_Collider2D;
        }
    }
    
    public void Open()
    {
        Collider2D.enabled = false;
        SpriteRenderer.sprite = m_OpenedDoor;
    }

    public void Close()
    {
        Collider2D.enabled = true;
        SpriteRenderer.sprite = m_ClosedDoor;
    }
}
