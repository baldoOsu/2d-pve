using System.Collections.Generic;

namespace BehaviorTree
{
	public class Parallel : BtNode
	{
		public Parallel() : base() { }
		public Parallel(List<BtNode> children) : base(children) { }

		public override NodeState Evaluate()
		{
			foreach (BtNode node in children)
			{
        node.Evaluate();
			}

			return NodeState.RUNNING;
		}

	}

}
