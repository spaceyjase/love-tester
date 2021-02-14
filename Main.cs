using System.Collections.Generic;
using System.Linq;
using Godot;

namespace LoveTester
{
  public class Main : Control
  {
    [Export] private float delayTimer = 0.25f;
    [Export] private int pseudoRandomIterations = 2;
  
    // TODO: from names in UI - register on ready?
    private readonly List<string> loveItems = new List<string>
    {
      "Uncontrollable",
      "Hot Stuff",
      "Passionate",
      "Burning",
      "Sexy",
      "Wild",
      "Mild",
      "Harmless",
      "Clammy",
      "Blah"
    };
  
    private float timer;
    private List<string> pseudoRandomItems;
    private int currentItem;

    public override void _Ready()
    {
      base._Ready();
    
      GD.Randomize();
    
      // Generate a pseudo random list that's repeated while input is held
      GenerateLoveItems();
    }

    private void GenerateLoveItems()
    {
      pseudoRandomItems = new List<string>();
      for (var n = 0; n < pseudoRandomIterations; ++n)
      {
        foreach (var item in loveItems.OrderBy(a => GD.Randi()))
        {
          pseudoRandomItems.Add(item);
        }
      }
      currentItem = 0;
    }

    public override void _Process(float delta)
    {
      base._Process(delta);
    
      timer -= delta;

      if (Input.IsActionJustReleased("ui_select"))
      {
        // Generate a new list...
        GenerateLoveItems();
      }
    
      if (!Input.IsActionPressed("ui_select")) return;
      if (timer > 0f) return;

      var item = pseudoRandomItems[currentItem++ % pseudoRandomItems.Count];
      GD.Print(item);
      timer = delayTimer;
    }
  }
}
