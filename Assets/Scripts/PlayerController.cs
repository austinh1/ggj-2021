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

    public bool IsGhost => GhostController.enabled;
    public bool IsHuman => HumanController.enabled;

    public Sprite PlayerSprite { get; set; }

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
        
        PlayerSprite = ghostSprite.GetComponent<SpriteRenderer>().sprite;
    }

    public void MakeIntoHuman()
    {
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

        PlayerSprite = humanSprite.GetComponent<SpriteRenderer>().sprite;
    }
}
