using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private PlayerMovement movement;
    private PlayerInteractor interactor;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        interactor = GetComponent<PlayerInteractor>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            bool hacked = interactor != null && interactor.TryHackNearestEnemy();
            if (!hacked && interactor != null)
            {
                interactor.TryInteract();
            }
        }

        if (movement == null)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            movement.Crouch(true);
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            movement.Crouch(false);
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            movement.SpeedUp(true);
        }

        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            movement.SpeedUp(false);
        }
    }
}
