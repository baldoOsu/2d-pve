using System;
using System.Collections;
using System.Collections.Generic;


namespace BehaviorTree
{
	public enum NodeState
	{
		RUNNING,
		SUCCESS,
		FAILURE
	}

	public class BtNode
	{
		protected NodeState state;

		public BtNode parent;
		protected List<BtNode> children = new List<BtNode>();

		private Dictionary<string, object> _dataContext = new Dictionary<string, object>();

		public BtNode()
		{
			parent = null;
		}
		public BtNode(List<BtNode> children)
		{
			foreach (BtNode child in children)
				_Attach(child);
		}

		private void _Attach(BtNode node)
		{
			node.parent = this;
			children.Add(node);
		}
		public virtual NodeState Evaluate() => NodeState.RUNNING;

		public void SetData(string key, object value)
		{
			_dataContext[key] = value;
		}

		public object GetData(string key)
		{
			object value = null;
			if (_dataContext.TryGetValue(key, out value))
				return value;

			BtNode node = parent;
			while (node != null)
			{
				value = node.GetData(key);
				if (value != null)
					return value;
				node = node.parent;
			}
			return null;
		}

		public bool ClearData(string key)
		{
			if (_dataContext.ContainsKey(key))
			{
				_dataContext.Remove(key);
				return true;
			}

			BtNode node = parent;
			while (node != null)
			{
				bool cleared = node.ClearData(key);
				if (cleared)
					return true;
				node = node.parent;
			}
			return false;
		}
	}

}
