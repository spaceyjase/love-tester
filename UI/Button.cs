using Godot;

namespace LoveTester.UI
{
  public class Button : TextureButton
  {
    [Signal] private delegate void ButtonPressed(string buttonName);

    private TextureRect arrowIndicator;

    public override void _Ready()
    {
      base._Ready();
      arrowIndicator = GetChild<TextureRect>(0);
    }

    public void OnButtonPressed()
    {
      EmitSignal(nameof(ButtonPressed), Name);
    }

    public void SetFocus()
    {
      if (arrowIndicator.Visible) return;
      arrowIndicator.Visible = true;
    }
    
    public void RemoveFocus()
    {
      if (!arrowIndicator.Visible) return;
      arrowIndicator.Visible = false;
    }
  }
}