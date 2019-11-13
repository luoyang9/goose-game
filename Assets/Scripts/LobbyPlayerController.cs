using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class LobbyPlayerController : MonoBehaviour
{
    public void Start()
    {
        Debug.Log("started " + this.name);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        //Debug.Log("moved");
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        Debug.Log("Click");
    }

    public void OnSubmit(InputAction.CallbackContext context)
    {
        Debug.Log("Submit");
    }

    public void OnCancel(InputAction.CallbackContext context)
    {
        Debug.Log("cancel");
    }

    public void OnDeviceLost(PlayerInput context)
    {
        Debug.Log("lost device");
    }

    public void OnDeviceRegained(PlayerInput context)
    {
        Debug.Log("regained device");
    }
}
