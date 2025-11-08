using UnityEngine;
using static Controls;
using UnityEngine.InputSystem;
using System;

[CreateAssetMenu(fileName = "New Input Reader", menuName = "Input/Input Reader")]

public class InputReader : ScriptableObject, IPlayerActions
{
    private Controls controls;
    #region Events
    public event Action<bool> OnFireEvent;
    public event Action<Vector3> OnMoveEvent;
    public event Action<bool> OnAimEvent;
    #endregion


    private void OnEnable()
    {
        if (controls == null)
        {
            controls = new Controls();
            controls.Player.SetCallbacks(this);
        } 

        controls.Player.Enable();
    }

    private void OnDisable()
    {
        controls.Player.Disable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        OnMoveEvent?.Invoke(context.ReadValue<Vector3>());
    }

    public void OnFirePrimary(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnFireEvent?.Invoke(true);
        }
        else if (context.canceled)
        {
            OnFireEvent?.Invoke(false);
        }
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnAimEvent?.Invoke(true);
        }
        else if (context.canceled)
        {
            OnAimEvent?.Invoke(false);
        }
    }

}
