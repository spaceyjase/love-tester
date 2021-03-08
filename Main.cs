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
    [Export] private float stopDelay = 0.25f;
    [Export] private int maxStopCount = 3;
    [Export] private int pseudoRandomIterations = 2;
    [Export] private float musicFadeDuration = 0.25f;
    [Export] private float actionMusicVolume = -10f;
    [Export] private bool attractModeEnabled = true;
    [Export] private float attractReenableTime = -30f;
    [Export] private float stoppedTime = 2f;

    private float timer;
    private List<Row> pseudoRandomItems;
    private int currentItem;
    private bool reset;
    private bool actionTouchRelease;
    private bool touched;
    private int stopCount;

    private GameState gameState;

    private CPUParticles2D BackgroundParticles => GetNode<CPUParticles2D>("BackgroundParticles");
    private AnimationPlayer ButtonAnimation => GetNode<AnimationPlayer>("ButtonAnimationPlayer");
    private Camera2D Camera => GetNode<Camera2D>("Camera2D");
    private TextureRect BackgroundGradient => GetNode<TextureRect>(nameof(BackgroundGradient));
    private AudioStreamPlayer ActionMusic => GetNode<AudioStreamPlayer>(nameof(ActionMusic));
    private Tween FadeMusicTween => GetNode<Tween>(nameof(FadeMusicTween));
    private AnimationPlayer FadeInAnimationPlayer => GetNode<AnimationPlayer>(nameof(FadeInAnimationPlayer));
    private AnimationPlayer InstructionsAnimationPlayer => GetNode<AnimationPlayer>(nameof(InstructionsAnimationPlayer));
    private AudioStreamPlayer InsertCoinSound => GetNode<AudioStreamPlayer>(nameof(InsertCoinSound));
    private AudioStreamPlayer BackgroundSound => GetNode<AudioStreamPlayer>(nameof(BackgroundSound));
    private Menu Menu => GetNode<Menu>(nameof(Menu));

    private bool insertCoinClicked;
    private bool instructionsShown;

    public override async void _Ready()
    {
      base._Ready();
      
      await ToSignal(FadeInAnimationPlayer, "animation_finished");
      
      ChangeState(GameState.Default);
    }

    private void ChangeState(GameState newState)
    {
      switch (newState)
      {
        case GameState.None:
          return;
        case GameState.Default:
          GD.Randomize();
          BackgroundParticles.Emitting = false;
          break;
        case GameState.AttractMode:
          GenerateLoveItems();
          break;
        case GameState.WaitingForHold:
          if (!instructionsShown)
          {
            ShowInstructions();
          }
          GenerateLoveItems();
          break;
        case GameState.Playing:
          break;
        case GameState.Stopping:
          break;
        case GameState.Stopped:
          break;
        case GameState.Finishing:
          break;
        case GameState.Finished:
          attractModeEnabled = false;
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
      }
      gameState = newState;
    }

    private void ShowInstructions()
    {
      instructionsShown = true;
      InstructionsAnimationPlayer.Stop();
      InstructionsAnimationPlayer.Play("instructions");
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

    public override async void _Process(float delta)
    {
      base._Process(delta);
      
      timer -= delta;

      switch (gameState)
      {
        case GameState.None:
          return;
        case GameState.Default:
          Menu.Display();
          ChangeState(GameState.AttractMode);
          break;
        case GameState.AttractMode:
          if (insertCoinClicked)
          {
            insertCoinClicked = false;
            BackgroundGradient.Visible = Global.GameMode == GameMode.Modern;
            ResetLights();
            Menu.Conceal();
            InsertCoinSound.Play();
            ChangeState(GameState.WaitingForHold);
          }
          else
          {
            if (!attractModeEnabled && timer < attractReenableTime)
            {
              attractModeEnabled = true;
              ResetLights();
            }

            if (attractModeEnabled)
            {
              if (timer > 0f) return;
              pseudoRandomItems[currentItem++ % pseudoRandomItems.Count].Off();
              pseudoRandomItems[currentItem % pseudoRandomItems.Count].On();
              timer = attractTimer;
            }
          }
          break;
        case GameState.WaitingForHold:
          if (!Input.IsActionPressed(Global.MainButton) && !touched) return;
          actionTouchRelease = false;
          ChangeState(GameState.Playing);
          break;
        case GameState.Playing:
          if (Input.IsActionJustReleased(Global.MainButton) || actionTouchRelease)
          {
            actionTouchRelease = false;
            stopCount = 1;
            timer = stopDelay;
            ButtonAnimation.Play("idle");
            ChangeState(GameState.Stopping);
            return;
          }
          if (timer > 0f) return;
          if (reset)
          {
            reset = false;
            StartMusic();
            ButtonAnimation.Play("pressed");
            if (Global.GameMode == GameMode.Modern)
            {
              BackgroundParticles.Emitting = true;
            }
          }
          if (Global.GameMode == GameMode.Modern)
          {
            Camera.Call("add_stress", 0.5f);
          }
          pseudoRandomItems[currentItem++ % pseudoRandomItems.Count].Off();
          pseudoRandomItems[currentItem % pseudoRandomItems.Count].On();
          timer = delayTimer; 
          break;
        case GameState.Stopping:
          if (timer > 0f) return;
          pseudoRandomItems[currentItem++ % pseudoRandomItems.Count].Off();
          pseudoRandomItems[currentItem % pseudoRandomItems.Count].On();
          // slow to a halt...
          timer = stopDelay * ++stopCount;
          // long enough? stop now
          if (timer > (stopDelay * maxStopCount))
          {
            ChangeState(GameState.Stopped);
          }
          break;
        case GameState.Stopped:
          StopMusic();
          pseudoRandomItems[currentItem % pseudoRandomItems.Count].ShowParticles();
          pseudoRandomItems[currentItem % pseudoRandomItems.Count].PlayStoppedSound();
          BackgroundParticles.Emitting = false;
          ChangeState(GameState.Finishing);
          break;
        case GameState.Finishing:
          await ToSignal(GetTree().CreateTimer(stoppedTime), "timeout");
          ChangeState(GameState.Finished);
          break;
        case GameState.Finished:
          ChangeState(GameState.Default);
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

    // ReSharper disable once UnusedMember.Global
    public void OnInsertCoinClicked()
    {
      insertCoinClicked = true;
      Global.GameMode = GameMode.Original;
      BackgroundSound.Stop();
    }
    
    // ReSharper disable once UnusedMember.Global
    public void OnOptionButtonClicked()
    {
      insertCoinClicked = true;
      Global.GameMode = GameMode.Modern;
      BackgroundSound.Play();
    }
  }
}
