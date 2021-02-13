using Godot;
using System;
using System.Xml;

public class Row : HBoxContainer
{
  [Export] private string description;
  [Export] private Color bulbColour;

  private Label Description => GetNode<Label>("Description");
  private ColorRect ColorRect => GetNode<ColorRect>("CenterContainer/ColorRect");
  private static readonly Vector2 colourSize = new Vector2(64, 64);

  public override void _Ready()
  {
    base._Ready();

    ConfigureRow();
  }

  private void ConfigureRow()
  {
    Description.Text = description;
    ColorRect.Color = bulbColour;
    //ColorRect.Visible = false;
  }
}