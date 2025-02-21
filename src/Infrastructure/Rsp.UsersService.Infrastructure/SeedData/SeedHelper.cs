using System.Text.Json;

namespace Rsp.UsersService.Infrastructure.SeedData;

public static class SeedHelper
{
    public static List<TEntity> SeedData<TEntity>(string fileName)
    {
        var currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
        const string path = "Data";
        var fullPath = Path.Combine(currentDirectory, path, fileName);

        var result = new List<TEntity>();
        using (var reader = new StreamReader(fullPath))
        {
            string json = reader.ReadToEnd();
            result = JsonSerializer.Deserialize<List<TEntity>>(json);
        }

        return result!;
    }
}