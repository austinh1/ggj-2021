using System;
using UnityEngine;

public class GhostController : MonoBehaviour, IPlayerMovement
{
    private Rigidbody2D rigidbody2D;
    private PlayerController playerController;
    private TimeSpan dashCooldown = new TimeSpan(0);
    private Vector2 dashDir;
    private float dashBoost = 0f;

    public bool IsConverting { get; private set; }

    [Range(0.0f, 10.0f)]
    public float speed = 5f;

    [Range(0.0f, 20.0f)]
    public float dashSpeed = 10f;

    [Range(0, 10)]
    public int dashCooldownSeconds = 3;
    
    [Range(0, 10)]
    public float accelSpeed = 5f;
    
    private NetworkPlayer m_NetworkPlayer;

    private NetworkPlayer NetworkPlayer
    {
        get
        {
            if (m_NetworkPlayer == null)
                m_NetworkPlayer = GetComponent<NetworkPlayer>();

            return m_NetworkPlayer;
        }
    }
    
    private PossessObject m_PossessObject;

    private PossessObject PossessObject
    {
        get
        {
            if (m_PossessObject == null)
                m_PossessObject = GetComponent<PossessObject>();

            return m_PossessObject;
        }
    }
    
    private void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();
        IsConverting = false;
    }
    
    void Update()
    {
        float inputX = Input.GetAxis("Horizontal Ghost");
        float inputY = Input.GetAxis("Vertical Ghost");
        
        var boost = new Vector2(dashBoost, dashBoost) * dashDir;
        var move = new Vector2(inputX, inputY).normalized * speed;
        var multiplier = (PossessObject.IsPossessing ? 0.5f : 1f);
        var desiredVelocity = (move + boost) * new Vector2(multiplier, multiplier);
        rigidbody2D.velocity = Vector3.Lerp(rigidbody2D.velocity, desiredVelocity, accelSpeed * Time.deltaTime);

        // Only allow dashing while moving, it's not on cooldown, and not possessing something
        if (Input.GetKeyDown(KeyCode.Space) && dashCooldown.Ticks <= 0 && (inputX != 0 || inputY != 0) && !PossessObject.IsPossessing)
        {
            dashDir = new Vector2(Math.Sign(inputX), Math.Sign(inputY)).normalized;
            dashBoost = dashSpeed;
            dashCooldown = new TimeSpan(0, 0, dashCooldownSeconds);
        }
        
        // Decelerate dash
        if (dashBoost > 0)
        {
            dashBoost -= dashSpeed * Time.deltaTime;
        }
        else
        {
            dashBoost = 0f;
        }
        
        if (dashCooldown > TimeSpan.Zero)
        {
            dashCooldown -= TimeSpan.FromSeconds(Time.deltaTime);
        }

        if (rigidbody2D.velocity.x != 0)
        {
            playerController.SetFlipX(rigidbody2D.velocity.x < 0);
        }
    }

    public void SetEnabled(bool value)
    {
        enabled = value;
    }

    public void GetSlapped(bool fromBehind)
    {
        playerController.PlayerAnimator.SetBool("FromBehind", fromBehind);
        playerController.PlayerAnimator.SetTrigger("Slapped");
        IsConverting = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!enabled || PossessObject.IsPossessing || NetworkPlayer.Game.CurrentState != Game.GameState.InProgress)
            return;
        
        if (other.CompareTag("Key"))
        {
            NetworkPlayer.SendGotKeyMessage(other.gameObject);
        }
        else if (other.CompareTag("Sandwich"))
        {
            NetworkPlayer.SendGotSandwichMessage();
        }
    }
}