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
    [SerializeField] private TMP_Text m_RoomCode;
    [SerializeField] private GameObject m_JoinOrCreateRoom;

    private string Username { get; set; }

    private Observable<string> RoomCode { get; } = new Observable<string>();
    
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
        
        PhotonNetwork.ConnectUsingSettings();

        void CreateRoom()
        {
            RoomCode.Value = RandomString(4);
            PhotonNetwork.CreateRoom(RoomCode.Value, new RoomOptions());
            EnterRoom();
        }

        void JoinRoom()
        {
            RoomCode.Value = m_RoomCodeField.text;
            PhotonNetwork.JoinRoom(RoomCode.Value);
            EnterRoom();
        }

        void EnterRoom()
        {
            m_JoinOrCreateRoom.gameObject.SetActive(false);
        }
    }
    
    public override void OnConnectedToMaster()
    {
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarningFormat("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {0}", cause);
    }
    
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("PUN Basics Tutorial/Launcher:OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");

        // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
    }
    
    private static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[Random.Next(s.Length)]).ToArray());
    }
}
