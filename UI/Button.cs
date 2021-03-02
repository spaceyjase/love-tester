using Godot;

namespace LoveTester.UI
{
  public class Button : TextureButton
  {
    [Signal] private delegate void ButtonPressed(string buttonName);

    public void OnButtonPressed()
    {
      EmitSignal(nameof(ButtonPressed), Name);
    }
  }
}