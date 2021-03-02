using Godot;
using System;

public class Menu : Control
{
  [Signal] private delegate void Shown();

  private AnimationPlayer AnimationPlayer => GetNode<AnimationPlayer>(nameof(AnimationPlayer));

  private void OnButtonAnimationPlayerFinished(string name)
  {
    GD.Print($"completed: {name}");
  }
}
