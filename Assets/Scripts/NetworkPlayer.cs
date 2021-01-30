using Photon.Pun;
using UnityEngine;

public class NetworkPlayer : MonoBehaviour
{
    private IPlayerMovement m_PlayerMovement;

    private IPlayerMovement PlayerMovement => m_PlayerMovement ??= GetComponent<IPlayerMovement>();

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
        PlayerMovement.SetEnabled(IsLocal);
    }
}
