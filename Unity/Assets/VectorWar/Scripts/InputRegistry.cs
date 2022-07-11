using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using SepM.DataTypes;

/// <summary>
/// Class <c>InputRegistry</c> Stores game controls and provides helper methods for reading inputs
/// </summary>
public static class InputRegistry {
    public static string INPUT_UP_NM = "up";
    public static string INPUT_DOWN_NM = "down";
    public static string INPUT_LEFT_NM = "left";
    public static string INPUT_RIGHT_NM = "right";
    public static uint INPUT_UP = (1 << 0);
    public static uint INPUT_DOWN = (1 << 1);
    public static uint INPUT_LEFT = (1 << 2);
    public static uint INPUT_RIGHT = (1 << 3);
    public static uint INPUT_UP_THIS_FRAME = (1 << 100);
    public static uint INPUT_DOWN_THIS_FRAME = (1 << 101);
    public static uint INPUT_LEFT_THIS_FRAME = (1 << 102);
    public static uint INPUT_RIGHT_THIS_FRAME = (1 << 103);
    public static List<PlayerControl> p_controls = new List<PlayerControl>();
    public static List<(string, string)> DEFAULT_P1_KEYBOARD_BUTTON_MAPPING = new List<(string, string)>(){
        (INPUT_UP_NM,"w"),
        (INPUT_DOWN_NM,"s"),
        (INPUT_LEFT_NM,"a"),
        (INPUT_RIGHT_NM,"d"),
    };

    public static List<(string, string)> DEFAULT_P2_KEYBOARD_BUTTON_MAPPING = new List<(string, string)>(){
        (INPUT_UP_NM,"up"),
        (INPUT_DOWN_NM,"down"),
        (INPUT_LEFT_NM,"left"),
        (INPUT_RIGHT_NM,"right"),
    };

    /// <summary>
    /// Takes the given device, mappings, and player, and addes new corresponding PlayerControls to the registry
    /// </summary>
    public static void AddControlsToMap(InputDevice device, List<(string, string)> mappings, int player) {
        foreach ((string, string) mapping in mappings) {
            string mappingName = mapping.Item1;
            string mappingValue = mapping.Item2;
            p_controls.Add(new PlayerControl(device, player, mappingName, mappingValue));
        }
        p_controls = p_controls.Distinct().ToList<PlayerControl>();
    }

    /// <summary>
    /// Checks if the given input was pressed (checks if there's a 1 at the input's position in the "bit list" current inputs)
    /// </summary>
    public static bool CheckInput(long currentInputs, uint input) {
        return (currentInputs & input) != 0;
    }

    /// <summary>
    /// Checks if any mapping for the given input was pressed by the given player.
    /// </summary>
    public static bool CheckPlayerInput(string input, int player) {
        List<PlayerControl> playerControls = FindPlayerControlsByInput(input, player);
        bool result = playerControls.Any(c => GetInput(c));
        return result;
    }


    /// <summary>
    /// Grabs any mapping of the given input for the given player
    /// </summary>
    public static List<PlayerControl> FindPlayerControlsByInput(string input, int player) {
        List<PlayerControl> result = p_controls.FindAll(
            c => c.getInput() == input && c.getPlayer() == player
        ).ToList();
        return result;
    }

    /// <summary>
    /// Given a device, and a mapping, checks if the input is currently pressed
    /// </summary>
    public static InputControl GetButton(InputDevice device, string mapping) {
        InputControl button = null;
        if (device is Gamepad || device is Keyboard) {
            try { button = device.GetChildControl(mapping); }
            catch (Exception e) {
                Debug.LogError(string.Format("Invalid {0} control: {1}", device is Gamepad ? "gamepad" : "keyboard", mapping));
                Debug.LogError(e);
            }
        }
        return button;
    }

    /// <summary>
    /// Checks if the given PlayerControl is currently pressed
    /// </summary>
    public static bool GetInput(PlayerControl c) {
        //Check if the device is null
        if (NullDeviceCheck(c)) { return false; }

        bool result = false; //Store the result here
        ButtonControl button;

        button = (ButtonControl)GetButton(c.getDevice(), c.getMapping());

        //Check if we could locate the button
        if (button is null) { return false; }

        result = button.isPressed;
        return result;
    }

    /// <summary>
    /// Returns true if the passed device in the PlayerControl is null.
    /// </summary>
    public static bool NullDeviceCheck(PlayerControl c) {
        if (c.getDevice() == null || !c.getDevice().enabled) {
            Debug.LogWarning(
                string.Format("Input device is not active or existent for control {0} for player {1}!",
                    c.getInput(), c.getPlayer()));
            return true;
        }
        return false;
    }
}
