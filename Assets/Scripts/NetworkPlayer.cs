using JetBrains.Annotations;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class NetworkPlayer : MonoBehaviour
{
    [SerializeField] private TMP_Text m_UsernameText;
    
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

    public Observable<string> Username { get; } = new Observable<string>();

    private void Awake()
    {
        PlayerMovement.SetEnabled(IsLocal);
        
        Username.OnChange.AddListener(delegate
        {
            if (IsLocal)
                PhotonView.RPC(nameof(SetUsername), RpcTarget.OthersBuffered, PhotonView.Owner, Username.Value);
            
            m_UsernameText.text = Username.Value;
        });
    }

    [PunRPC]
    [UsedImplicitly]
    public void SetUsername(Player player, string username)
    {
        if (PhotonView.Owner.Equals(player))
        {
            Debug.Log($"Set Player {PhotonView.Owner.ActorNumber}'s username to {username}");
            Username.Value = username;
        }
    }
}
