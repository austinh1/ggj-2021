using JetBrains.Annotations;
using Photon.Pun;
using UnityEngine;

public class PossessObject : MonoBehaviour
{
    private PossessionObject NearestPossessionObject { get; set; }
    private PossessionObject CurrentPossessionObject { get; set; }
    private Sprite OriginalSprite { get; set; }
    private bool IsPossessing { get; set; }
    private float MinPossessionDistance { get; } = 1f;
    private int PossessedObjectIndex { get; set; } = -1;

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

    private void Start()
    {
        OriginalSprite = PlayerSprite.sprite;
    }

    private void CheckForPossessionObjects()
    {
        NearestPossessionObject = null;
        var closestDistance = 100f;
        foreach (var possessionObject in PossessionManager.instance.PossessionObjects)
        {
            float distanceFromPlayer = Vector3.Distance(possessionObject.transform.position, transform.position);

            if (!(distanceFromPlayer < MinPossessionDistance) || !(distanceFromPlayer < closestDistance)) continue;

            closestDistance = distanceFromPlayer;
            NearestPossessionObject = possessionObject;
        }

        foreach (var possessionObject in PossessionManager.instance.PossessionObjects)
        {
            if(possessionObject != NearestPossessionObject)
                possessionObject.SpriteRenderer.color = Color.white;
            else
                NearestPossessionObject.SpriteRenderer.color = Color.cyan;
        }
    }

    public void Update()
    {
        if(!IsPossessing) CheckForPossessionObjects();

        if (!Input.GetKeyDown(KeyCode.LeftShift)) return;

        if (!IsPossessing)
        {
            if (NearestPossessionObject != null)
            {
                PossessNearestObject();
            }
        }
        else
        {
            StopPossessing();
        }
    }

    private void StopPossessing()
    {
        CurrentPossessionObject.transform.position = transform.position;
        CurrentPossessionObject.SpriteRenderer.enabled = true;
        PlayerSprite.sprite = OriginalSprite;
        IsPossessing = false;
        
        if (IsLocal)
            PhotonView.RPC(nameof(StopPossessObjectRPC), RpcTarget.OthersBuffered, PhotonView.Owner, PossessedObjectIndex, OriginalSprite);

        PossessedObjectIndex = -1;
    }

    private void PossessNearestObject()
    {
        PossessedObjectIndex = PossessionManager.instance.PossessionObjects.IndexOf(CurrentPossessionObject);
        CurrentPossessionObject = NearestPossessionObject;
        NearestPossessionObject.SpriteRenderer.enabled = false;
        PlayerSprite.sprite = CurrentPossessionObject.SpriteRenderer.sprite;
        IsPossessing = true;
        
        if (IsLocal)
            PhotonView.RPC(nameof(PossessObjectRPC), RpcTarget.OthersBuffered, PhotonView.Owner, PossessedObjectIndex);
    }
    
    
    [PunRPC]
    [UsedImplicitly]
    public void PossessObjectRPC(PlayerController player, int possessedObjectIndex)
    {
        if (PhotonView.Owner.Equals(player))
        {
            PossessionObject pObject = PossessionManager.instance.PossessionObjects[possessedObjectIndex];
            pObject.SpriteRenderer.enabled = false;
            PlayerSprite.sprite = pObject.SpriteRenderer.sprite;
        }
    }
    
    [PunRPC]
    [UsedImplicitly]
    public void StopPossessObjectRPC(PlayerController player, int possessedObjectIndex, Sprite originalSprite)
    {
        if (PhotonView.Owner.Equals(player))
        {
            PossessionObject pObject = PossessionManager.instance.PossessionObjects[possessedObjectIndex];
            pObject.transform.position = player.transform.position;
            pObject.SpriteRenderer.enabled = true;
            PlayerSprite.sprite = originalSprite;
        }
    }
    
    
}
