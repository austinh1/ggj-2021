using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

public class MainMenu : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_InputField m_RoomCodeField;
    [SerializeField] private Button m_CreateButton;
    [SerializeField] private Button m_JoinButton;
    [SerializeField] private TMP_Text m_Error;
    [SerializeField] private TMP_Text m_RoomCode;
    [SerializeField] private TMP_Text m_PlayerCount;
    [SerializeField] private GameObject m_JoinOrCreateRoom;

    private string Username { get; set; }

    private Observable<string> RoomCode { get; } = new Observable<string>();

    private Observable<int> PlayerCount { get; } = new Observable<int>();
    
    private static Random Random { get; } = new Random();
    
    private void Start()
    {
        m_RoomCodeField.onSubmit.AddListener(delegate
        {
            JoinRoom();
        });
        
        m_CreateButton.onClick.AddListener(CreateRoom);
        
        m_JoinButton.onClick.AddListener(JoinRoom);
        
        RoomCode.OnChange.AddListener(delegate(string oldRoomName, string newRoomName)
        {
            m_RoomCode.text = newRoomName;
        });
        
        PlayerCount.OnChange.AddListener(delegate(int oldInt, int newInt)
        {
            m_PlayerCount.text = $"Player Count: {newInt}";
        });
        
        PhotonNetwork.ConnectUsingSettings();

        void CreateRoom()
        {
            RoomCode.Value = RandomString(4);
            PhotonNetwork.CreateRoom(RoomCode.Value, new RoomOptions());
        }

        void JoinRoom()
        {
            if (string.IsNullOrEmpty(m_RoomCodeField.text))
            {
                m_Error.text = "No room code has been entered!";
                return;
            }
            
            RoomCode.Value = m_RoomCodeField.text;
            PhotonNetwork.JoinRoom(m_RoomCodeField.text);
        }
    }
    
    public override void OnConnectedToMaster()
    {
        Debug.Log("CONNECTED");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("DISCONNECTED");
    }
    
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log($"Error joining room: {returnCode}, {message}");
        m_Error.text = $"Failed to join room {m_RoomCodeField.text}!";
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"Player {newPlayer.ActorNumber} joined!");

        base.OnPlayerEnteredRoom(newPlayer);
        PlayerCount.Value = PhotonNetwork.PlayerList.Length;
    }

    public override void OnJoinedRoom()
    {
        PlayerCount.Value = PhotonNetwork.PlayerList.Length;
        m_JoinOrCreateRoom.gameObject.SetActive(false);
        Debug.Log($"Joined room {RoomCode.Value}");
    }
    
    private static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[Random.Next(s.Length)]).ToArray());
    }
}
