using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Button = LoveTester.UI.Button;

public class Menu : Control
{
  [Signal] private delegate void Shown();
  [Signal] private delegate void Hidden();
  [Signal] private delegate void OnButtonClicked(string buttonName);

  [Export] private string[] buttonNames;

  private AudioStreamPlayer2D ButtonClick => GetNode<AudioStreamPlayer2D>(nameof(ButtonClick));
  private AnimationPlayer ButtonAnimationPlayer => GetNode<AnimationPlayer>(nameof(ButtonAnimationPlayer));
  
  private bool displayed = true; // displayed by default, i.e. when Ready

  private List<LoveTester.UI.Button> menuButtons;
  private int currentButton;

  public override void _Ready()
  {
    base._Ready();

    menuButtons = new List<LoveTester.UI.Button>();
    foreach (var buttonName in buttonNames)
    {
      menuButtons.Add(GetNode<LoveTester.UI.Button>(buttonName));
    }
  }

  public override void _Process(float delta)
  {
    if (!displayed) return;
    
    menuButtons[currentButton % menuButtons.Count].SetFocus();
  }

  private void OnFocusLost(string buttonName)
  {
    menuButtons[currentButton++ % menuButtons.Count].RemoveFocus();
  }

  private void OnButtonAnimationPlayerFinished(string name)
  {
    var signal = name == "play_slide_in" ? nameof(Shown) : nameof(Hidden);
    EmitSignal(signal);
  }

  public async void Display()
  {
    if (displayed) return;
    currentButton = 0;
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
    menuButtons.ForEach(b => b.RemoveFocus());
  }

  public void OnButtonPressed(string name)
  {
    ButtonClick.Play();
    EmitSignal(nameof(OnButtonClicked), name);
  }

  private void OnFocusEntered(string name)
  {
    for (var n = 0; n < menuButtons.Count; ++n)
    {
      menuButtons[n].RemoveFocus();
      if (menuButtons[n].Name == name) currentButton = n;
    }
  }
}
