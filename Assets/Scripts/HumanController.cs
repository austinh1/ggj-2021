using System;
using UnityEngine;

public class HumanController : MonoBehaviour, IPlayerMovement
{
    private Rigidbody2D rigidbody2D;
    private PlayerController playerController;

    [Range(0.0f, 10.0f)]
    public float speed = 7f;

    [Range(0.0f, 10.0f)]
    public float slapRange = 2f;

    private void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        float inputX = Input.GetAxis("Horizontal Human");
        float inputY = Input.GetAxis("Vertical Human");

        playerController.PlayerAnimator.SetBool("Walking", inputX != 0 || inputY != 0);

        var direction = new Vector3(inputX, inputY);

        rigidbody2D.velocity = direction.normalized * speed;

        if (Input.GetKeyDown(KeyCode.F))
        {
            playerController.PlayerAnimator.SetTrigger("Slapping");
            Slap();
        }
    }

    public void SetEnabled(bool value)
    {
        enabled = value;
    }

    private void Slap()
    {
        Debug.Log("Attempt slap...");
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

        if (nearestPlayer != null)
        {
            var netPlayer = nearestPlayer.GetComponent<NetworkPlayer>();
            Debug.Log(String.Format("You slapped {0}!", netPlayer.Username.Value));

            var slappedRigidBody = nearestPlayer.GetComponent<Rigidbody2D>();
            // Determine whether the player was behind the ghost or not and adjust sprite accordingly
            // This actually gets the direction they are moving rather than "facing". We don't have a facing direction right now.
            var slappedFacingDir = slappedRigidBody.velocity.x != 0 ? Math.Sign(slappedRigidBody.velocity.x) : 1;
            var dirToSlapped = Math.Sign(nearestPlayer.transform.position.x - transform.position.x);
            bool fromBehind = slappedFacingDir != dirToSlapped;
            nearestPlayer.PlayerAnimator.SetBool("FromBehind", fromBehind);
            nearestPlayer.PlayerAnimator.SetTrigger("Slapped");
        }
    }
}