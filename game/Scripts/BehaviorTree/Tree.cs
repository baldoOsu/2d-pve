using System.Collections;
using System.Collections.Generic;
using Godot;

namespace BehaviorTree
{
	public abstract partial class Tree : Node2D
	{

		private BtNode _root = null;

		public override void _Ready()
		{
			_root = SetupTree();
		}

		public override void _PhysicsProcess(double delta)
		{
			if (_root != null)
				_root.Evaluate();
		}

		protected abstract BtNode SetupTree();

	}

}
