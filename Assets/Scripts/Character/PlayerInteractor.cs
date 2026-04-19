using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Interact")]
    public float interactRadius = 1.2f;
    public LayerMask interactLayer;

    public void TryInteract()
    {
        Collider2D hit = Physics2D.OverlapCircle(
            transform.position,
            interactRadius,
            interactLayer
        );

        if (hit == null)
        {
            return;
        }

        Interactable interactable = hit.GetComponent<Interactable>();
        if (interactable != null)
        {
            interactable.Interact();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
