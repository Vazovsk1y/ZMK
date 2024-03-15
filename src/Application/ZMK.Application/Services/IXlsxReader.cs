namespace ZMK.Application.Services;

public interface IXlsxReader<T>
{
    IEnumerable<T> Read(string filePath);
}