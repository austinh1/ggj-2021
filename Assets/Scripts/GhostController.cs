using System;
using UnityEngine;

public class GhostController : MonoBehaviour, IPlayerMovement
{
    private Rigidbody2D rigidbody2D;
    private PlayerController playerController;
    private SpriteRenderer renderer;
    private TimeSpan dashCooldown = new TimeSpan(0);
    private Vector2 dashDir;
    private float dashBoost = 0f;

    [Range(0.0f, 10.0f)]
    public float speed = 5f;

    [Range(0.0f, 20.0f)]
    public float dashSpeed = 10f;

    [Range(0, 10)]
    public int dashCooldownSeconds = 3;
    
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
        renderer = transform.Find("GhostSprite").GetComponent<SpriteRenderer>();
    }
    
    void Update()
    {
        float inputX = Input.GetAxis("Horizontal Ghost");
        float inputY = Input.GetAxis("Vertical Ghost");
        
        var boost = new Vector2(dashBoost, dashBoost) * dashDir;
        var move = new Vector2(inputX, inputY).normalized * speed;
        rigidbody2D.velocity = move + boost;

        // Only allow dashing while moving and it's not on cooldown
        if (Input.GetKeyDown(KeyCode.Space) && dashCooldown.Ticks <= 0 && (inputX != 0 || inputY != 0))
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
            renderer.flipX = rigidbody2D.velocity.x < 0;
        }
    }

    public void SetEnabled(bool value)
    {
        enabled = value;
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