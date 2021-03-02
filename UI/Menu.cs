using Godot;
using System;

public class Menu : Control
{
  [Signal] private delegate void Shown();
  [Signal] private delegate void Hidden();
  [Signal] private delegate void PlayButtonClicked();
  [Signal] private delegate void OptionButtonClicked();

  private AudioStreamPlayer2D ButtonClick => GetNode<AudioStreamPlayer2D>(nameof(ButtonClick));
  private AnimationPlayer ButtonAnimationPlayer => GetNode<AnimationPlayer>(nameof(ButtonAnimationPlayer));
  
  private bool displayed = true; // displayed by default, i.e. when Ready

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
}
