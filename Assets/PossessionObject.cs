
using UnityEngine;

public class PossessionObject : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;

    public SpriteRenderer SpriteRenderer
    {
        get
        {
            if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();

            return _spriteRenderer;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
