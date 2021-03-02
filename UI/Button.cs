using Godot;

namespace LoveTester.UI
{
  public class Button : TextureButton
  {
    [Signal] private delegate void ButtonPressed(string buttonName);
    
    [Export] private int downOffset = 3;

    private Label buttonLabel;
    private Vector2 buttonUpPosition;
    private Vector2 buttonDownPosition;
  
    public override void _Ready()
    {
      base._Ready();

      foreach (var child in GetChildren())
      {
        if (!(child is Label childLabel)) continue;
      
        buttonLabel = childLabel;
        buttonUpPosition = buttonLabel.RectPosition;
        
        buttonDownPosition = buttonUpPosition;
        buttonDownPosition.y = buttonUpPosition.y + downOffset;
      
        break;
      }
    }

    public void OnButtonDown()
    {
      buttonLabel.RectPosition = buttonDownPosition;
    }
  
    public void OnButtonUp()
    {
      buttonLabel.RectPosition = buttonUpPosition;
      EmitSignal(nameof(ButtonPressed), Name);
    }
  }
}