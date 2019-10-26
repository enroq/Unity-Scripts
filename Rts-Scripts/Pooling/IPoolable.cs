public interface IPoolable
{
    int ParentInstanceId { get; set; }

    void OnExtraction();
}
