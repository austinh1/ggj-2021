using System;
using Photon.Pun;
using UnityEngine;

public class HumanController : MonoBehaviour, IPlayerMovement
{
    [Range(0.0f, 10.0f)]
    public float speed = 7f;

    [Range(0.0f, 10.0f)]
    public float slapRange = 2f;

    public GameObject slapEffectPrefab;
    
    private Rigidbody2D _rigidbody2D;
    private PlayerController _playerController;
    private PhotonView _photonView;
    private NetworkPlayer _networkPlayer;
    
    private Observable<bool> _isWalking = new Observable<bool>();
    private Observable<bool> _isSlapPressed = new Observable<bool>();
    private Observable<bool> _isFacingLeft = new Observable<bool>();

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
    
    private bool IsLocal => PhotonView.IsMine;

    private void Start()
    {
        _isWalking.OnChange.AddListener(SetWalkAnimation);
        _isSlapPressed.OnChange.AddListener(SetSlapAnimation);
        _isFacingLeft.OnChange.AddListener(SetFacingDirection);
    }

    void Update()
    {
        if (NetworkPlayer.Game.SettingUp)
            return;
        
        var inputX = Input.GetAxis("Horizontal Human");
        var inputY = Input.GetAxis("Vertical Human");

        _isWalking.Value = inputX != 0 || inputY != 0;

        var direction = new Vector3(inputX, inputY);

        Rigidbody2D.velocity = direction.normalized * speed;
        
        _isSlapPressed.Value = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.F);
        var isSlapAnimating = PlayerController.PlayerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Slapping");
        if (isSlapAnimating)
            CheckSlap();

        if (Rigidbody2D.velocity.x != 0)
            _isFacingLeft.Value = Rigidbody2D.velocity.x < 0;

    }

    public void SetEnabled(bool value)
    {
        enabled = value;
    }

    private void CheckSlap()
    {
        // Don't allow hitting things before game starts. Only allow a single hit per slap animation.
        if (NetworkPlayer.Game.CurrentState != Game.GameState.InProgress || PlayerController.PlayerAnimator.GetBool("SlapHitTarget"))
            return;

        PlayerController nearestPlayer = null;
        var closestDistance = float.PositiveInfinity;
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in players)
        {
            // You can't slap yourself! I mean you could... But it wouldn't exactly do anything. So stop being silly.
            if (player == this.gameObject)
            {
                continue;
            }

            PlayerController playerScript = player.GetComponent<PlayerController>();

            float distanceFromPlayer = Vector3.Distance(player.transform.position, transform.position);
            if (playerScript.IsGhost && !player.GetComponent<GhostController>().IsConverting && distanceFromPlayer <= Math.Min(closestDistance, slapRange))
            {
                closestDistance = distanceFromPlayer;
                nearestPlayer = playerScript;
            }
        }

        if (nearestPlayer != null )
        {
            var netPlayer = nearestPlayer.GetComponent<NetworkPlayer>();
            Debug.Log(String.Format("You slapped {0}!", netPlayer.Username.Value));

            var slappedSpriteRenderer = nearestPlayer.transform.Find("GhostSprite").GetComponent<SpriteRenderer>();
            // Determine whether the player was behind the ghost or not and adjust sprite accordingly
            var slappedFacingDir = slappedSpriteRenderer.flipX ? -1 : 1;
            var dirToSlapped = Math.Sign(nearestPlayer.transform.position.x - transform.position.x);
            bool fromBehind = slappedFacingDir == dirToSlapped;
            
            var ghostController = nearestPlayer.GetComponent<GhostController>();
            ghostController.GetSlapped(fromBehind);
            ghostController.SendGetSlappedMessage(fromBehind);
            PlayerController.PlayerAnimator.SetBool("SlapHitTarget", true);

            // Play slap sound effect and create visual
            PlayerController.PlaySlapSound();
            Instantiate(slapEffectPrefab, nearestPlayer.transform.position + new Vector3(0.6f, 0.6f, 0), Quaternion.identity);
        }
    }
    
    void SetFacingDirection(bool oldVal, bool newVal)
    {
        PlayerController.SetFlipX(newVal);

        if (IsLocal)
            PhotonView.RPC(nameof(PlayerController.FaceDirectionRPC), RpcTarget.Others, PhotonView.Owner, newVal);
    }

    void SetWalkAnimation(bool oldVal, bool newVal)
    {
        PlayerController.PlayerAnimator.SetBool("Walking", newVal);
        
        if (IsLocal)
            PhotonView.RPC(nameof(PlayerController.WalkAnimationRPC), RpcTarget.Others, PhotonView.Owner, newVal);
    }
    
    void SetSlapAnimation(bool oldVal, bool newVal)
    {
        if (PlayerController.PlayerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Slapping"))
            return;

        PlayerController.PlayerAnimator.SetBool("SlapHitTarget", false);
        PlayerController.PlayerAnimator.SetTrigger("Slapping");

        if (IsLocal)
            PhotonView.RPC(nameof(PlayerController.SlapAnimationRPC), RpcTarget.Others, PhotonView.Owner);
    }

    
}