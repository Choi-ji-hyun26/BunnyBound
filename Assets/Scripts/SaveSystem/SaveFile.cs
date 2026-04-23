
[System.Serializable]
public class SaveFile<T> // Save Root Wrapper 클래스
{
    public int version;
    public T data;

    public SaveFile(int version, T data)
    {
        this.version = version;
        this.data = data;
    }
}
