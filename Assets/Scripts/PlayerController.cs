﻿using UnityEngine;

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

    private PossessObject m_PossessObject;

    private PossessObject PossessObject
    {
        get
        {
            if (m_PossessObject == null)
                m_PossessObject = GetComponent<PossessObject>();

            return m_PossessObject;
        }
    }

    public bool IsGhost => GhostController.enabled;
    public bool IsHuman => HumanController.enabled;

    public Sprite PlayerSprite { get; set; }

    public void MakeIntoGhost()
    {
        GhostController.SetEnabled(true);
        HumanController.SetEnabled(false);
        PossessObject.enabled = true;

        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        Sprite randomSprite = GhostController.sprites[Random.Range(0, GhostController.sprites.Length)];
        
        spriteRenderer.sprite = randomSprite;
        PlayerSprite = randomSprite;
    }

    public void MakeIntoHuman()
    {
        GhostController.SetEnabled(false);
        HumanController.SetEnabled(true);
        PossessObject.enabled = false;
        
        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        spriteRenderer.sprite = HumanController.sprites[Random.Range(0, HumanController.sprites.Length)];
    }
}
