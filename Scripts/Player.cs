using Godot;
using System;
using System.Linq;

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

	public const float Speed = 8500.0f;
	private Direction dir = Direction.Front;
  private Texture[] crosshairs = { null, null };
  private Viewport viewport;

  private static double BULLET_CD_RESET_TIMER = 0.15;
  private double bulletCd = -1.0;

	public override void _Ready()
	{
	  this.InitCrosshairAnim();
		this.anim = GetNode<AnimatedSprite2D>("./Movement");
		this.anim.Play("front_idle");
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

    Vector2 globalMousePos = this.GetGlobalMousePosition();
    this.SetDir(globalMousePos);

    this.bulletCd -= delta;
    bool isShooting = Input.IsActionPressed("shoot");
    if (isShooting && this.bulletCd < 0) {
      this.Shoot(this.GlobalTransform.Origin, globalMousePos);
    }

    

    this.PlayAnim(this.VecToMovement(velocity), isShooting);

    this.Velocity = velocity;
    MoveAndSlide();
	}

	private void PlayAnim(string movement, bool isShooting)
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
	if (isShooting) {
	  this.anim.Play("shoot");
	  return;
	}
		this.anim.Play("front_" + movement);
		// this.anim.Play(DirectionTable[(int)this.dir] + movement);
	}

	private string VecToMovement(Vector2 vec)
	{
		if (vec.X == 0 && vec.Y == 0)
		{
			return "idle";
		}

	return "idle";


		// if (vec.X > 0)
		// {
		// 	this.dir = Direction.Right;
		// 	return "walk";
		// }
		// else if (vec.X < 0)
		// {
		// 	this.dir = Direction.Left;
		// 	return "walk";
		// }

		// if (vec.Y > 0)
		// {
		// 	this.dir = Direction.Front;
		// 	return "walk";
		// }
		// else if (vec.Y < 0)
		// {
		// 	this.dir = Direction.Back;
		// 	return "walk";
		// }

	}

  private void SetDir(Vector2 globalMousePos) {
    if ((globalMousePos.X - this.Position.X) > 0) {
      this.dir = Direction.Right;
      return;
    }

    this.dir = Direction.Left;
  }

  private void Shoot(Vector2 startPos, Vector2 dir) {
    var spaceState = GetWorld2D().DirectSpaceState;

    // det her gør så man ikke kan skyde igennem vægge
    var RayQuery = PhysicsRayQueryParameters2D.Create(startPos, dir);
    var RayResult = spaceState.IntersectRay(RayQuery);

    Variant val = new();
    StaticBody2D cbVal = val.As<StaticBody2D>();
    RayResult.TryGetValue("collider", out val);
    try {
      cbVal = val.As<StaticBody2D>();
    } catch {
      return;
    }

    if (cbVal?.HasMeta("Enemy_Hitbox") == false)
      return;

    // det her gør så man skal have crosshair på enemy hitbox for at kunne ramme
    // hvis man bruger raycast alene, kan man ramme selv med crosshair bag enemy
    var PointQuery = new PhysicsPointQueryParameters2D();
    PointQuery.Position = dir;
    var PointResult = spaceState.IntersectPoint(PointQuery);
    if (PointResult.Count == 0)
      return;
    PointResult[0].TryGetValue("collider", out val);

    cbVal?.GetParent().Free();
    this.bulletCd = BULLET_CD_RESET_TIMER;
  }

  private void InitCrosshairAnim() {
    this.crosshairs[0] = ResourceLoader.Load<Texture>("res://Assets/Cursors/crosshair-frame-0.png");
    this.crosshairs[1] = ResourceLoader.Load<Texture>("res://Assets/Cursors/crosshair-frame-1.png");
    var timer = new System.Threading.Timer((object state) => {
      this.CallDeferred("SetMouseCursor", null);
    }, null, 0, 300);
  }

  public void SetMouseCursor() {
	  Input.SetCustomMouseCursor(this.crosshairs[0], default, new Vector2(17, 17));
	  (this.crosshairs[0], this.crosshairs[1]) = (this.crosshairs[1], this.crosshairs[0]);
  }
}
