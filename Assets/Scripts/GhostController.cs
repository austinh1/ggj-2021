using System;
using Photon.Pun;
using UnityEngine;

public class GhostController : MonoBehaviour, IPlayerMovement
{
    [Range(0.0f, 10.0f)]
    public float speed = 5f;

    [Range(0.0f, 20.0f)]
    public float dashSpeed = 10f;

    [Range(0, 10)]
    public int dashCooldownSeconds = 3;
    
    [Range(0, 10)]
    public float accelSpeed = 5f;

    private Rigidbody2D _rigidbody2D;
    private PlayerController _playerController;
    private PhotonView _photonView;
    private NetworkPlayer _networkPlayer;
    private PossessObject _possessObject;
    private TimeSpan _dashCooldown = new TimeSpan(0);
    private Vector2 _dashDir;
    private float _dashBoost = 0f;
    
    private Observable<bool> _isFacingLeft = new Observable<bool>();
    
    public bool IsConverting { get; set; }
    
    private PhotonView PhotonView
    {
        get
        {
            if (_photonView == null)
                _photonView = GetComponent<PhotonView>();

            return _photonView;
        }
    }
    private PlayerController PlayerController
    {
        get
        {
            if (_playerController == null)
                _playerController = GetComponent<PlayerController>();
    
            return _playerController;
        }
    }
    private Rigidbody2D Rigidbody2D
    {
        get
        {
            if (_rigidbody2D == null)
                _rigidbody2D = GetComponent<Rigidbody2D>();
    
            return _rigidbody2D;
        }
    }
    private NetworkPlayer NetworkPlayer
    {
        get
        {
            if (_networkPlayer == null)
                _networkPlayer = GetComponent<NetworkPlayer>();

            return _networkPlayer;
        }
    }
    private PossessObject PossessObject
    {
        get
        {
            if (_possessObject == null)
                _possessObject = GetComponent<PossessObject>();

            return _possessObject;
        }
    }
    
    private bool IsLocal => PhotonView.IsMine;
    private void Start()
    {
        IsConverting = false;
        _isFacingLeft.OnChange.AddListener(SetFacingDirection);
    }
    
    void Update()
    {
        var inputX = Input.GetAxis("Horizontal Ghost");
        var inputY = Input.GetAxis("Vertical Ghost");
        
        var boost = new Vector2(_dashBoost, _dashBoost) * _dashDir;
        var move = new Vector2(inputX, inputY).normalized * speed;
        var multiplier = IsConverting ? 0 : (PossessObject.IsPossessing ? 0.5f : 1f);
        var desiredVelocity = (move + boost) * new Vector2(multiplier, multiplier);
        Rigidbody2D.velocity = Vector3.Lerp(Rigidbody2D.velocity, desiredVelocity, accelSpeed * Time.deltaTime);

        // Only allow dashing while moving, it's not on cooldown, and not possessing something
        if (Input.GetKeyDown(KeyCode.Space) && _dashCooldown.Ticks <= 0 && (inputX != 0 || inputY != 0) && !PossessObject.IsPossessing)
        {
            _dashDir = new Vector2(Math.Sign(inputX), Math.Sign(inputY)).normalized;
            _dashBoost = dashSpeed;
            _dashCooldown = new TimeSpan(0, 0, dashCooldownSeconds);
        }
        
        // Decelerate dash
        if (_dashBoost > 0)
            _dashBoost -= dashSpeed * Time.deltaTime;
        else
            _dashBoost = 0f;
        
        if (_dashCooldown > TimeSpan.Zero)
            _dashCooldown -= TimeSpan.FromSeconds(Time.deltaTime);
        

        if (Rigidbody2D.velocity.x != 0)
            _isFacingLeft.Value = Rigidbody2D.velocity.x < 0;
        
    }
    
    void SetFacingDirection(bool oldVal, bool newVal)
    {
        PlayerController.SetFlipX(newVal);

        if (IsLocal)
            PhotonView.RPC(nameof(PlayerController.FaceDirectionRPC), RpcTarget.Others, PhotonView.Owner, newVal);
    }

    public void SetEnabled(bool value)
    {
        enabled = value;
    }

    public void GetSlapped(bool fromBehind)
    {
        PlayerController.PlaySlapSound();

        if (PossessObject.IsPossessing)
        {
            // Knock them out!
            PossessObject.StopPossessing(true);
        }
        else
        {
            // Slap them back to humanity!
            PlayerController.PlayerAnimator.SetBool("FromBehind", fromBehind);
            PlayerController.PlayerAnimator.SetTrigger("Slapped");
            IsConverting = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!enabled || PossessObject.IsPossessing || NetworkPlayer.Game.CurrentState != Game.GameState.InProgress)
            return;
        
        if (other.CompareTag("Key"))
            NetworkPlayer.SendGotKeyMessage(other.gameObject);
        else if (other.CompareTag("Sandwich"))
            NetworkPlayer.SendGotSandwichMessage();
    }
}