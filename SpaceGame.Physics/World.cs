using System.Linq;

namespace SpaceGame.Physics;

public class World
{
    private readonly List<Body?> _bodies = [];
    private readonly Queue<BodyHandle> _freeList = [];

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
        if (_bodies.Count > handle.Handle)
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

    public Body? Get(BodyHandle handle)
    {
        return _bodies.ElementAtOrDefault(handle.Handle);
    }

    public HashSet<Body> Query(BoundingBox boundingBox)
    {
        return [.. _bodies
            .Where(b => b is not null)
            .Select(b => b!)
            .Where(b => b.Shape switch {
                    Shape.Circle circle => boundingBox.Overlaps(circle.GetBoundingBox()),
                    _ => throw new NotSupportedException($"Shape type {b.Shape.GetType()} is not supported for bounding box queries.")
                }
            )];
    }

    public bool Colliding(Body b1, Body b2)
    {
        return CollisionResolver.Colliding(b1.Shape, b2.Shape);
    }
}
