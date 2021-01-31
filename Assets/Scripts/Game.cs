using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using Photon.Pun;
using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera m_CinemachineVirtualCamera;
    [SerializeField] private Camera m_Camera;
    [SerializeField] private MainMenu m_MainMenu;
    [SerializeField] private List<Transform> m_GhostSpawnPoints;
    [SerializeField] private List<Transform> m_HumanSpawnPoints;
    [SerializeField] private List<LockedDoor> m_LockedDoors;
    [SerializeField] private List<GameObject> m_Keys;
    [SerializeField] private GameObject m_Sandwich;

    public Dictionary<int, int> TotalPlayerToStartingHumansMap { get; } = new Dictionary<int, int>()
    {
        {2, 1},
        {3, 1},
        {4, 1},
        {5, 1},
        {6, 1},
        {7, 1},
        {8, 1},
        {9, 1},
        {10, 1},
    };

    public GameState CurrentState { get; set; }

    private NetworkPlayer NetworkPlayer { get; set; }
    
    private int KeysLeft { get; set; }
    
    private float StartTime { get; set; }
    
    private float SetUpDuration { get; } = 1f;

    public bool SettingUp
    {
        get
        {
            var timeSinceStart = Time.time - StartTime;
            return timeSinceStart < SetUpDuration;
        }
    }

    private void Start()
    {
        ResetDoorKeyAndSandwich();
    }

    public void JoinRoom()
    {
        CurrentState = GameState.Setup;
        
        NetworkPlayer = PhotonNetwork.Instantiate("Player", Vector2.zero, Quaternion.identity).GetComponent<NetworkPlayer>();
            
        m_CinemachineVirtualCamera.Follow = NetworkPlayer.transform;
        m_Camera.transform.position = Vector3.zero;
        
        if (PhotonNetwork.IsMasterClient)
        {
            NetworkPlayer.SendMakeIntoHumanMessage();
            NetworkPlayer.transform.position = m_HumanSpawnPoints[0].position;
        }
        else
        {
            NetworkPlayer.SendMakeIntoGhostMessage();
            NetworkPlayer.SendSetOriginallyGhostMessage(true);
            
            var index = PhotonNetwork.PlayerList.ToList().IndexOf(NetworkPlayer.PhotonView.Owner);
            var ghostSpawnPoint = m_GhostSpawnPoints[index - 1];
            NetworkPlayer.transform.position = ghostSpawnPoint.position;
        }

        m_MainMenu.NetworkPlayer = NetworkPlayer;
        
        ResetDoorKeyAndSandwich();
    }

    public void LeaveRoom()
    {
        CurrentState = GameState.Lobby;
        m_Camera.transform.position = Vector3.zero;
    }

    public enum GameState
    {
        Lobby,
        Setup,
        InProgress,
        Complete
    }  

    public void StartGameMaster()
    {
        NetworkPlayer.SendStartGameMessage();
        SetGameStateToInProgress();

        StartCoroutine(AssignHumansAndPositionAsync());
    }

    private IEnumerator AssignHumansAndPositionAsync()
    {
        var networkPlayers = GetNetworkPlayers();
        
        TotalPlayerToStartingHumansMap.TryGetValue(networkPlayers.Count, out var humanCount);
        var humanNetworkPlayers = networkPlayers.Where(pv => pv.GetComponent<PlayerController>().IsHuman).ToList();
        
        if (humanNetworkPlayers.Count < humanCount)
        {
            var neededHumanCount = humanCount - humanNetworkPlayers.Count;
            var ghostNetworkPlayers = networkPlayers.Where(pv => pv.GetComponent<PlayerController>().IsGhost).ToList();
            for (var i = 0; i < neededHumanCount; i++)
            {
                ghostNetworkPlayers[i].SendMakeIntoHumanMessage();
            }
        }

        yield return new WaitForSeconds(.2f);
        
        PositionHumanAndGhosts();
    }

    public void PositionHumanAndGhosts()
    {
        var networkPlayers = GetNetworkPlayers();
        var humanNetworkPlayers = networkPlayers.Where(pv => pv.GetComponent<PlayerController>().IsHuman).ToList();
        for (var i = 0; i < humanNetworkPlayers.Count; i++)
        {
            humanNetworkPlayers[i].SendSetPositionMessage(m_HumanSpawnPoints[i].position);
        }

        var ghostNetworkPlayers = networkPlayers.Where(pv => pv.GetComponent<PlayerController>().IsGhost).ToList();

        if (!ghostNetworkPlayers.Any())
            return;

        for (var i = 0; i < m_GhostSpawnPoints.Count; i++)
        {
            if (i >= ghostNetworkPlayers.Count)
                break;
            
            var spawnPoint = m_GhostSpawnPoints[i];
            
            var ghostNetworkPlayer = ghostNetworkPlayers[i];
            var position = spawnPoint.position;
            
            ghostNetworkPlayer.SendSetPositionMessage(position);
        }
    }

    public void SetGameStateToInProgress()
    {
        CurrentState = GameState.InProgress;
        StartTime = Time.time;

        foreach (var key in m_Keys)
        {
            key.SetActive(true);
        }
        
        m_Sandwich.SetActive(true);
    }

    public void GotKey(int keyIndex)
    {
        var key = m_Keys[keyIndex];
        key.gameObject.SetActive(false);

        KeysLeft -= 1;
        m_MainMenu.UpdateKeysLeft(KeysLeft);
        
        if (KeysLeft <= 0)
        {
            foreach (var lockedDoor in m_LockedDoors)
                lockedDoor.Open();
        }
    }

    public int GetKeyIndex(GameObject key)
    {
        return m_Keys.IndexOf(key);
    }

    public void GotSandwich()
    {
        CurrentState = GameState.Complete;

        m_MainMenu.OpenGhostsWin();
        
        m_Sandwich.SetActive(false);
    }
    
    public void AllHumans()
    {
        CurrentState = GameState.Complete;

        m_MainMenu.OpenHumansWin();
        
        m_Sandwich.SetActive(false);
    }

    public void RestartGame()
    {
        CurrentState = GameState.Lobby;

        ResetDoorKeyAndSandwich();
    }

    public void ResetDoorKeyAndSandwich()
    {
        foreach (var lockedDoor in m_LockedDoors)
            lockedDoor.Close();

        foreach (var key in m_Keys)
            key.SetActive(false);
        
        m_Sandwich.SetActive(false);

        KeysLeft = m_Keys.Count;
        m_MainMenu.UpdateKeysLeft(KeysLeft);
    }

    public List<NetworkPlayer> GetNetworkPlayers()
    {
        var playersInRoom = PhotonNetwork.PlayerList;
        return PhotonNetwork.PhotonViews.Where(pv => playersInRoom.Contains(pv.Owner)).Select(pv => pv.GetComponent<NetworkPlayer>()).ToList();
    }
}
