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
    
    private PlayerController m_Player;

    public PlayerController Player
    {
        get
        {
            if (m_Player == null)
                m_Player = GetComponent<PlayerController>();

            return m_Player;
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

    public Observable<string> Username { get; } = new Observable<string>();
    
    private Game Game { get; set; }
    
    private MainMenu MainMenu { get; set; }
    
    private void Awake()
    {
        Game = FindObjectOfType<Game>();
        MainMenu = FindObjectOfType<MainMenu>();
        
        PlayerMovement.SetEnabled(IsLocal);
        
        Username.OnChange.AddListener(delegate
        {
            if (IsLocal)
                PhotonView.RPC(nameof(SetUsernameRPC), RpcTarget.OthersBuffered, PhotonView.Owner, Username.Value);
            
            m_UsernameText.text = Username.Value;
        });

        if (IsLocal)
            Username.Value = MainMenu.Username;
    }

    [PunRPC]
    [UsedImplicitly]
    public void SetUsernameRPC(Player player, string username)
    {
        if (PhotonView.Owner.Equals(player))
        {
            Debug.Log($"Set Player {PhotonView.Owner.ActorNumber}'s username to {username}");
            
            Username.Value = username;
        }
    }

    public void SendStartGameMessage()
    {
        PhotonView.RPC(nameof(StartGameRPC), RpcTarget.OthersBuffered, PhotonView.Owner);
    }

    [PunRPC]
    public void StartGameRPC(Player player)
    {
        if (PhotonView.Owner.Equals(player))
        {
            Debug.Log($"Player {player.ActorNumber} started the game!");
            Game.SetGameStateToInProgress();
        }
    }

    public void MakeIntoHuman()
    {
        PhotonView.RPC(nameof(MakeIntoHumanRPC), RpcTarget.AllBuffered, PhotonView.Owner, Username.Value);
    }

    [PunRPC]
    public void MakeIntoHumanRPC(Player player)
    {
        if (PhotonView.Owner.Equals(player))
        {
            Debug.Log($"Player {player.ActorNumber} was made into a human!");
            Player.MakeIntoHuman();
        }
    }
    
    public void MakeIntoGhost()
    {
        PhotonView.RPC(nameof(MakeIntoGhostRPC), RpcTarget.AllBuffered, PhotonView.Owner, Username.Value);
    }

    [PunRPC]
    public void MakeIntoGhostRPC(Player player)
    {
        if (PhotonView.Owner.Equals(player))
        {
            Debug.Log($"Player {player.ActorNumber} was made into a ghost!");
            Player.MakeIntoGhost();
        }
    }
}
