using Godot;

using BehaviorTree;
using System;
using System.Collections.Generic;

// public class WithinRangeNode : BtNode
// {
//     public override NodeState Evaluate()
//     {
//         object dataResponse = GetData("Position");
//         if (dataResponse == null)
//             return NodeState.FAILURE;

//         return NodeState.SUCCESS;

//         //Vector2 position = (Vector2)dataResponse;

//     }
// }

public class AttackPlayerNode : BtNode
{
    private CharacterBody2D _selfTransform;
    private CharacterBody2D _playerTransform;
    private float _speed;

    public AttackPlayerNode(CharacterBody2D selfTransform, CharacterBody2D playerTransform, float speed) {
        _selfTransform = selfTransform;
        _playerTransform = playerTransform;
        _speed = speed;
    }

    public override NodeState Evaluate()
    {
        Vector2 dir = (_playerTransform.Position - _selfTransform.Position).Normalized();
        _selfTransform.Velocity = dir * _speed;
        _selfTransform.MoveAndSlide();

        GD.Print("Attacking player");
        return NodeState.RUNNING;
    }
}

public class AttackPlayerCheckNode : BtNode
{
    private CharacterBody2D _selfTransform;
    private CharacterBody2D _playerTransform;

    public AttackPlayerCheckNode(CharacterBody2D selfTransform, CharacterBody2D playerTransform) {
        _selfTransform = selfTransform;
        _playerTransform = playerTransform;
    }

    public override NodeState Evaluate()
    {
        float dist = (_playerTransform.Position - _selfTransform.Position).Length();

        if (dist < 5)
            return NodeState.SUCCESS;

        return NodeState.FAILURE;
    }
}

public class ChasePlayerNode : BtNode
{
    private CharacterBody2D _selfTransform;
    private CharacterBody2D _playerTransform;
    private float _speed;

    public ChasePlayerNode(CharacterBody2D selfTransform, CharacterBody2D playerTransform, float speed) {
        _selfTransform = selfTransform;
        _playerTransform = playerTransform;
        _speed = speed;
    }

    public override NodeState Evaluate()
    {
        Vector2 dir = (_playerTransform.Position - _selfTransform.Position).Normalized();
        _selfTransform.Velocity = dir * _speed;
        _selfTransform.MoveAndSlide();

        GD.Print("Chasing player");
        return NodeState.RUNNING;
    }
}

public class ChasePlayerCheckNode : BtNode
{
    private CharacterBody2D _selfTransform;
    private CharacterBody2D _playerTransform;

    public ChasePlayerCheckNode(CharacterBody2D selfTransform, CharacterBody2D playerTransform) {
        _selfTransform = selfTransform;
        _playerTransform = playerTransform;
    }

    public override NodeState Evaluate()
    {
        float dist = (_playerTransform.Position - _selfTransform.Position).Length();

        if (dist < 100)
            return NodeState.SUCCESS;

        return NodeState.FAILURE;
    }
}

public partial class PatrolNode : BtNode
{
    private CharacterBody2D _selfTransform;
    private List<Node2D> _waypoints;
    private int _currentWaypointIdx = 0;
    private float _speed;
    public PatrolNode(CharacterBody2D selfTransform, List<Node2D> waypoints, float speed)
    {
        _speed = speed;
        _selfTransform = selfTransform;
        _waypoints = waypoints;

        List<double> distances = new();
        for (int i = 0; i < waypoints.Count; i++) {
            Vector2 diff = waypoints[i].Position - _selfTransform.Position;
            distances.Add(Math.Sqrt(Math.Pow(diff.X, 2) + Math.Pow(diff.Y,2)));
        }

        double lowest = distances[0];
        for (int i = 1; i < 3; i++) {
            if (distances[i] < lowest) {
                lowest = distances[i];
                _currentWaypointIdx = i; // ^_^
            }
        }
    }

    public override NodeState Evaluate() {
        Node2D currentWaypoint = _waypoints[_currentWaypointIdx];
        Vector2 dir = (currentWaypoint.GlobalPosition - _selfTransform.GlobalPosition).Normalized();
        _selfTransform.Velocity = dir.Normalized() * _speed;
        _selfTransform.MoveAndSlide();

        if (_selfTransform.GlobalPosition.DistanceTo(currentWaypoint.GlobalPosition) < 5.0f ) {
            GD.Print("Moving to next waypoint");
            _currentWaypointIdx = (_currentWaypointIdx+1) % _waypoints.Count;
        }

        return NodeState.RUNNING;
    }
}

public class AvoidObstacleCheck : BtNode
{
    Node2D _selfTransform;
    PhysicsDirectSpaceState2D _spaceState;
    public AvoidObstacleCheck(Node2D selfTransform, PhysicsDirectSpaceState2D spaceState)
    {
        _selfTransform = selfTransform;
    }

    public override NodeState Evaluate()
    {
        return NodeState.FAILURE;
    }
}

public partial class RootEnemyNode : BtNode
{
    private Selector _root;
    public RootEnemyNode(CharacterBody2D selfTransform, CharacterBody2D playerTransform, List<Node2D> waypoints, float speed)
    {
        AttackPlayerCheckNode atkPlayerCheckNode = new(selfTransform, playerTransform);
        ChasePlayerCheckNode chasePlayerCheckNode = new(selfTransform, playerTransform);

        PatrolNode patrolActionNode = new(selfTransform, waypoints, speed);
        ChasePlayerNode chasePlayerActionNode = new(selfTransform, playerTransform, speed);
        AttackPlayerNode atkPlayerActionNode = new(selfTransform, playerTransform, speed);

        List<BtNode> nodes = new()
        {
            new Sequence(new() { atkPlayerCheckNode, atkPlayerActionNode }),
            new Sequence(new() { chasePlayerCheckNode, chasePlayerActionNode }),
            patrolActionNode
        };

        

        _root = new(nodes);
    }

    public override NodeState Evaluate()
    {
        return _root.Evaluate();
    }
}