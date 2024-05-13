using Godot;
using MultiplayerInputSharp;

public partial class DemoPlayer : Node2D {
  [Signal]
  public delegate void LeaveEventHandler(int player);

  private int _player;
  private DeviceInput _input;

  public void Init(int playerNum, int device) {
    _player = playerNum;

    // in my project, I got the device integer by accessing the singleton autoload PlayerManager
    // but for simplicity, it's not an autoload in this demo.
    // but I recommend making it a singleton so you can access the player data from anywhere.
    // that would look like the following line, instead of the device function parameter above.
    //    device = PlayerManager.GetPlayerDevice(_player);
    _input = new DeviceInput(device);

    GetNode<Label>("Player").Text = playerNum.ToString();
  }

	public override void _Process(double delta) {
    var move = _input.GetVector("move_left", "move_right", "move_up", "move_down");
    Position += move;

    // let the player leave by pressing the "join" button
    if (_input.IsActionJustPressed("join")) {
      // an alternative to this is just call PlayerManager.leave(player)
      // but that only works if you set up the PlayerManager singleton
      EmitSignal(SignalName.Leave, _player);
    }
  }
}
