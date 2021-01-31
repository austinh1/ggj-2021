using System;
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
    [SerializeField] private Transform m_HumanSpawnPoint;
    [SerializeField] private List<GameObject> m_LockedDoors;
    [SerializeField] private List<Key> m_Keys;

    private GameState CurrentState { get; set; }

    private NetworkPlayer NetworkPlayer { get; set; }
    
    private int KeysLeft { get; set; }

    private void Start()
    {
        KeysLeft = m_Keys.Count;
    }

    public void JoinRoom()
    {
        CurrentState = GameState.Setup;
        
        NetworkPlayer = PhotonNetwork.Instantiate("Player", Vector2.zero, Quaternion.identity).GetComponent<NetworkPlayer>();
            
        m_CinemachineVirtualCamera.Follow = NetworkPlayer.transform;
        m_Camera.transform.position = Vector3.zero;

        if (PhotonNetwork.IsMasterClient)
            NetworkPlayer.MakeIntoHuman();
        else
            NetworkPlayer.MakeIntoGhost();
            
    }

    public void LeaveRoom()
    {
        CurrentState = GameState.Lobby;
        m_Camera.transform.position = Vector3.zero;
    }

    private enum GameState
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

        var photonViews = PhotonNetwork.PhotonViews;
        var humanPhotonView = photonViews.First(pv => pv.GetComponent<PlayerController>() != null && pv.GetComponent<PlayerController>().IsHuman);
        humanPhotonView.transform.position = m_HumanSpawnPoint.position;

        var ghostPhotonViews = photonViews.Where(pv =>
            pv.GetComponent<PlayerController>() != null && pv.GetComponent<PlayerController>().IsGhost).ToList();

        if (!ghostPhotonViews.Any())
            return;

        for (var i = 0; i < m_GhostSpawnPoints.Count; i++)
        {
            if (i >= ghostPhotonViews.Count)
                break;
            
            var spawnPoint = m_GhostSpawnPoints[i];
            
            var ghostPhotonView = ghostPhotonViews[i];
            var position = spawnPoint.position;
            
            var networkPlayer = ghostPhotonView.GetComponent<NetworkPlayer>();
            networkPlayer.SendSetPositionMessage(position);
        }
    }

    public void SetGameStateToInProgress()
    {
        CurrentState = GameState.InProgress;
    }

    public void GotKey(int keyIndex)
    {
        var key = m_Keys[keyIndex];
        key.gameObject.SetActive(false);

        KeysLeft -= 1;

        if (KeysLeft <= 0)
        {
            foreach (var lockedDoor in m_LockedDoors)
                lockedDoor.gameObject.SetActive(false);
        }
    }

    public int GetKeyIndex(Key key)
    {
        return m_Keys.IndexOf(key);
    }
}
