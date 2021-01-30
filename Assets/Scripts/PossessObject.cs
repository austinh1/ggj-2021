using UnityEngine;

public class PossessObject : MonoBehaviour
{
    private PossessionObject NearestPossessionObject { get; set; }
    private PossessionObject CurrentPossessionObject { get; set; }
    private Sprite OriginalSprite { get; set; }
    private bool IsPossessing { get; set; }
    private float MinPossessionDistance { get; set; } = 1f;

    private SpriteRenderer _playerSprite;

    private SpriteRenderer PlayerSprite
    {
        get
        {
            if (_playerSprite == null) _playerSprite = GetComponentInChildren<SpriteRenderer>();

            return _playerSprite;
        }
    }

    private void Start()
    {
        OriginalSprite = PlayerSprite.sprite;
    }

    public void CheckForPossessionObjects()
    {
        NearestPossessionObject = null;
        float closestDistance = 100f;
        foreach (var possessionObject in PossessionManager.instance.PossessionObjects)
        {
            float distanceFromPlayer = Vector3.Distance(possessionObject.transform.position, transform.position);

            if (!(distanceFromPlayer < MinPossessionDistance) || !(distanceFromPlayer < closestDistance)) continue;

            closestDistance = distanceFromPlayer;
            NearestPossessionObject = possessionObject;
        }
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (!IsPossessing)
            {
                CheckForPossessionObjects();

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
    }

    private void StopPossessing()
    {
        CurrentPossessionObject.transform.position = transform.position;
        CurrentPossessionObject.SpriteRenderer.enabled = true;
        PlayerSprite.sprite = OriginalSprite;
        IsPossessing = false;
    }

    private void PossessNearestObject()
    {
        CurrentPossessionObject = NearestPossessionObject;
        NearestPossessionObject.SpriteRenderer.enabled = false;
        PlayerSprite.sprite = CurrentPossessionObject.SpriteRenderer.sprite;
        IsPossessing = true;
    }
}
