using Godot;
using System;

public partial class Muteret_hund : Enemy
{
	protected override string VecToMovement(Vector2 vec)
	{
	// undgå at opdatere this.dir hvis man skyder
	if (vec.X >= 0)
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
