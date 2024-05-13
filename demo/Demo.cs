using System;
using System.Collections.Generic;
using Godot;

public partial class Demo : Node {
  /// <summary>
  /// this is a singleton autoload in my project but for the purposes of this demo,
  /// this is simpler
  /// </summary>
  private PlayerManager _playerManager;

  /// <summary>map from player integer to the player node</summary>
  private Dictionary<int, Node> _playerNodes = new ();

  public override void _Ready() {
    _playerManager = GetNode<PlayerManager>("PlayerManager");
    _playerManager.PlayerJoined += player => SpawnPlayer(player);
    _playerManager.PlayerLeft += player => DeletePlayer(player);
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(double delta) {
    _playerManager.HandleJoinInput();
  }

  private void SpawnPlayer(int player) {
    var playerScene = GD.Load<PackedScene>("res://demo/demo_player.tscn");
    var playerNode = playerScene.Instantiate<DemoPlayer>();
    playerNode.Leave += (player) => { OnPlayerLeave(player); };
    _playerNodes[player] = playerNode;

    // let the player know which device controls it
    var device = _playerManager.GetPlayerDevice(player);
    playerNode.Init(player, device);

    // add the player to the tree
    AddChild(playerNode);

    // random spawn position
    playerNode.Position = new Vector2((Random.Shared.NextSingle() * 350f) + 50f, (Random.Shared.NextSingle() * 350f) + 50f);
  }

  private void DeletePlayer(int player) {
    _playerNodes[player].QueueFree();
    _playerNodes.Remove(player);
  }

  public void OnPlayerLeave(int player) {
    // just let the player manager know this player is leaving
    // this will, through the player manager's "player_left" signal,
    // indirectly call delete_player because it's connected in this file's _ready()
    _playerManager.Leave(player);
  }
}
