using Cinemachine;
using Photon.Pun;
using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera m_CinemachineVirtualCamera;
    [SerializeField] private Camera m_Camera;
    [SerializeField] private MainMenu m_MainMenu;

    private GameState CurrentState { get; set; }

    public void JoinRoom()
    {
        CurrentState = GameState.Setup;
        
        var networkPlayer = PhotonNetwork.Instantiate("PlayerPrefab Austin", Vector2.zero, Quaternion.identity).GetComponent<NetworkPlayer>();
        m_CinemachineVirtualCamera.Follow = networkPlayer.transform;
        m_Camera.transform.position = Vector3.zero;
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
}
