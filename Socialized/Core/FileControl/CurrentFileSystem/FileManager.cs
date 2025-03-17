using Serilog;

namespace Core.FileControl.CurrentFileSystem;

public class FileManager(ILogger logger) : IFileManager
{
    private readonly string currentDirectory = Directory.GetCurrentDirectory();
    public readonly ILogger Logger = logger;
    public DateTime currentTime = DateTime.Now;
    public string dailyFolder = "/" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day + "/";

    public async virtual Task<string> SaveFileAsync(Stream file, string relativePath)
    {
        Logger.Information("����� �� ���������� ����� � ������� ������.");
        string fileName = Guid.NewGuid().ToString();
        ChangeDailyPath();
        string fileRelativePath = "/" + relativePath + dailyFolder;
        var fullPath = CheckDirectory(fileRelativePath);
        var result = await SaveToAsync(file, fullPath, fileName);
        if (result)
        {
            Logger.Information("���� ��� ���������� � ������� ������.");
            return fileRelativePath + fileName;
        }
        Logger.Error("���� �� ��� ���������� � ������� ������.");
        return string.Empty;
    }
    public void ChangeDailyPath()
    {
        if (currentTime.Day != DateTime.Now.Day)
        {
            currentTime = DateTime.Now;
            dailyFolder = "/" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day + "/";
            Logger.Information("���� �� ����� ��� ������� �� ����� ����={dailyFolder}");
        }
    }
    public string CheckDirectory(string fileRelativePath)
    {
        var fullPath = currentDirectory + fileRelativePath;
        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
            Logger.Information("���� �������� ���� �����. ���� �� ����� ->/" + fileRelativePath);
        }
        return fullPath;
    }
    public void DeleteFile(string relativePath)
    {
        if (File.Exists(currentDirectory + relativePath))
        {
            File.Delete(currentDirectory + relativePath);
            Logger.Information("���� ��� ��������� � ������� �������.");
        }
    }
    public async virtual Task<bool> SaveToAsync(Stream file, string fullPath, string fileName)
    {
        var fileFullPathToSave = fullPath + fileName;
        if (File.Exists(fileFullPathToSave))
        {
            Logger.Error("������ �� ���� �������� � ������� ������ ���� � ����� ����� ������.");
            return false;
        }
        try
        {
            using (var stream = new FileStream(fileFullPathToSave, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            Logger.Information($"��� ��������� ����� ����={fileName}.");
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error($"������� ��� ��������� �����: {ex.Message}");
            return false;
        }
    }

}