using UnityEngine;

public class Game : MonoBehaviour
{
    private GameState CurrentState { get; set; }

    public void JoinRoom()
    {
        CurrentState = GameState.Setup;
    }

    public void LeaveRoom()
    {
        CurrentState = GameState.Lobby;
    }
    
    public enum GameState
    {
        Lobby,
        Setup,
        InProgress,
        Complete
    }
}
