using Godot;
using System;

public partial class Muteret_mand : Enemy
{
  protected override string VecToMovement(Vector2 vec)
	{
		if (vec.X == 0 && vec.Y == 0)
			return "idle";

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

		if (vec.Y > 0)
		{
			this.dir = Direction.Up;
			return "walk";
		}
		else if (vec.Y < 0)
		{
			this.dir = Direction.Down;
			return "walk";
		}

		return ""; // vil aldrig ske
	}
}
