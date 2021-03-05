using Godot;

namespace LoveTester.UI
{
  public class Button : TextureButton
  {
    [Signal] private delegate void ButtonPressed(string buttonName);
    [Signal] private delegate void FocusEntered(string buttonName);
    [Signal] private delegate void FocusLost(string buttonName);

    [Export] private float destinationOffset = 50f;
    [Export] private float holdTime = 1f;

    private TextureRect arrowIndicator;
    private Tween indicatorTween;

    private const float initialTimer = 0.25f;
    private float timer;

    private Vector2 indicatorStartPosition;
    private Vector2 indicatorEndPosition;
    private bool tweenCompleted;

    private Texture textureNormal;
    private Texture texturePressed;

    public override void _Ready()
    {
      base._Ready();
      arrowIndicator = GetChild<TextureRect>(0);
      indicatorStartPosition = arrowIndicator.RectPosition;
      indicatorEndPosition = indicatorStartPosition + new Vector2(destinationOffset, 0f);
      Connect("focus_entered", this, nameof(OnFocusEntered));
      
      indicatorTween = GetChild<Tween>(1);
      indicatorTween.Connect("tween_completed", this, nameof(OnIndicatorTweenCompleted));
      
      textureNormal = TextureNormal;
      texturePressed = TexturePressed;
    }

    public override void _Process(float delta)
    {
      base._Process(delta);

      if (!arrowIndicator.Visible) return;
      
      if (Input.IsActionJustPressed(Global.MenuButton))
      {
        timer = 0f;
      }

      if (tweenCompleted)
      { // button was 'held' to press... wait until release...
        TextureNormal = texturePressed;
        if (!Input.IsActionJustReleased(Global.MenuButton)) return;
        TextureNormal = textureNormal;
        OnButtonPressed();
        return;
      }

      if (timer > initialTimer && Input.IsActionPressed(Global.MenuButton) && !indicatorTween.IsActive())
      {
        indicatorTween.InterpolateProperty(arrowIndicator, "rect_position", indicatorStartPosition,
          indicatorEndPosition, holdTime);
        indicatorTween.Start();
      }

      if (Input.IsActionJustReleased(Global.MenuButton))
      {
        StopTweening();
        EmitSignal(nameof(FocusLost), Name);
      }
      
      timer += delta;
    }

    private void OnButtonPressed()
    {
      StopTweening();
      EmitSignal(nameof(ButtonPressed), Name);
    }

    private void OnFocusEntered()
    {
      SetFocus();
      EmitSignal(nameof(FocusEntered), Name);
    }
    
    private void OnIndicatorTweenCompleted(object o, NodePath key)
    {
      tweenCompleted = true;
    }
    
    public void SetFocus()
    {
      if (arrowIndicator.Visible) return;
      StopTweening();
      arrowIndicator.Visible = true;
    }

    private void StopTweening()
    {
      indicatorTween.StopAll();
      indicatorTween.ResetAll();
      arrowIndicator.RectPosition = indicatorStartPosition;
      tweenCompleted = false;
    }

    public void RemoveFocus()
    {
      if (!arrowIndicator.Visible) return;
      arrowIndicator.Visible = false;
      StopTweening();
      ReleaseFocus();
    }
  }
}