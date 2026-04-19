using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private PlayerMovement _movement;
    private PlayerInteractor _interactor;

    private void Awake()
    {
        _movement = GetComponent<PlayerMovement>();
        _interactor = GetComponent<PlayerInteractor>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (_interactor != null)
            {
                _interactor.TryInteract();
            }
        }

        if (_movement == null)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            _movement.Crouch(true);
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            _movement.Crouch(false);
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            _movement.SpeedUp(true);
        }

        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            _movement.SpeedUp(false);
        }
    }
}
