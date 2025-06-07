namespace Common;

public class FileFormatter
{
    public static string GetFile(string containerName, string fileName,string extension)
    {
        var result = "";

        if (!string.IsNullOrEmpty(fileName))
        {
            result = $"$https://flowia.s3.eu-central-1.amazonaws.com/{containerName}/{fileName}{extension}";
        }

        return result;
    }
}