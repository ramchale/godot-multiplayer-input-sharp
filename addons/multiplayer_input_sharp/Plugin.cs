#if TOOLS
using Godot;

namespace MultiplayerInputSharp;

[Tool]
public partial class Plugin : EditorPlugin {
  private const string AUTOLOAD_NAME = "MultiplayerInputSharp";

  public override void _EnterTree() {
    base._EnterTree();
    AddAutoloadSingleton(AUTOLOAD_NAME, "res://addons/multiplayer_input_sharp/MultiplayerInput.cs");
  }

  public override void _ExitTree() {
    RemoveAutoloadSingleton(AUTOLOAD_NAME);
    base._ExitTree();
  }
}
#endif
