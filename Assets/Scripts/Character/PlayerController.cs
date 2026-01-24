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

    // Взаимодействеи, бег и приседание, по базе
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            interactor.TryInteract();
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
