using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using LoveTester.UI;

namespace LoveTester
{
  public class Main : Control
  {
    [Export] private float delayTimer = 0.25f;
    [Export] private float attractTimer = 1f;
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

    private GameState gameState;

    private CPUParticles2D BackgroundParticles => GetNode<CPUParticles2D>("BackgroundParticles");
    private AnimationPlayer ButtonAnimation => GetNode<AnimationPlayer>("ButtonAnimationPlayer");
    private Camera2D Camera => GetNode<Camera2D>("Camera2D");
    private AudioStreamPlayer ActionMusic => GetNode<AudioStreamPlayer>(nameof(ActionMusic));
    private Tween FadeMusicTween => GetNode<Tween>(nameof(FadeMusicTween));
    private Menu Menu => GetNode<Menu>(nameof(Menu));

    public override void _Ready()
    {
      base._Ready();
      ChangeState(GameState.Default);
    }

    private void ChangeState(GameState newState)
    {
      switch (newState)
      {
        case GameState.Default:
          GD.Randomize();
          BackgroundParticles.Emitting = false;
          break;
        case GameState.AttractMode:
          GD.Print("Waiting for coin...");
          GenerateLoveItems();
          break;
        case GameState.WaitingForHold:
          GD.Print("Waiting for hold...");
          GenerateLoveItems();
          break;
        case GameState.Playing:
          GD.Print("Playing...");
          break;
        case GameState.Stopping:
          GD.Print("Stopping...");
          break;
        case GameState.Stopped:
          GD.Print("Stopped...");
          break;
        case GameState.Finished:
          GD.Print("Finished... waiting for input");
          break;
        case GameState.Options:
          GD.Print("Display options");
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
      }
      gameState = newState;
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
      ResetLights();
      reset = true;
    }

    public override async void _Process(float delta)
    {
      base._Process(delta);
      
      timer -= delta;

      switch (gameState)
      {
        case GameState.Default:
          // TODO: show UI
          await ToSignal(Menu, "Shown");
          ChangeState(GameState.AttractMode);
          break;
        case GameState.AttractMode:
          // TODO: dialog or interface showing 'insert coin'/attract mode/instructions
          if (Input.IsActionJustReleased(MainButton) /* TODO: button pressed */)
          {
            ChangeState(GameState.WaitingForHold);
            return;
          }
          if (timer > 0f) return;
          pseudoRandomItems[currentItem++ % pseudoRandomItems.Count].Off();
          pseudoRandomItems[currentItem % pseudoRandomItems.Count].On();
          timer = attractTimer;
          break;
        case GameState.WaitingForHold:
          if (!Input.IsActionPressed(MainButton) && !touched) return;
          ChangeState(GameState.Playing);
          break;
        case GameState.Playing:
          if (Input.IsActionJustReleased(MainButton) || actionTouchRelease)
          {
            actionTouchRelease = false;
            ChangeState(GameState.Stopping);
            return;
          }

          if (timer > 0f) return;
          if (reset)
          {
            reset = false;
            StartMusic();
            ButtonAnimation.Play("pressed");
            BackgroundParticles.Emitting = true;
          }

          // TODO: if enabled...
          Camera.Call("add_stress", 0.5f);
          pseudoRandomItems[currentItem++ % pseudoRandomItems.Count].Off();
          pseudoRandomItems[currentItem % pseudoRandomItems.Count].On();

          timer = delayTimer; 
          break;
        case GameState.Stopping:
          // TODO: stopping... slow to a halt over N seconds
          ChangeState(GameState.Stopped);
          break;
        case GameState.Stopped:
          ButtonAnimation.Play("idle");
          StopMusic();
          pseudoRandomItems[currentItem % pseudoRandomItems.Count].ShowParticles();
          pseudoRandomItems[currentItem % pseudoRandomItems.Count].PlayStoppedSound();
          BackgroundParticles.Emitting = false;
          ChangeState(GameState.Finished);
          break;
        case GameState.Finished:
          if (Input.IsActionJustReleased(MainButton) /* TODO: button pressed */)
          {
            ChangeState(GameState.Default);
          }
          break;
        case GameState.Options:
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
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
