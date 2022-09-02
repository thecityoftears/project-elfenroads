using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

// from https://forum.unity.com/threads/onmousedown-with-new-input-system.955053/
public class MouseReader : MonoBehaviour
{
    private Vector2 mousePos = new Vector2();
    public UnityEngine.UI.GraphicRaycaster raycaster;

    public void ReadMouse(InputAction.CallbackContext ctx)
    {

        if (EventSystem.current.currentSelectedGameObject != null)
            return;

        if (ctx.phase == InputActionPhase.Performed &&
            Physics.Raycast(
                Camera.main.ScreenPointToRay(
                    Mouse.current.position.ReadValue()),
                out RaycastHit hit))
        {
            MouseDown mouseDown = hit.collider.gameObject.GetComponent<MouseDown>();

            mouseDown?.OnMouseDown();
        }
    }
}