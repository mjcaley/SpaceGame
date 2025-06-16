namespace SpaceGame.Renderer;

public class BorrowedBuffer<T>(T buffer, List<T> returnList)
{
    public T Buffer => buffer;
    
    public void Return()
    {
        returnList.Add(buffer);
    }
}
