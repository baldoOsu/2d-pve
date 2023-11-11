using Godot;
using System;

public partial class Muteret_mand : CharacterBody2D
{
  public const float Speed = 50.0f;
  public const float JumpVelocity = -400.0f;
  private CharacterBody2D playerObj;
  public override void _Ready()
    {
      base._Ready();
      this.playerObj = GetNode<CharacterBody2D>("/root/Root/Map/Player");
      GD.Print(this.playerObj.GetPath());
    }

  public override void _PhysicsProcess(double delta)
  {
    Vector2 direction = this.playerObj.Position - this.Position;

    this.Velocity = direction.Normalized() * Muteret_mand.Speed;
    MoveAndSlide();
  }
}
