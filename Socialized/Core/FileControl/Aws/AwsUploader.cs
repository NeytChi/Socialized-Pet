using Serilog;
using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Core.FileControl.CurrentFileSystem;

namespace Core.FileControl.Aws
{
    public class AwsUploader : FileManager, IFileManager
    {
        private AwsSettings Settings { get; set; }
        private readonly RegionEndpoint Region;
        private readonly IAmazonS3 S3Client;
        private readonly TransferUtility FileTransferUtility;

        public AwsUploader(ILogger logger, AwsSettings settings) : base(logger)
        {
            Settings = settings;
            Region = RegionEndpoint.GetBySystemName(Settings.AwsBucketRegion);
            S3Client = new AmazonS3Client(Settings.AwsAccessKeyId, Settings.AwsSecretKeyId, Region);
            FileTransferUtility = new TransferUtility(S3Client);
            Logger.Information("AWS S3 Bucket ��� ������ �������������.");
        }
        public async override Task<string> SaveFileAsync(Stream file, string relativePath)
        {
            Logger.Information("����� �� ���������� ����� �� ����� AWS S3 Bucket.");
            ChangeDailyPath();
            var fileName = Guid.NewGuid().ToString();
            var fullPath = "/" + relativePath + dailyFolder;
            var result = await SaveToAsync(file, fullPath, fileName);
            if (result)
            {
                Logger.Information("���� ��� ���������� �� ����� AWS S3 Bucket.");
                return fullPath + fileName;
            }
            Logger.Error("���� �� ��� ���������� �� ����� AWS S3 Bucket.");
            return string.Empty;
        }
        public async override Task<bool> SaveToAsync(Stream stream, string relativeFilePath, string fileName)
        {
            Logger.Information($"����� �� ���������� ����� �� ����� AWS S3 Bucket �� ����� ������={relativeFilePath}.");
            try
            {
                var fileTransferUtilityRequest = new TransferUtilityUploadRequest
                {
                    BucketName = Settings.AwsBucketName,
                    InputStream = stream,
                    StorageClass = S3StorageClass.StandardInfrequentAccess,
                    PartSize = stream.Length,
                    Key = relativeFilePath + fileName,
                    CannedACL = S3CannedACL.PublicRead
                };
                fileTransferUtilityRequest.Metadata.Add("param1", "Value1");
                fileTransferUtilityRequest.Metadata.Add("param2", "Value2");
                await FileTransferUtility.UploadAsync(fileTransferUtilityRequest);
                Logger.Information($"��� �������� ����� ���� �� ����� AWS S3 Bucket �� ����� ������={relativeFilePath}.");
                return true;
            }
            catch (AmazonS3Exception e)
            {
                Logger.Error($"�� ������� ������� ���� �� ������ AWS S3 Bucket, AmazonS3 ����������={e.Message}");
            }
            catch (Exception e)
            {
                Logger.Error($"�� ������� ������� ���� �� ������ AWS S3 Bucket, ����������={e.Message}");
            }
            return false;
        }
    }
}