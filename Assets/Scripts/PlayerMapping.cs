using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class PlayerMapping
{
    public string PlayerTag { get; private set; }
    public InputDevice[] Controller { get; private set; }
    public GameObject Character { get; set; }
    public bool Active { get; set; }

    public PlayerMapping(string tag, InputDevice[] controller, GameObject character)
    {
        PlayerTag = tag;
        Controller = controller;
        Character = character;
        Active = true;
    }

    public override string ToString()
    {
        return $"tag: {PlayerTag}, Character: {Character}, Active: {Active}";
    }
}
