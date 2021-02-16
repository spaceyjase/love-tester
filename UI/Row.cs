using Godot;
using System;
using System.Xml;

public class Row : HBoxContainer
{
  [Export] private string description;
  [Export] private Color bulbColour;

  public string Description => description;
  
  private Label DescriptionLabel => GetNode<Label>("Description");
  private ColorRect ColorRect => GetNode<ColorRect>("CenterContainer/ColorRect");
  private Light2D Light => GetNode<Light2D>("CenterContainer/Light2D");
  private static readonly Vector2 colourSize = new Vector2(64, 64);

  public override void _Ready()
  {
    base._Ready();

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
    // TODO: tween light
    Light.Visible = false;
  }

  public void On()
  {
    Light.Visible = true;
  }
}