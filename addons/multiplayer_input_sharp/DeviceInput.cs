namespace MultiplayerInputSharp;

using Godot;

public partial class DeviceInput : RefCounted {
  [Signal]
  public delegate void ConnectionChangedEventHandler(bool connected);

  private int _device;
  private bool _isConnected = true;

  public DeviceInput(int deviceNum) {
    _device = deviceNum;
    Input.JoyConnectionChanged += (device, connected) => OnJoyConnectionChanged((int)device, connected);
  }

  /// <returns>Returns true if this device is the keyboard/mouse "device"</returns>
  public bool IsKeyboard() {
    return _device < 0;
  }

  /// <returns>Returns true if this device is a joypad.</returns>
  public bool IsJoypad() {
    return _device >= 0;
  }

  /// <seealso cref="Input.GetJoyGuid"/>
  /// <returns>If this is the keyboard device, this returns "Keyboard"</returns>
  public string GetGuid() {
    if (IsKeyboard()) {
      return "Keyboard";
    }

    return Input.GetJoyGuid(_device);
  }

  /// <seealso cref="Input.GetJoyName"/>
  /// <returns>If this is the keyboard device, this returns "Keyboard"</returns>
  public string GetName() {
    if (IsKeyboard()) {
      return "Keyboard";
    }

    return Input.GetJoyName(_device);
  }

  /// <seealso cref="Input.GetJoyVibrationDuration"/>
  /// <returns>This will always be 0.0 for the keyboard device.</returns>
  public float GetVibrationDuration() {
    if (IsKeyboard()) {
      return 0.0f;
    }

    return Input.GetJoyVibrationDuration(_device);
  }

  /// <seealso cref="Input.GetJoyVibrationStrength"/>
  /// <returns>This will always be Vector2.ZERO for the keyboard device.</returns>
  public Vector2 get_vibration_strength() {
    if (IsKeyboard()) {
      return Vector2.Zero;
    }

    return Input.GetJoyVibrationStrength(_device);
  }

  /// <seealso cref="Input.IsJoyKnown"/>
  /// <returns>This will always return true for the keyboard device.</returns>
  public bool IsKnown() {
    if (IsKeyboard()) {
      return true;
    }

    return Input.IsJoyKnown(_device);
  }

  /// <seealso cref="Input.StartJoyVibration"/>
  /// <returns>This does nothing for the keyboard device.</returns>
  public void StartVibration(float weakMagnitude, float strongMagnitude, float duration = 0.0f) {
    if (IsKeyboard()) {
      return;
    }

    Input.StartJoyVibration(_device, weakMagnitude, strongMagnitude, duration);
  }

  /// <seealso cref="Input.StopJoyVibration"/>
  /// <returns>This does nothing for the keyboard device.</returns>
  public void StopVibration() {
    if (IsKeyboard()) {
      return;
    }

    Input.StopJoyVibration(_device);
  }

  /// <summary>This is equivalent to Input.GetActionRawStrength except it will only check the relevant device.</summary>
  public float GetActionRawStrength(StringName action, bool exactMatch = false) {
    if (!_isConnected) {
      return 0.0f;
    }

    return MultiplayerInput.GetActionRawStrength(_device, action, exactMatch);
  }

  /// <summary>This is equivalent to Input.GetActionStrength except it will only check the relevant device.</summary>
  public float GetActionStrength(StringName action, bool exactMatch = false) {
    if (!_isConnected) {
      return 0.0f;
    }

    return MultiplayerInput.GetActionStrength(_device, action, exactMatch);
  }

  /// <summary>This is equivalent to Input.GetAxis except it will only check the relevant device.</summary>
  public float GetAxis(StringName negativeAction, StringName positiveAction) {
    if (!_isConnected) {
      return 0.0f;
    }

    return MultiplayerInput.GetAxis(_device, negativeAction, positiveAction);
  }

  /// <summary>This is equivalent to Input.GetVector except it will only check the relevant device.</summary>
  public Vector2 GetVector(StringName negativeX, StringName positiveX, StringName negativeY, StringName positiveY,
    float deadzone = -1.0f) {
    if (!_isConnected) {
      return Vector2.Zero;
    }

    return MultiplayerInput.GetVector(_device, negativeX, positiveX, negativeY, positiveY, deadzone);
  }

  /// <summary>This is equivalent to Input.IsActionJustPressed except it will only check the relevant device.</summary>
  public bool IsActionJustPressed(StringName action, bool exactMatch = false) {
    if (!_isConnected) {
      return false;
    }

    return MultiplayerInput.IsActionJustPressed(_device, action, exactMatch);
  }

  /// <summary>This is equivalent to Input.isActionJustReleased except it will only check the relevant device.</summary>
  public bool IsActionJustReleased(StringName action, bool exactMatch = false) {
    if (!_isConnected) {
      return false;
    }

    return MultiplayerInput.IsActionJustReleased(_device, action, exactMatch);
  }

  /// <summary>This is equivalent to Input.isActionPressed except it will only check the relevant device.</summary>
  public bool IsActionPressed(StringName action, bool exactMatch = false) {
    if (!_isConnected) {
      return false;
    }

    return MultiplayerInput.IsActionPressed(_device, action, exactMatch);
  }

  /// <summary>Takes exclusive control over all "ui_" actions.</summary>
  /// <seealso cref="MultiplayerInput.SetUiActionDevice"/>
  public void TakeUiActions() {
    if (!_isConnected) {
      return;
    }

    MultiplayerInput.SetUiActionDevice(_device);
  }

  /// <summary>Internal method that is called whenever any device is connected or disconnected.
  /// <para>This is how this object keeps its "isConnected" property updated.</para>
  /// </summary>
  public void OnJoyConnectionChanged(int device, bool connected) {
    if (device == _device) {
      EmitSignal(SignalName.ConnectionChanged, connected);
      _isConnected = connected;
    }
  }
}
