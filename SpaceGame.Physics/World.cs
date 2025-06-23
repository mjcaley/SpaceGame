using System.Numerics;
using System.Linq;

namespace SpaceGame.Physics;

public class World
{
    private readonly List<Body?> _bodies = [];
    private readonly Queue<BodyHandle> _freeList = [];

    public event EventHandler<PositionChangedEvent>? PositionChanged;

    protected virtual void OnPositionChanged(BodyHandle handle, Vector2 position)
    {
        PositionChanged?.Invoke(this, new(handle, position));
    }

    public BodyHandle Add(Body body)
    {
        if (_freeList.Count > 0)
        {
            var handle = _freeList.Dequeue();
            _bodies[handle.Handle] = body;
            return handle;
        }
        _bodies.Add(body);

        return new BodyHandle { Handle = _bodies.Count - 1 };
    }

    public void Remove(BodyHandle handle)
    {
        if (_bodies.Count <= handle.Handle)
        {
            return;
        }

        _bodies[handle.Handle] = null;
        _freeList.Enqueue(handle);
    }

    public void Clear()
    {
        _bodies.Clear();
        _freeList.Clear();
    }

    public Body? Get(BodyHandle handle) =>
        _bodies.Count <= handle.Handle ? null : _bodies[handle.Handle];

    public HashSet<Body> Query(BoundingBox boundingBox) =>
    [.. _bodies
        .Where(b => b is not null)
        .Select(b => b!)
        .Where(b => b.Shape switch {
                Shape.Circle circle => boundingBox.Overlaps(circle.GetBoundingBox()),
                _ => throw new NotSupportedException($"Shape type {b.Shape.GetType()} is not supported for bounding box queries.")
            }
        )];

    public void Step(float deltaTime)
    {
        var broadCollisions = Broadphase();
        var narrowCollisions = Narrowphase(broadCollisions);

    }

    public HashSet<CollisionPair> Broadphase()
    {
        var pairs = new HashSet<CollisionPair>();
        
        foreach (var b1 in _bodies.Where(b => b is not null))
        {
            foreach (var b2 in _bodies.Skip(1).Union(_bodies.Take(1)).Where(b => b is not null))
            {
                var b1Box = Shape.GetBoundingBox(b1!.Shape);
                var b2Box = Shape.GetBoundingBox(b2!.Shape);
                if (b1Box.Overlaps(b2Box))
                {
                    pairs.Add(new CollisionPair(b1, b2));
                }
            }
        }

        return pairs;
    }

    public Dictionary<CollisionPair, CollisionResult> Narrowphase(HashSet<CollisionPair> broadPairs) => new([
        ..broadPairs
            .Select(p => new Tuple<CollisionPair, CollisionResult?>(p, CollisionResolver.Resolve(p.A.Shape, p.B.Shape)))
            .Where(t => t.Item2 is not null)
            .Select(t => new KeyValuePair<CollisionPair, CollisionResult>(t.Item1, t.Item2!))
    ]);

    public HashSet<CollisionPair> Resolve(Dictionary<CollisionPair, CollisionResult> collisions)
    {
        foreach (var collision in collisions)
        {
            if (CollisionResolver.TryResolve(collision.Key.A.Shape, collision.Key.B.Shape, out var stillColliding))
            {

            }
        }

        return new();
    }
    
    public bool Colliding(Body b1, Body b2)
    {
        return CollisionResolver.Colliding(b1.Shape, b2.Shape);
    }
}
