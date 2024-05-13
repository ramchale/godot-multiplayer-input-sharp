using System.Collections.Generic;
using System.Linq;
using Godot;
using MultiplayerInputSharp;

public partial class PlayerManager : Node {
  // player is 0-3
  // device is -1 for keyboard/mouse, 0+ for joypads
  // these concepts seem similar but it is useful to separate them so for example, device 6 could control player 1.

  [Signal]
  public delegate void PlayerJoinedEventHandler(int player);

  [Signal]
  public delegate void PlayerLeftEventHandler(int player);

  /// <summary>
  /// map from player integer to dictionary of data
  /// the existence of a key in this dictionary means this player is joined.
  /// use GetPlayerData() and SetPlayerData() to use this dictionary.
  /// </summary>
  private Dictionary<int, Dictionary<StringName, Variant>> _playerData = new();

  private const int MAX_PLAYERS = 8;

  public void Join(int device) {
    var player = NextPlayer();
    if (player >= 0) {
      // initialize default player data here
      // "team" and "car" are remnants from my game just to provide an example
      _playerData[player] = new Dictionary<StringName, Variant>();
      _playerData[player].Add("device", device);
      _playerData[player].Add("team", 0);
      _playerData[player].Add("car", "muscle");

      EmitSignal(SignalName.PlayerJoined, player);
    }
  }

  public void Leave(int player) {
    if (_playerData.ContainsKey(player)) {
      _playerData.Remove(player);
      EmitSignal(SignalName.PlayerLeft, player);
    }
  }

  public int GetPlayerCount() {
    return _playerData.Count;
  }

  public int[] GetPlayerIndexes() {
    return _playerData.Keys.ToArray();
  }

  public int GetPlayerDevice(int player) {
    return (int)_playerData[player]["device"];
  }

  // null means it doesn't exist.
  public Variant? GetPlayerData(int player, StringName key)
  {
    if (_playerData.ContainsKey(player) && _playerData[player].ContainsKey(key)) {
      return _playerData[player][key];
    }

    return null;
  }

  public void SetPlayerData(int player, StringName key, Variant value) {
    // if this player is not joined, don't do anything
    if (!_playerData.ContainsKey(player)) {
      return;
    }

    _playerData[player][key] = value;
  }

  /// <summary>
  /// call this from a loop in the main menu or anywhere they can join
  /// this is an example of how to look for an action on all devices
  /// </summary>
  public void HandleJoinInput() {
    foreach (var device in GetUnjoinedDevices()) {
      if (MultiplayerInput.IsActionJustPressed(device, "join")) {
        Join(device);
      }
    }
  }

  /// <summary>
  /// <para>
  /// to see if anybody is pressing the "start" action
  /// this is an example of how to look for an action on all players
  /// note the difference between this and handleJoinInput(). players vs devices.
  /// </para>
  /// </summary>
  /// <returns>True if any player has just pressed start</returns>
  public bool SomeoneWantsToStart() {
    foreach (var player in _playerData.Keys) {
      var device = GetPlayerDevice(player);

      if (MultiplayerInput.IsActionJustPressed(device, "start"))
      {
      	return true;
      }
    }

    return false;
  }

  public bool IsDeviceJoined(int device) {
    return _playerData.Where((pair => (int)pair.Value["device"] == device)).Any();
  }

  /// <returns>
  /// returns a valid player integer for a new player.
  /// returns -1 if there is no room for a new player.
  /// </returns>
  private int NextPlayer() {
    for (var i = 0; i < MAX_PLAYERS; i++) {
      if (!_playerData.ContainsKey(i)) {
        return i;
      }
    }

    return -1;
  }

  /// <returns>returns an array of all valid devices that are *not* associated with a joined player</returns>
  public int[] GetUnjoinedDevices() {
    var devices = Input.GetConnectedJoypads();
    // also consider keyboard player
    devices.Add(-1);

    // filter out devices that are joined:
    return devices.Where(key => !IsDeviceJoined(key)).ToArray();
  }
}
