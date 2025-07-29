public interface ISingleton 
{
    bool IsInited { get; }

    void Init();

    void Destroy();
}