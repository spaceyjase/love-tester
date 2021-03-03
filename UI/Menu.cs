using Godot;
using System;
using System.Linq;

public class Menu : Control
{
  [Signal] private delegate void Shown();
  [Signal] private delegate void Hidden();
  [Signal] private delegate void PlayButtonClicked();
  [Signal] private delegate void OptionButtonClicked();

  private AudioStreamPlayer2D ButtonClick => GetNode<AudioStreamPlayer2D>(nameof(ButtonClick));
  private AnimationPlayer ButtonAnimationPlayer => GetNode<AnimationPlayer>(nameof(ButtonAnimationPlayer));
  
  private bool displayed = true; // displayed by default, i.e. when Ready

  private LoveTester.UI.Button[] menuButtons;
  private int currentButton;

  public override void _Ready()
  {
    base._Ready();

    menuButtons = new[]
    {
      GetNode<LoveTester.UI.Button>("PlayButton"),
      GetNode<LoveTester.UI.Button>("OptionsButton")
    };
  }

  public override void _Process(float delta)
  {
    if (!displayed) return;
    
    menuButtons[currentButton % menuButtons.Length].SetFocus();
    if (Input.IsActionPressed(Global.MenuButton))
    {
      // TODO: Start indicator tween
    }

    if (Input.IsActionJustReleased(Global.MenuButton))
    {
      // TODO: cancel indicator tween
      menuButtons[currentButton++ % menuButtons.Length].RemoveFocus();
    }
  }

  private void OnButtonAnimationPlayerFinished(string name)
  {
    var signal = name == "play_slide_in" ? nameof(Shown) : nameof(Hidden);
    EmitSignal(signal);
  }

  public async void Display()
  {
    if (displayed) return;
    ButtonAnimationPlayer.Play("play_slide_in");
    await ToSignal(this, nameof(Shown));
    displayed = true;
  }

  public async void Conceal()
  {
    if (!displayed) return;
    ButtonAnimationPlayer.Play("play_slide_out");
    await ToSignal(this, nameof(Hidden));
    displayed = false;
    Array.ForEach(menuButtons, b => b.RemoveFocus());
  }

  public void OnButtonPressed(string name)
  {
    ButtonClick.Play();
    
    var signal = string.Empty;
    switch (name)
    {
      case "PlayButton":
        signal = nameof(PlayButtonClicked);
        break;
      case "OptionsButton":
        signal = nameof(OptionButtonClicked);
        break;
      default:
        return;
    }
    EmitSignal(signal);
  }

  private void OnFocusEntered(string name)
  {
    for (var n = 0; n < menuButtons.Length; ++n)
    {
      menuButtons[n].RemoveFocus();
      if (menuButtons[n].Name == name) currentButton = n;
    }
  }
}
