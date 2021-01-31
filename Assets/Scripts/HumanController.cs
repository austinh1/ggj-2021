using System;
using UnityEngine;

public class HumanController : MonoBehaviour, IPlayerMovement
{
    private Rigidbody2D rigidbody2D;
    private PlayerController playerController;
    private SpriteRenderer renderer;

    [Range(0.0f, 10.0f)]
    public float speed = 7f;

    [Range(0.0f, 10.0f)]
    public float slapRange = 2f;

    public AudioClip slapSound;
    public GameObject slapEffectPrefab;
    
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

    private void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();
        renderer = transform.Find("HumanSprite").GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        float inputX = Input.GetAxis("Horizontal Human");
        float inputY = Input.GetAxis("Vertical Human");

        playerController.PlayerAnimator.SetBool("Walking", inputX != 0 || inputY != 0);

        var direction = new Vector3(inputX, inputY);

        rigidbody2D.velocity = direction.normalized * speed;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.F))
        {
            playerController.PlayerAnimator.SetTrigger("Slapping");
            
            if (NetworkPlayer.Game.CurrentState == Game.GameState.InProgress)
                Slap();
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

    private void Slap()
    {
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
            if (playerScript.IsGhost && distanceFromPlayer <= Math.Min(closestDistance, slapRange))
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
            nearestPlayer.PlayerAnimator.SetBool("FromBehind", fromBehind);
            nearestPlayer.PlayerAnimator.SetTrigger("Slapped");

            // Play slap sound effect and create visual
            var audioSource = GetComponent<AudioSource>();
            audioSource.PlayOneShot(slapSound);
            Instantiate(slapEffectPrefab, nearestPlayer.transform.position + new Vector3(0.6f, 0.6f, 0), Quaternion.identity);
        }
    }
}