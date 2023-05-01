using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;

    public static PlayerInput PlayerInput;

    public bool MenuOpenCloseInput { get; private set; }
    public bool UIMenuInputClose { get; private set; }

    private InputAction _menuOpenCloseAction;

    private InputAction _UIMenuCloseAction;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        PlayerInput = GetComponent<PlayerInput>();
        _menuOpenCloseAction = PlayerInput.actions["MenuOpenClose"];
        _UIMenuCloseAction = PlayerInput.actions["MenuOpenCloseUI"];
    }

    private void Update()
    {
        MenuOpenCloseInput = _menuOpenCloseAction.WasPressedThisFrame();

        UIMenuInputClose = _UIMenuCloseAction.WasPressedThisFrame();
    }
}