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
    [SerializeField] private TMP_Text m_Error;
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
        }

        void JoinRoom()
        {
            if (string.IsNullOrEmpty(m_RoomCodeField.text))
            {
                m_Error.text = "No room code has been entered!";
                return;
            }
                
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

    public override void OnJoinedRoom()
    {
        RoomCode.Value = m_RoomCodeField.text;
        m_JoinOrCreateRoom.gameObject.SetActive(false);
    }
    
    private static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[Random.Next(s.Length)]).ToArray());
    }
}
