using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace LoveTester
{
  public class Main : Control
  {
    [Export] private float delayTimer = 0.25f;
    [Export] private int pseudoRandomIterations = 2;
    [Export] private float musicFadeDuration = 0.25f;
    [Export] private float actionMusicVolume = -10f;

    private const string MainButton = "main_button";
    
    private float timer;
    private List<Row> pseudoRandomItems;
    private int currentItem;
    private bool reset;
    private bool actionTouchRelease;
    private bool touched;

    private CPUParticles2D BackgroundParticles => GetNode<CPUParticles2D>("BackgroundParticles");
    private AnimationPlayer ButtonAnimation => GetNode<AnimationPlayer>("ButtonAnimationPlayer");
    private Camera2D Camera => GetNode<Camera2D>("Camera2D");
    private AudioStreamPlayer ActionMusic => GetNode<AudioStreamPlayer>(nameof(ActionMusic));
    private Tween FadeMusicTween => GetNode<Tween>(nameof(FadeMusicTween));

    public override void _Ready()
    {
      base._Ready();
    
      GD.Randomize();
    
      // Generate a pseudo random list that's repeated while input is held
      GenerateLoveItems();

      BackgroundParticles.Emitting = false;
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

      if (Input.IsActionJustReleased(MainButton) || actionTouchRelease)
      {
        actionTouchRelease = false;
        
        ButtonAnimation.Play("idle");
        StopMusic();
        // Text effect for the stopped row
        pseudoRandomItems[currentItem % pseudoRandomItems.Count].ShowParticles();
        
        // Generate a new list...
        GenerateLoveItems();
        BackgroundParticles.Emitting = false;
      }
    
      if (!Input.IsActionPressed(MainButton) && !touched) return;
      if (timer > 0f) return;

      if (reset)
      {
        StartMusic();
        ButtonAnimation.Play("pressed");
        ResetLights();
        BackgroundParticles.Emitting = true;
      }
      
      Camera.Call("add_stress", 0.5f);
      pseudoRandomItems[currentItem++ % pseudoRandomItems.Count].Off();
      pseudoRandomItems[currentItem % pseudoRandomItems.Count].On();
      
      timer = delayTimer;
    }
    
    private void StartMusic()
    {
        if (!ActionMusic.Playing)
        {
          ActionMusic.Play();
        }

        ActionMusic.VolumeDb = actionMusicVolume;
    }

    private void StopMusic()
    {
        FadeMusicTween.Stop(ActionMusic);
        FadeMusicTween.InterpolateProperty(ActionMusic, "volume_db", actionMusicVolume, -80, musicFadeDuration,
          Tween.TransitionType.Linear, Tween.EaseType.Out);
        FadeMusicTween.Start();
    }

    private void ResetLights()
    {
      pseudoRandomItems.ForEach(r => r.ImmediateOff());
      reset = false;
    }

    public override void _Input(InputEvent @event)
    {
      base._Input(@event);
      if (!(@event is InputEventScreenTouch)) return;
      touched = @event.IsPressed();
      if (!touched) actionTouchRelease = true;
    }
  }
}
