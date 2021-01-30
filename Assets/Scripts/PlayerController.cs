using UnityEngine;

public class PlayerController : MonoBehaviour
{
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


    public bool IsGhost => GhostController.enabled;
    public bool IsHuman => HumanController.enabled;


    public void MakeIntoGhost()
    {
        GhostController.SetEnabled(true);
        HumanController.SetEnabled(false);
            
        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        spriteRenderer.sprite = HumanController.sprites[Random.Range(0, HumanController.sprites.Length)];
    }

    public void MakeIntoHuman()
    {
        GhostController.SetEnabled(false);
        HumanController.SetEnabled(true);

        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        spriteRenderer.sprite = GhostController.sprites[Random.Range(0, GhostController.sprites.Length)];
    }
}
