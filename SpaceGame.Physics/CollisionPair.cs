namespace SpaceGame.Physics;

public class CollisionPair(Body b1, Body b2)
{
    public Body A => b1;
    public Body B => b2;
}
