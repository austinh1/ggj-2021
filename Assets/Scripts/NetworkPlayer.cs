using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class NetworkPlayer : MonoBehaviour
{
    [SerializeField] private TMP_Text m_UsernameText;
    
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

    public PhotonView PhotonView
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

    public Game Game { get; set; }
    
    private MainMenu MainMenu { get; set; }

    public bool OriginallyGhost { get; set; }
    
    private void Awake()
    {
        Game = FindObjectOfType<Game>();
        MainMenu = FindObjectOfType<MainMenu>();
        
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

    public void SendMakeIntoHumanMessage()
    {
        PhotonView.RPC(nameof(MakeIntoHumanRPC), RpcTarget.AllBuffered, PhotonView.Owner);
    }

    [PunRPC]
    public void MakeIntoHumanRPC(Player player)
    {
        if (PhotonView.Owner.Equals(player))
        {
            Debug.Log($"Player {player.ActorNumber} was made into a human!");
            Player.MakeIntoHuman();

            if (Game.CurrentState != Game.GameState.InProgress)
                return;

            var networkPlayers = Game.GetNetworkPlayers();
            var ghostNetworkPlayers = networkPlayers.Where(np => np.GetComponent<PlayerController>().IsGhost);

            if (!ghostNetworkPlayers.Any())
            {
                SendAllHumansMessage();
            }
        }
    }
    
    public void SendMakeIntoHumanAndPositionEveryoneMessage()
    {
        PhotonView.RPC(nameof(MakeIntoHumanRPC), RpcTarget.AllBuffered, PhotonView.Owner);
    }

    [PunRPC]
    public void MakeIntoHumanAndPositionEveryoneRPC(Player player)
    {
        if (PhotonView.Owner.Equals(player))
        {
            Debug.Log($"Player {player.ActorNumber} was made into a human!");
            Player.MakeIntoHuman();
            
            Game.PositionHumanAndGhosts();
        }
    }
    
    public void SendMakeIntoGhostMessage()
    {
        PhotonView.RPC(nameof(MakeIntoGhostRPC), RpcTarget.AllBuffered, PhotonView.Owner);
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

    public void SendSetOriginallyGhostMessage(bool originallyGhost)
    {
        PhotonView.RPC(nameof(SetOriginallyGhostRPC), RpcTarget.AllBuffered, PhotonView.Owner, originallyGhost);
    }
    
    [PunRPC]
    public void SetOriginallyGhostRPC(Player player, bool originallyGhost)
    {
        if (PhotonView.Owner.Equals(player))
        {
            Debug.Log($"Player {player.ActorNumber} was set to be originally a ghost!");
            OriginallyGhost = originallyGhost;
        }
    }


    public void SendSetPositionMessage(Vector3 position)
    {
        PhotonView.RPC(nameof(SetPositionRPC), RpcTarget.All, PhotonView.Owner, position);
    }
    
    [PunRPC]
    public void SetPositionRPC(Player player, Vector3 position)
    {
        if (PhotonView.Owner.Equals(player))
        {
            Debug.Log($"Set position of player {player.ActorNumber}!");
            transform.position = position;
        }
    }
    
    public void SendGotKeyMessage(GameObject key)
    {
        if (!IsLocal)
            return;

        var index = Game.GetKeyIndex(key);
        
        Game.GotKey(index);
        PhotonView.RPC(nameof(GotKeyRPC), RpcTarget.Others, PhotonView.Owner, index);
    }
    
    [PunRPC]
    public void GotKeyRPC(Player player, int keyIndex)
    {
        if (PhotonView.Owner.Equals(player))
        {
            Debug.Log($"Player {player.ActorNumber} found key {keyIndex}!");
            Game.GotKey(keyIndex);
        }
    }

    public void SendGotSandwichMessage()
    {
        if (!IsLocal)
            return;

        Game.GotSandwich();
        PhotonView.RPC(nameof(GotSandwichRPC), RpcTarget.Others, PhotonView.Owner);
    }
    
    [PunRPC]
    public void GotSandwichRPC(Player player)
    {
        if (PhotonView.Owner.Equals(player))
        {
            Game.GotSandwich();
        }
    }
    
    public void SendRestartGameMessage()
    {
        if (!IsLocal)
            return;

        MainMenu.RestartGame();
        PhotonView.RPC(nameof(RestartGameRPC), RpcTarget.Others, PhotonView.Owner);
    }
    
    [PunRPC]
    public void RestartGameRPC(Player player)
    {
        if (PhotonView.Owner.Equals(player))
        {
            MainMenu.RestartGame();
        }
    }

    public void ShowUsername()
    {
        m_UsernameText.enabled = true;
    }

    public void HideUsername()
    {
        m_UsernameText.enabled = false;
    }
    
    public void SendAllHumansMessage()
    {
        if (!IsLocal)
            return;

        Game.AllHumans();
        PhotonView.RPC(nameof(AllHumansRPC), RpcTarget.Others, PhotonView.Owner);
    }
    
    [PunRPC]
    public void AllHumansRPC(Player player)
    {
        if (PhotonView.Owner.Equals(player))
        {
            Game.AllHumans();
        }
    }
}
