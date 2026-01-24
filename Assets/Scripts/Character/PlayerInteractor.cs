using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    public float interactRadius = 1.2f;
    public LayerMask interactLayer;

    // Трогаем траву :3
    public void TryInteract()
    {
        Collider2D hit = Physics2D.OverlapCircle(
            transform.position,
            interactRadius,
            interactLayer
        );

        if (hit != null)
        {
            Interactable interactable = hit.GetComponent<Interactable>();
            if (interactable != null)
            {
                interactable.Interact();
            }
        }
    }

    // Область взаимодейстия, идна только в дебаге

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
