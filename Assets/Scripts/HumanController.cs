using System;
using UnityEngine;

public class HumanController : MonoBehaviour, IPlayerMovement
{
    private Rigidbody2D rigidbody2D;

    public Sprite[] sprites;

    [Range(0.0f, 10.0f)]
    public float speed = 7f;

    [Range(0.0f, 10.0f)]
    public float slapRange = 3f;

    private void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float inputX = Input.GetAxis("Horizontal Human");
        float inputY = Input.GetAxis("Vertical Human");

        rigidbody2D.velocity = new Vector2(speed * inputX, speed * inputY);

        if (Input.GetKeyDown(KeyCode.F))
        {
            Slap();
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

        if (nearestPlayer != null)
        {
            var netPlayer = nearestPlayer.GetComponent<NetworkPlayer>();
            Debug.Log(String.Format("You slapped {0}!", netPlayer.Username));
            nearestPlayer.MakeIntoHuman();
        }
    }
}