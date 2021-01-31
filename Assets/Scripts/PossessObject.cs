using JetBrains.Annotations;
using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine;

public class PossessObject : MonoBehaviour
{
    private PossessionObject NearestPossessionObject { get; set; }
    private PossessionObject CurrentPossessionObject { get; set; }
    private Sprite OriginalSprite { get; set; }
    public bool IsPossessing { get; set; }
    private float MinPossessionDistance { get; } = 1f;
    private int PossessedObjectIndex { get; set; } = -1;

    // This is the time before they can possess after being slapped out of an object
    public int possessCooldownMilliseconds { get; set; } = 1000;
    private TimeSpan possessCooldown = new TimeSpan(0);

    private SpriteRenderer _playerSprite;

    private SpriteRenderer PlayerSprite
    {
        get
        {
            if (_playerSprite == null) _playerSprite = GetComponentInChildren<SpriteRenderer>();

            return _playerSprite;
        }
    }
    
    private PhotonView m_PhotonView;

    private PhotonView PhotonView
    {
        get
        {
            if (m_PhotonView == null)
                m_PhotonView = GetComponent<PhotonView>();

            return m_PhotonView;
        }
    }
    
    private bool IsLocal => PhotonView.IsMine;

    private PlayerController _playerController;

    private PlayerController PlayerController
    {
        get
        {
            if (_playerController == null) _playerController = GetComponent<PlayerController>();

            return _playerController;
        }
    }
    
    private NetworkPlayer m_NetworkPlayer;

    private NetworkPlayer NetworkPlayer
    {
        get
        {
            if (m_NetworkPlayer == null) m_NetworkPlayer = GetComponent<NetworkPlayer>();

            return m_NetworkPlayer;
        }
    }
    
    private void Start()
    {
        OriginalSprite = PlayerSprite.sprite;
    }

    private void CheckForPossessionObjects()
    {
        NearestPossessionObject = null;
        var closestDistance = 100f;
        foreach (var possessionObject in PossessionManager.instance._possessionObjects)
        {
            float distanceFromPlayer = Vector3.Distance(possessionObject.transform.position, transform.position);

            if (!(distanceFromPlayer < MinPossessionDistance) || !(distanceFromPlayer < closestDistance)) continue;

            closestDistance = distanceFromPlayer;
            NearestPossessionObject = possessionObject;
        }

        foreach (var possessionObject in PossessionManager.instance._possessionObjects)
        {
            if (possessionObject != NearestPossessionObject)
                possessionObject.SpriteRenderer.color = Color.white;

            if(NearestPossessionObject != null)
                NearestPossessionObject.SpriteRenderer.color = Color.cyan;
        }
    }

    public void Update()
    {
        if (!IsLocal || NetworkPlayer.Game.CurrentState != Game.GameState.InProgress) return;

        if (possessCooldown > TimeSpan.Zero)
        {
            possessCooldown -= TimeSpan.FromSeconds(Time.deltaTime);
        }

        if (!IsPossessing) CheckForPossessionObjects();

        if (!Input.GetKeyDown(KeyCode.F)) return;

        if (!IsPossessing)
        {
            if (NearestPossessionObject != null && possessCooldown <= TimeSpan.Zero)
            {
                PossessNearestObject();
            }
        }
        else
        {
            StopPossessing();
        }
    }

    public void StopPossessing(bool useCooldown = false)
    {
        PlayerController.PlayerAnimator.enabled = true;
        if (CurrentPossessionObject != null)
        {
            CurrentPossessionObject.transform.position = transform.position;
            CurrentPossessionObject.SpriteRenderer.enabled = true;
        }
        PlayerSprite.sprite = OriginalSprite;
        IsPossessing = false;
        NetworkPlayer.ShowUsername();

        if (IsLocal)
        {
            PhotonView.RPC(nameof(StopPossessObjectRPC), RpcTarget.Others, PhotonView.Owner, PossessedObjectIndex);
            if (useCooldown)
            {
                possessCooldown = new TimeSpan(possessCooldownMilliseconds * 10000); // 1 tick = 10,0000 ms
            }
        }

        PossessedObjectIndex = -1;
    }

    private void PossessNearestObject()
    {
        PlayerController.PlayerAnimator.enabled = false;
        CurrentPossessionObject = NearestPossessionObject;
        PossessedObjectIndex = PossessionManager.instance._possessionObjects.IndexOf(CurrentPossessionObject);
        CurrentPossessionObject.SpriteRenderer.enabled = false;
        PlayerSprite.sprite = CurrentPossessionObject.SpriteRenderer.sprite;
        PlayerSprite.flipX = false;
        IsPossessing = true;
        NetworkPlayer.HideUsername();

        if (IsLocal)
            PhotonView.RPC(nameof(PossessObjectRPC), RpcTarget.Others, PhotonView.Owner, PossessedObjectIndex);
    }
    
    [PunRPC]
    [UsedImplicitly]
    public void PossessObjectRPC(Player player, int possessedObjectIndex)
    {
        if (!PhotonView.Owner.Equals(player)) return;

        PlayerController.PlayerAnimator.enabled = false;
        PossessionObject pObject = PossessionManager.instance._possessionObjects[possessedObjectIndex];
        pObject.SpriteRenderer.enabled = false;
        PlayerSprite.sprite = pObject.SpriteRenderer.sprite;
        PlayerSprite.flipX = false;
        IsPossessing = true;
        NetworkPlayer.HideUsername();
    }
    
    [PunRPC]
    [UsedImplicitly]
    public void StopPossessObjectRPC(Player player, int possessedObjectIndex)
    {
        if (!PhotonView.Owner.Equals(player)) return;

        PlayerController.PlayerAnimator.enabled = true;
        PossessionObject pObject = PossessionManager.instance._possessionObjects[possessedObjectIndex];
        pObject.transform.position = transform.position;
        pObject.SpriteRenderer.enabled = true;
        PlayerSprite.sprite = PlayerController.PlayerSprite;
        IsPossessing = false;
        NetworkPlayer.ShowUsername();
    }
}
