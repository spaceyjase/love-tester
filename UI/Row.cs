using Godot;
using System;

public class Row : HBoxContainer
{
  [Export] private string description;

  private Label Description => GetNode<Label>("Description");

  public override void _Ready()
  {
    base._Ready();

    Description.Text = description;
  }
}