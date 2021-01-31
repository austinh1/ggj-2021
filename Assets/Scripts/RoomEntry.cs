using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class RoomEntry : MonoBehaviour
{
    [SerializeField] private TMP_Text _roomName;
    [SerializeField] private TMP_Text _playerCount;
    [SerializeField] private TMP_Text _maxPlayers;

    [SerializeField] private Button _joinButton;

    public void Start()
    {
        _joinButton.onClick.AddListener(delegate
            {
                GameObject.Find("Main Menu").GetComponent<MainMenu>().JoinRoom(_roomName.text);
            });
    }
    
    public void Init(string name, int playerCount, int maxPlayers)
    {
        _roomName.text = name;
        _playerCount.text = playerCount.ToString();
        _maxPlayers.text = maxPlayers.ToString();

        if (playerCount >= maxPlayers){
            _joinButton.interactable = false;
            _joinButton.GetComponentInChildren<TMP_Text>().text = "FULL";
        }
    }
}
