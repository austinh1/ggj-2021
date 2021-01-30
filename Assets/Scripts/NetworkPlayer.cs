using Photon.Pun;
using UnityEngine;

public class NetworkPlayer : MonoBehaviour
{
    private GhostMovement m_GhostMovement;

    private GhostMovement GhostMovement
    {
        get
        {
            if (m_GhostMovement == null)
                m_GhostMovement = GetComponent<GhostMovement>();

            return m_GhostMovement;
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

    private void Awake()
    {
        GhostMovement.enabled = IsLocal;
    }
}
