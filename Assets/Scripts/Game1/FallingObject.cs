using UnityEngine;

public enum FallingObjectType
{
    Good,
    Bad
}

public class FallingObject : MonoBehaviour
{
    #region Variables
    [Header("Object Settings")]
    [SerializeField] private FallingObjectType _type;
    [SerializeField] private int _scoreValue = 1;
    [SerializeField] private float _fallSpeed = 3f;

    [Header("Destroy Settings")]
    [SerializeField] private float _destroyY = -6f;

    #endregion
    
    private void Update()
    {
        if (CatchGameManager.Instance != null && CatchGameManager.Instance.GameEnded) return;

        transform.Translate(Vector3.down * _fallSpeed * Time.deltaTime); // Update object position to fall downwards

        if (transform.position.y <= _destroyY) Destroy(gameObject); // Destroy the object if it goes below the destroy threshold
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return; // Ignore collisions with non-player objects

        // If there's a collision with the player
        if (_type == FallingObjectType.Good) // And the object is good
        {
            CatchGameManager.Instance.AddScore(_scoreValue); // Increase the score
        }
        else // If the object is bad
        {
            CatchGameManager.Instance.LoseLife(); // Decrease lives
        }

        Destroy(gameObject); // Destroy the object 
    }
}