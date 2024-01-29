using System.Collections.Generic;

namespace BehaviorTree
{
	public class Selector : BtNode
	{
		public Selector() : base() { }
		public Selector(List<BtNode> children) : base(children) { }

		public override NodeState Evaluate()
		{
			foreach (BtNode node in children)
			{
				switch (node.Evaluate())
				{
					case NodeState.FAILURE:
						continue;
					case NodeState.SUCCESS:
						state = NodeState.SUCCESS;
						return state;
					case NodeState.RUNNING:
						state = NodeState.RUNNING;
						return state;
					default:
						continue;
				}
			}

			state = NodeState.FAILURE;
			return state;
		}

	}

}
