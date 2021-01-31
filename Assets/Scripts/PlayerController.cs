using System;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

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
    
    private PhotonView m_PhotonView;

    private PhotonView PhotonView
    {
        get
        {
            if (m_PhotonView == null)
                m_PhotonView = GetComponent<PhotonView>();

            return m_PhotonView;
        }
    }
    
    private bool IsLocal => PhotonView.IsMine;

    public bool IsGhost { get; private set; } = true;
    public bool IsHuman { get; private set; }

    public SpriteRenderer PlayerSpriteRenderer { get; private set; }
    public Sprite PlayerSprite { get; private set; }
    public Animator PlayerAnimator { get; private set; }

    private void Start()
    {
        if (IsLocal)
            return;
        
        GhostController.SetEnabled(false);
        HumanController.SetEnabled(false);
        PossessObject.enabled = false;
    }

    public void MakeIntoGhost()
    {
        IsGhost = true;
        IsHuman = false;
        
        if (IsLocal)
        {
            GhostController.SetEnabled(true);
            HumanController.SetEnabled(false);
            PossessObject.enabled = true;
        }

        var ghostSprite = transform.Find("GhostSprite").gameObject;
        var humanSprite = transform.Find("HumanSprite").gameObject;
        ghostSprite.SetActive(true);
        humanSprite.SetActive(false);

        PlayerSpriteRenderer = ghostSprite.GetComponent<SpriteRenderer>();
        PlayerSprite = PlayerSpriteRenderer.sprite;
        PlayerAnimator = ghostSprite.GetComponent<Animator>();
    }

    public void MakeIntoHuman()
    {
        IsGhost = false;
        IsHuman = true;
        
        if (IsLocal)
        {
            GhostController.SetEnabled(false);
            HumanController.SetEnabled(true);
            PossessObject.enabled = false;    
        }

        var ghostSprite = transform.Find("GhostSprite").gameObject;
        var humanSprite = transform.Find("HumanSprite").gameObject;
        ghostSprite.SetActive(false);
        humanSprite.SetActive(true);

        PlayerSpriteRenderer = humanSprite.GetComponent<SpriteRenderer>();
        PlayerSprite = PlayerSpriteRenderer.sprite;
        PlayerAnimator = humanSprite.GetComponent<Animator>();
    }

    public void SetFlipX(bool flipX)
    {
        PlayerSpriteRenderer.flipX = flipX;
    }
}
