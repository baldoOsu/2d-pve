using Godot;
using System;

public partial class Player : CharacterBody2D
{
	private AnimatedSprite2D anim;
	public enum Direction
	{
		Front,
		Right,
		Left,
		Back
	}
	public static readonly string[] DirectionTable = { "front_", "side_", "side_", "back_" };

	public const float Speed = 4500.0f;
	private Direction dir = Direction.Front;

	public override void _Ready()
	{
		//this.anim = GetNode<AnimatedSprite2D>("./Movement");
		//this.anim.Play("front_idle");
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = this.Velocity;

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");
		if (direction != Vector2.Zero)
		{
			velocity = direction * Player.Speed * (float)delta;
		}
		else
		{
			velocity.X = Mathf.MoveToward(this.Velocity.X, 0, Player.Speed * (float)delta);
			velocity.Y = 0;
		}

		//this.PlayAnim(this.VecToMovement(velocity));
		this.Velocity = velocity;
		MoveAndSlide();
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
		if (vec.X == 0 && vec.Y == 0)
		{
			return "idle";
		}


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

		if (vec.Y > 0)
		{
			this.dir = Direction.Front;
			return "walk";
		}
		else if (vec.Y < 0)
		{
			this.dir = Direction.Back;
			return "walk";
		}

		// should never happen, but the compiler is a bit stupid
		return "";
	}
}
