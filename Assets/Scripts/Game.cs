using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using DefaultNamespace;
using Photon.Pun;
using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera m_CinemachineVirtualCamera;
    [SerializeField] private Camera m_Camera;
    [SerializeField] private MainMenu m_MainMenu;
    [SerializeField] private List<Transform> m_GhostSpawnPoints;
    [SerializeField] private Transform m_HumanSpawnPoint;

    private GameState CurrentState { get; set; }

    private NetworkPlayer NetworkPlayer { get; set; }

    public void JoinRoom()
    {
        CurrentState = GameState.Setup;
        
        NetworkPlayer = PhotonNetwork.Instantiate("PlayerPrefab Austin", Vector2.zero, Quaternion.identity).GetComponent<NetworkPlayer>();
            
        m_CinemachineVirtualCamera.Follow = NetworkPlayer.transform;
        m_Camera.transform.position = Vector3.zero;

        if (PhotonNetwork.IsMasterClient)
            NetworkPlayer.Player.MakeIntoHuman();
        else
            NetworkPlayer.Player.MakeIntoGhost();
            
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
        var humanPhotonView = photonViews.First(pv => pv.GetComponent<Player>() != null && pv.GetComponent<Player>().CurrentType == PlayerType.Human);
        humanPhotonView.transform.position = m_HumanSpawnPoint.position;
        
        
    }

    public void SetGameStateToInProgress()
    {
        CurrentState = GameState.InProgress;
    }
}
