using Godot;
using System;

public partial class Muteret_mand : CharacterBody2D
{
  private CharacterBody2D playerObj;


  private const float SPEED = 50.0f;
  private const double DAMAGE_CD = 1.0f;
  private double currentDamageCd = 1.0f;
  private const int DAMAGE = 10;
  public override void _Ready()
	{
	  base._Ready();
	  this.playerObj = GetNode<CharacterBody2D>("/root/Root/Map/Player");
	}

  public override void _PhysicsProcess(double delta)
  {
	this.currentDamageCd -= delta;

	Vector2 direction = this.playerObj.Position - this.Position;

	this.Velocity = direction.Normalized() * Muteret_mand.SPEED;
	MoveAndSlide();

	if (this.currentDamageCd > 0)
	  return;

	int collisionCount = GetSlideCollisionCount();
	for (int i = 0; i < collisionCount; i++) {
	  // sikrer ikke at gøre mere end 10 damage ad gangen
	  if (this.currentDamageCd > 0)
		return;

	  KinematicCollision2D collision = GetSlideCollision(i);
	  GodotObject collider = collision.GetCollider();
	  if (collider.HasMethod("Damage"))  {
		collider.CallDeferred("Damage", Muteret_mand.DAMAGE);
		this.currentDamageCd = Muteret_mand.DAMAGE_CD;
	  }
	}
  }
}
