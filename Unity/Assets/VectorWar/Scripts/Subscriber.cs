using Unity.Mathematics.FixedPoint;

// TODO: Move to Physics package?
public interface Subscriber
{
    void Subscribe();
    void Unsubscribe();
    void Step(fp timestep, long inputs);
}
