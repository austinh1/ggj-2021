using System;
using JetBrains.Annotations;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour
{
    public AudioClip slapSound;

    private GhostController m_GhostController;
    private MainMenu m_MainMenu;

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

    public MainMenu MainMenu
    {
        get
        {
            if (m_MainMenu == null)
            {
                var gameObj = GameObject.Find("Main Menu");
                if (gameObj != null)
                {
                    m_MainMenu = gameObj.GetComponent<MainMenu>();
                }
            }

            return m_MainMenu;
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
        if (IsHuman)
        {
            MainMenu.ModifyGhostCount(1);
            MainMenu.ModifyHumanCount(-1);
        }

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

        GhostController.IsConverting = false;
    }

    public void MakeIntoHuman()
    {
        if (IsGhost)
        {
            MainMenu.ModifyHumanCount(1);
            MainMenu.ModifyGhostCount(-1);
        }

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
        // Never flip while posessing.
        if (PossessObject.IsPossessing)
        {
            PlayerSpriteRenderer.flipX = false;
            return;
        }

        PlayerSpriteRenderer.flipX = flipX;
    }

    public void PlaySlapSound()
    {
        var audioSource = GetComponent<AudioSource>();
        audioSource.PlayOneShot(slapSound);
    }
    
    #region PunRPCs
    [PunRPC]
    [UsedImplicitly]
    public void WalkAnimationRPC(Player player, bool walking)
    {
        if (!PhotonView.Owner.Equals(player)) return;
        PlayerAnimator.SetBool("Walking", walking);
    }
    
    [PunRPC]
    [UsedImplicitly]
    public void SlapAnimationRPC(Player player)
    {
        if (!PhotonView.Owner.Equals(player)) return;
        PlayerAnimator.SetTrigger("Slapping");
    }
    
    [PunRPC]
    [UsedImplicitly]
    public void FaceDirectionRPC(Player player, bool facingLeft)
    {
        SetFlipX(facingLeft);
    }
    #endregion
}
