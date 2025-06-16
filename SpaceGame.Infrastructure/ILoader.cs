namespace SpaceGame.Infrastructure;

public interface ILoader<T>
{
    bool TryGet(string name, out T? value);
    T Load(string name);
    ILoader<T> AddScope();
}
