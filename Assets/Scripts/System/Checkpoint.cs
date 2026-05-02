using System;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint;
    
    public Vector3 RespawnPosition => respawnPoint != null ? respawnPoint.position : transform.position;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ActivateCheckpoint();
            Destroy(gameObject);
        }
    }

    private void ActivateCheckpoint()
    {
        GameStateManager gameState = GameStateManager.Instance;
        if (gameState == null)
        {
            Debug.LogWarning("GameStateManager не найден в сцене.");
            return;
        }

        gameState.UpdateCheckpoint(this);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(RespawnPosition, 0.3f);
        Gizmos.DrawLine(transform.position, RespawnPosition);
    }
}