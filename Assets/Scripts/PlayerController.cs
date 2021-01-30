using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerType CurrentType { get; set; }
        
    private GhostController m_GhostController;

    private GhostController GhostController
    {
        get
        {
            if (m_GhostController == null)
                m_GhostController = GetComponent<GhostController>();

            return m_GhostController;
        }
    }
        
    private HumanController m_HumanController;

    private HumanController HumanController
    {
        get
        {
            if (m_HumanController == null)
                m_HumanController = GetComponent<HumanController>();

            return m_HumanController;
        }
    }


    public bool IsGhost { get { return GhostController.enabled; } }
    public bool IsHuman { get { return HumanController.enabled; } }


    public void MakeIntoGhost()
    {
        CurrentType = PlayerType.Ghost;
            
        GhostController.SetEnabled(true);
        HumanController.SetEnabled(false);
            
        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        spriteRenderer.sprite = HumanController.sprites[Random.Range(0, HumanController.sprites.Length)];
    }

    public void MakeIntoHuman()
    {
        CurrentType = PlayerType.Human;

        GhostController.SetEnabled(false);
        HumanController.SetEnabled(true);

        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        spriteRenderer.sprite = GhostController.sprites[Random.Range(0, GhostController.sprites.Length)];
    }

    public enum PlayerType
    {
        Ghost,
        Human
    }
}
