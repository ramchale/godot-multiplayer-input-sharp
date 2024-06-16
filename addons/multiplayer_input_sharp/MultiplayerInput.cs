namespace MultiplayerInputSharp;

using System;
using Godot;
using System.Collections.Generic;
using Godot.Collections;

/// <summary>A globally accessible manager for device-specific actions.
/// <para>
/// This class automatically duplicates relevant events on all actions for new joypads
/// when they connect and disconnect.
/// It also provides a nice API to access all the normal "Input" methods,
/// but using the device integers and the same action names.
/// All methods in this class that have a "device" parameter can accept -1
/// which means the keyboard device.
/// NOTE: The -1 device will not work on Input methods because it is a specific
/// concept to this MultiplayerInput class.
///
/// See DeviceInput for an object-oriented way to get input for a single device.
/// </para>
/// </summary>
public partial class MultiplayerInput : Node {
  ///<summary>An array of all the non-duplicated action names</summary>
  private static Godot.Collections.Array<StringName> _coreActions;

  /// <summary>A dictionary of all action names
  /// <para>
  /// The keys are the device numbers. the values are a dictionary that maps action name to device action name
  /// for example device_actions[device][action_name] is the device-specific action name
  /// the purpose of this is to cache all the StringNames of all the actions
  /// ... so it doesn't need to generate them every time
  /// </para>
  /// </summary>
  private static System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<string, string>>
    _deviceActions = new ();

  /// Array of GUIDs - If a device with an ignored GUID is detected, no input actions will be added.
  private static List<string> _ignoredGuids = new ();

  static MultiplayerInput() {
    if (Engine.IsEditorHint()) {
      return;
    }

    Reset();
  }

  private static void Reset() {
    InputMap.LoadFromProjectSettings();
    _coreActions = InputMap.GetActions();

    foreach (var action in _coreActions) {
      foreach (var e in InputMap.ActionGetEvents(action)) {
        if (IsJoypadEvent(e) && !IsUiAction(action)) {
          e.Device = 8;
        }
      }
    }

    // create actions for gamepads that connect in the future and also clean up when gamepads disconnect
    Input.JoyConnectionChanged += ((device, connected) => OnJoyConnectionChanged((int)device, connected));
  }

  private static void OnJoyConnectionChanged(int device, bool connected) {
    if (connected) {
      CreateActionsForDevice(device);
    }
    else {
      DeleteActionsForDevice(device);
    }
  }

  private static void CreateActionsForDevice(int device) {
    // skip action creation if the device should be ignored
    if (!_ignoredGuids.Contains(Input.GetJoyGuid(device))) {
      _deviceActions[device] = new System.Collections.Generic.Dictionary<string, string>();

      foreach (var coreAction in _coreActions) {
        var newAction = device + coreAction;
        var deadZone = InputMap.ActionGetDeadzone(coreAction);

        // get all joypad events for this action
        var unfilteredEvents = InputMap.ActionGetEvents(coreAction);

        var events = new Array<InputEvent>();

        foreach (var inputEvent in unfilteredEvents) {
          if (IsJoypadEvent(inputEvent)) {
            events.Add(inputEvent);
          }
        }

        // only copy this event if it is relevant to joypads
        if (events.Count > 0) {
          // first add the action with the new name
          InputMap.AddAction(newAction, deadZone);
          _deviceActions[device][coreAction] = newAction;

          // then copy all the events associated with that action
          // this only includes events that are relevant to joypads
          foreach (var inputEvent in events) {
            // without duplicating, all of them have a reference to the same event object
            // which doesn't work because this has to be unique to this device
            var newEvent = (InputEvent)inputEvent.Duplicate(true);
            newEvent.Device = device;

            // switch the device to be just this joypad
            InputMap.ActionAddEvent(newAction, newEvent);
          }
        }
      }
    }
  }

  private static void DeleteActionsForDevice(int device) {
    _deviceActions.Remove(device);
    var actionsToErase = new List<StringName>();
    var deviceNumStr = device.ToString();

    // figure out which actions should be erased
    foreach (var action in InputMap.GetActions()) {
      var actionString = action.ToString();
      var maybeDevice = actionString.Substring(0, deviceNumStr.Length);
      if (maybeDevice == deviceNumStr) {
        actionsToErase.Add(action);
      }
    }

    // now actually erase them
    // this is done separately so I'm not erasing from the collection I'm looping on
    // not sure if this is necessary but whatever, this is safe
    foreach (var action in actionsToErase) {
      InputMap.EraseAction(action);
    }
  }

  /**
   * use these functions to query the action states just like normal Input functions
   */

  /// <summary>This is equivalent to Input.GetActionRawStrength except it will only check the relevant device.</summary>
  public static float GetActionRawStrength(int device, StringName action, bool exactMatch = false) {
    if (device > 0) {
      action = GetActionName(device, action);
    }

    return Input.GetActionRawStrength(action, exactMatch);
  }

