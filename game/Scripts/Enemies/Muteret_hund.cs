using Godot;
using System;

public partial class Muteret_hund : CharacterBody2D
{
  private CharacterBody2D playerObj;


  private const float SPEED = 50.0f;
  private const double DAMAGE_CD = 1.0f;
  private double currentDamageCd = 1.0f;
  private const int DAMAGE = 10;

  public enum Direction
	{
		Down,
		Right,
		Left,
		Up
	}
	public static readonly string[] DirectionTable = { "up_", "side_", "side_", "front_" };
	private Direction dir = Direction.Left;
  AnimatedSprite2D anim;

  public override void _Ready()
	{
    this.anim = GetNode<AnimatedSprite2D>("./Movement");
	  this.playerObj = GetNode<CharacterBody2D>("/root/Root/Map/Player");
	}

  public override void _PhysicsProcess(double delta)
  {
	this.currentDamageCd -= delta;

	Vector2 direction = this.playerObj.Position - this.Position;

  PlayAnim(VecToMovement(direction));

	this.Velocity = direction.Normalized() * Muteret_hund.SPEED;
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
		collider.CallDeferred("Damage", Muteret_hund.DAMAGE);
		this.currentDamageCd = Muteret_hund.DAMAGE_CD;
	  }
	}
  }

  private void PlayAnim(string movement)
	{
		switch (this.dir)
		{
			case Direction.Left:
				this.anim.FlipH = true;
				break;

			case Direction.Right:
				this.anim.FlipH = false;
				break;
		}

    this.anim.Play(DirectionTable[(int)this.dir] + movement);
	}

  private string VecToMovement(Vector2 vec)
	{
    // undgå at opdatere this.dir hvis man skyder
    if (vec.X > 0)
    {
      this.dir = Direction.Right;
      return "walk";
    }
    else if (vec.X < 0)
    {
      this.dir = Direction.Left;
      return "walk";
    }

    return ""; // vil aldrig ske

	}
}
