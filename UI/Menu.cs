using Godot;
using System;

public class Menu : Control
{
  [Signal] private delegate void Shown();
  [Signal] private delegate void Hidden();

  private AudioStreamPlayer2D ButtonClick => GetNode<AudioStreamPlayer2D>(nameof(ButtonClick));
  private AnimationPlayer ButtonAnimationPlayer => GetNode<AnimationPlayer>(nameof(ButtonAnimationPlayer));

  private void OnButtonAnimationPlayerFinished(string name)
  {
    var signal = name == "play_slide_in" ? nameof(Shown) : nameof(Hidden);
    EmitSignal(signal);
  }

  public async void Display()
  {
    ButtonAnimationPlayer.Play("play_slide_in");
    await ToSignal(this, nameof(Shown));
  }

  public async void Conceal()
  {
    ButtonAnimationPlayer.Play("play_slide_out");
    await ToSignal(this, nameof(Hidden));
  }

  public void OnButtonPressed(string name)
  {
    ButtonClick.Play();
    // TODO: Emit appropriate signal - insert coin or options
  }
}
