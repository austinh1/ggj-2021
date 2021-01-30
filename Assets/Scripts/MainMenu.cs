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
    [SerializeField] private TMP_InputField m_UsernameField;
    [SerializeField] private Button m_CreateButton;
    [SerializeField] private Button m_JoinButton;
    [SerializeField] private Button m_LeaveButton;
    [SerializeField] private Button m_StartButton;
    [SerializeField] private TMP_Text m_Error;
    [SerializeField] private TMP_Text m_RoomCode;
    [SerializeField] private TMP_Text m_PlayerCount;
    [SerializeField] private TMP_Text m_Connecting;
    [SerializeField] private GameObject m_JoinOrCreateRoom;
    [SerializeField] private Game m_Game;

    public string Username { get; private set; }

    private Observable<string> RoomCode { get; } = new Observable<string>();

    private Observable<int> PlayerCount { get; } = new Observable<int>();
    
    private static Random Random { get; } = new Random();
    
    public NetworkPlayer NetworkPlayer { get; set; }
    
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
        
        m_LeaveButton.onClick.AddListener(delegate
        {
            PhotonNetwork.LeaveRoom();
        });
        
        m_StartButton.onClick.AddListener(delegate
        {
            m_StartButton.gameObject.SetActive(false);
            m_Game.StartGameMaster();
        });
        
        PhotonNetwork.ConnectUsingSettings();

        void CreateRoom()
        {
            if (!ValidateUsername())
                return;

            // Generate a random string and attempt to make room a few times, to greatly reduce the risk of random string collisions causing a failed room create.
            bool success = false;
            int attempt = 0;
            do
            {
                RoomCode.Value = RandomString(4);
                success = PhotonNetwork.CreateRoom(RoomCode.Value, new RoomOptions());
                attempt++;
            } while (!success && attempt < 5);
        }

        void JoinRoom()
        {
            if (!ValidateUsername())
                return;
            
            if (string.IsNullOrEmpty(m_RoomCodeField.text))
            {
                m_Error.text = "No room code has been entered!";
                return;
            }
            
            RoomCode.Value = m_RoomCodeField.text.ToUpper();
            PhotonNetwork.JoinRoom(RoomCode.Value);
        }

        bool ValidateUsername()
        {
            if (string.IsNullOrEmpty(m_UsernameField.text))
            {
                m_Error.text = "No username has been entered!";
                return false;
            }

            Username = m_UsernameField.text;
            return true;
        }
    }
    
    public override void OnConnectedToMaster()
    {
        Debug.Log("CONNECTED");
        m_Connecting.gameObject.SetActive(false);
        m_JoinOrCreateRoom.SetActive(true);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("DISCONNECTED");
    }
    
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log($"Error joining room: {returnCode}, {message}");
        
        m_Error.text = $"Couldn't find room {m_RoomCodeField.text}!";
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"Player {newPlayer.ActorNumber} joined!");
        
        base.OnPlayerEnteredRoom(newPlayer);
        PlayerCount.Value = PhotonNetwork.PlayerList.Length;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"Player {otherPlayer.ActorNumber} left :(");
        base.OnPlayerLeftRoom(otherPlayer);
        PlayerCount.Value = PhotonNetwork.PlayerList.Length;
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"Joined room {RoomCode.Value}");
        
        PlayerCount.Value = PhotonNetwork.PlayerList.Length;
        m_JoinOrCreateRoom.SetActive(false);
        m_LeaveButton.gameObject.SetActive(true);
        
        if (PhotonNetwork.IsMasterClient)
            m_StartButton.gameObject.SetActive(true);
        
        m_Game.JoinRoom();
    }

    public override void OnLeftRoom()
    {
        Debug.Log($"Left room {RoomCode.Value}");
        
        base.OnLeftRoom();
        RoomCode.Value = string.Empty;
        PlayerCount.Value = 0;
        m_JoinOrCreateRoom.SetActive(true);
        m_LeaveButton.gameObject.SetActive(false);
        m_StartButton.gameObject.SetActive(false);
        m_Error.text = string.Empty;
        
        m_Game.LeaveRoom();
    }
    
    private static string RandomString(int length)
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[Random.Next(s.Length)]).ToArray());
    }
}
