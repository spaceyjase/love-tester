using Godot;

namespace LoveTester.UI
{
  public class Row : HBoxContainer
  {
    [Export] private string description;
    [Export] private Color bulbColour;
    [Export] private float endLightEnergy;
    [Export] private float offDuration = 0.05f;
    [Export] private PackedScene particles;
    [Export] private AudioStreamOGGVorbis selectedSound;

    public string Description => description;
  
    private Label DescriptionLabel => GetNode<Label>("Description");
    private ColorRect ColorRect => GetNode<ColorRect>("CenterContainer/ColorRect");
    private Light2D Light => GetNode<Light2D>("CenterContainer/Light2D");
    private Tween LightTween => GetNode<Tween>("LightTween");
    private AudioStreamPlayer2D SelectedSound => GetNode<AudioStreamPlayer2D>(nameof(SelectedSound));
    private AudioStreamPlayer2D StoppedSound => GetNode<AudioStreamPlayer2D>(nameof(StoppedSound));
    private AnimationPlayer LightAnimationPlayer => GetNode<AnimationPlayer>(nameof(LightAnimationPlayer));

    private float startingLightEnergy;
    private CPUParticles2D particlesInstance;

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
      StoppedSound.Stream = selectedSound;
    }

    public void Off()
    {
      if (!Light.Visible) return;
      if (LightTween.IsActive()) return;
    
      LightTween.InterpolateProperty(Light, "energy", startingLightEnergy, endLightEnergy,
        offDuration, Tween.TransitionType.Sine);
      LightTween.Start();
    }
  
    public void ImmediateOff()
    {
      Light.Visible = false;
      LightAnimationPlayer.Stop();
    }

    public void _on_LightTween_tween_completed(object o, NodePath key)
    {
      Light.Visible = false;
    }

    public void On()
    {
      Light.Energy = startingLightEnergy;
      Light.Visible = true;
      SelectedSound.Play();
    }

    public void PlayStoppedSound()
    {
      StoppedSound.Play();
    }

    public void ShowParticles()
    {
      if (particles == null) return;

      if (particlesInstance == null)
      {
        particlesInstance = particles.Instance() as CPUParticles2D;
        DescriptionLabel.AddChild(particlesInstance);
      }

      if (particlesInstance == null) return;
      
      particlesInstance.Emitting = true;
      foreach (var child in particlesInstance.GetChildren())
      {
        if (child is CPUParticles2D particles2D)
        {
          particles2D.Emitting = true;
        }
      }
      LightAnimationPlayer.Seek(0f);
      LightAnimationPlayer.Play("selected");
    }
  }
}