  /// <summary>This is equivalent to Input.GetActionStrength except it will only check the relevant device.</summary>
  public static float GetActionStrength(int device, StringName action, bool exactMatch = false) {
    if (device >= 0) {
      action = GetActionName(device, action);
    }

    return Input.GetActionStrength(action, exactMatch);
  }

  /// <summary>This is equivalent to Input.GetAxis except it will only check the relevant device.</summary>
  public static float GetAxis(int device, StringName negativeAction, StringName positiveAction) {
    if (device >= 0) {
      negativeAction = GetActionName(device, negativeAction);
      positiveAction = GetActionName(device, positiveAction);
    }

    return Input.GetAxis(negativeAction, positiveAction);
  }

  /// <summary>This is equivalent to Input.GetVector except it will only check the relevant device.</summary>
  public static Vector2 GetVector(int device, StringName negativeX, StringName negativeY, StringName positiveX,
    StringName positiveY, float deadzone = -1f) {
    if (device >= 0) {
      negativeX = GetActionName(device, negativeX);
      positiveX = GetActionName(device, positiveX);
      negativeY = GetActionName(device, negativeY);
      positiveY = GetActionName(device, positiveY);
    }

    return Input.GetVector(negativeX, negativeY, positiveX, positiveY, deadzone);
  }


  /// <summary>This is equivalent to Input.IsActionJustPressed except it will only check the relevant device.</summary>
  public static bool IsActionJustPressed(int device, StringName action, bool exactMatch = false) {
    if (device >= 0) {
      action = GetActionName(device, action);
    }

    return Input.IsActionJustPressed(action, exactMatch);
  }


  /// <summary>This is equivalent to Input.IsActionJustReleased except it will only check the relevant device.</summary>
  public static bool IsActionJustReleased(int device, StringName action, bool exactMatch = false) {
    if (device >= 0) {
      action = GetActionName(device, action);
    }

    return Input.IsActionJustReleased(action, exactMatch);
  }

  /// <summary>This is equivalent to Input.IsActionPressed except it will only check the relevant device.</summary>
  public static bool IsActionPressed(int device, StringName action, bool exactMatch = false) {
    if (device >= 0) {
      action = GetActionName(device, action);
    }

    return Input.IsActionPressed(action, exactMatch);
  }

  /// <returns>Returns the name of a gamepad-specific action</returns>
  private static StringName GetActionName(int device, StringName action) {
    if (device >= 0) {
      // if it says this dictionary doesn't have the key,
      // that could mean it's an invalid action name.
      // or it could mean that action doesn't have a joypad event assigned
      if (!_deviceActions.ContainsKey(device)) {
        throw new Exception($"Device {device} has no actions. Maybe the joypad is disconnected.");
      }

      return _deviceActions[device][action];
    }

    // return the normal action name for the keyboard player
    return action;
  }

  /// <summary>Restricts actions that start with "ui_" to only work on a single device.</summary>
  /// <remarks>NOTE: this calls reset(), so if you make any changes to the InputMap via code, you'll need to make them again.</remarks>
  /// <param name="device">
  /// Pass a -2 to reset it back to default behavior, to allow all devices to trigger "ui_" actions.
  /// For example, pass a -1 if you want only the keyboard/mouse device to control menus.
  /// </param>
  public static void SetUiActionDevice(int device) {
    // First, totally re - create the InputMap for all devices
    // This is necessary because this function may have messed up the UI Actions
    // ... on a previous call
    Reset();

    // We are back to default behavior.
    // So if that's what the caller wants, we're done!
    if (device == -2) {
      return;
    }

    // find all ui actions and erase irrelevant events
    foreach (var action in InputMap.GetActions()) {
      // ignore non-ui-actions
      if (!IsUiAction(action)) {
        break;
      }

      if (device == -1) {
        // in this context, we want to erase all joypad events
        foreach (var e in InputMap.ActionGetEvents(action)) {
          if (IsJoypadEvent(e)) {
            InputMap.ActionEraseEvent(action, e);
          }
        }
      }
      else {
        // in this context, we want to delete all non-joypad events.
        // and we also want to set the event's device to the given device.
        foreach (var e in InputMap.ActionGetEvents(action)) {
          if (IsJoypadEvent(e)) {
            e.Device = device;
          }
          else {
            // this isn't event a joypad event, so erase it entirely
            InputMap.ActionEraseEvent(action, e);
          }
        }
      }
    }
  }

  /// <returns>Returns true if the given event is a joypad event.</returns>
  private static bool IsJoypadEvent(InputEvent inputEvent) =>
    inputEvent is InputEventJoypadButton or InputEventJoypadMotion;

  /// <returns>
  /// Returns true if this is a UI action.
  /// Which basically just means it starts with "ui_".
  /// But you can override this in your project if you want.
  /// </returns>
  private static bool IsUiAction(StringName actionName) => actionName.ToString().StartsWith("ui_");
}
