using System.Collections.Generic;
using System.Linq;
using Godot;

namespace LoveTester
{
  public class Main : Control
  {
    [Export] private float delayTimer = 0.25f;
    [Export] private int pseudoRandomIterations = 2;
    
    private float timer;
    private List<Row> pseudoRandomItems;
    private int currentItem;
    private bool reset;

    public override void _Ready()
    {
      base._Ready();
    
      GD.Randomize();
    
      // Generate a pseudo random list that's repeated while input is held
      GenerateLoveItems();
    }

    private void GenerateLoveItems()
    {
      pseudoRandomItems = new List<Row>();
      for (var n = 0; n < pseudoRandomIterations; ++n)
      {
        foreach (var row in Global.Rows.OrderBy(a => GD.Randi()))
        {
          if (pseudoRandomItems.Any() && pseudoRandomItems.Last() == row) continue;
          pseudoRandomItems.Add(row);
        }
      }
      reset = true;
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

      if (reset)
      {
        ResetLights();
      }

      pseudoRandomItems[currentItem++ % pseudoRandomItems.Count].Off();
      pseudoRandomItems[currentItem % pseudoRandomItems.Count].On();
      timer = delayTimer;
    }

    private void ResetLights()
    {
      pseudoRandomItems.ForEach(r => r.Off());
      reset = false;
    }
  }
}
