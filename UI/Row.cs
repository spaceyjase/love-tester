using Godot;
using System;
using System.Xml;

public class Row : HBoxContainer
{
  [Export] private string description;
  [Export] private Color bulbColour;
  [Export] private float endLightEnergy = 0f;
  [Export] private float offDuration = 0.05f;

  public string Description => description;
  
  private Label DescriptionLabel => GetNode<Label>("Description");
  private ColorRect ColorRect => GetNode<ColorRect>("CenterContainer/ColorRect");
  private Light2D Light => GetNode<Light2D>("CenterContainer/Light2D");
  private Tween LightTween => GetNode<Tween>("LightTween");

  private float startingLightEnergy;

  public override void _Ready()
  {
    base._Ready();

    startingLightEnergy = Light.Energy;
    ConfigureRow();

    Global.Register(this);
  }

  private void ConfigureRow()
  {
    DescriptionLabel.Text = description;
    var backgroundColor = bulbColour;
    backgroundColor.a = 164f;
    ColorRect.Color = backgroundColor;
    Light.Color = bulbColour;
    Light.Visible = false;  // Initial state is off.
  }

  public void Off()
  {
    if (!Light.Visible) return;
    if (LightTween.IsActive()) return;
    
    LightTween.InterpolateProperty(Light, "energy", startingLightEnergy, endLightEnergy,
      offDuration, Tween.TransitionType.Sine);
    LightTween.Start();
  }

  public void _on_LightTween_tween_completed(object o, NodePath key)
  {
    Light.Visible = false;
  }

  public void On()
  {
    Light.Energy = startingLightEnergy;
    Light.Visible = true;
  }
}