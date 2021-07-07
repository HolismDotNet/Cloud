using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Holism.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace Holism.Azure
{
    public class Storage
    {
        // todo: Change uploads to take bytes array, instead of bytes, to make things easier and reduce boilerplate code.
        public const string DefaultStorageConnectionStringKey = "StorageConnectionString";

        public static void UploadVideo(byte[] bytes, Guid guid, string containerName)
        {
            UploadVideo(DefaultStorageConnectionStringKey, bytes, guid, containerName);
        }

        public static void UploadVideo(string connectionStringKey, byte[] bytes, Guid guid, string containerName)
        {
            Upload(connectionStringKey, bytes, guid.ToString() + ".mp4", containerName, "video/mp4");
        }

        public static void UploadImage(byte[] bytes, Guid guid, string containerName)
        {
            UploadImage(DefaultStorageConnectionStringKey, bytes, guid, containerName);
        }

        public static void UploadImage(string connectionStringKey, byte[] bytes, Guid guid, string containerName)
        {
            Upload(connectionStringKey, bytes, guid.ToString() + ".png", containerName, "image/png");
        }

        public static void UploadImageAndCreateThumbnail(byte[] bytes, Guid guid, string containerName, int? maxWith, int? maxHeight)
        {
            UploadImage(DefaultStorageConnectionStringKey, bytes, guid, containerName);
            throw new NotImplementedException();
            // var thumbnail = ImageHelper.MakeImageThumbnail(maxWith, maxHeight, bytes).GetBytes();
            // Upload(DefaultStorageConnectionStringKey, thumbnail, guid.ToString() + "_thumbnail.png", containerName, "image/png");
        }

        public static void UploadImageAndCreateThumbnail(string connectionStringKey, byte[] bytes, Guid guid, string containerName, int? maxWith, int? maxHeight)
        {
            UploadImageAndCreateThumbnail(DefaultStorageConnectionStringKey, bytes, guid, containerName, maxWith, maxHeight);
        }

        public static void UploadAudio(byte[] bytes, Guid guid, string containerName)
        {
            UploadAudio(DefaultStorageConnectionStringKey, bytes, guid, containerName);
        }

        public static void UploadAudio(string connectionStringKey, byte[] bytes, Guid guid, string containerName)
        {
            Upload(connectionStringKey, bytes, guid.ToString() + ".mp3", containerName, "audio/mpeg");
        }

        public static void UploadFile(byte[] bytes, string fileName, string extension, string containerName)
        {
            UploadFile(DefaultStorageConnectionStringKey, bytes, fileName, extension, containerName);
        }

        public static void UploadFile(string connectionStringKey, byte[] bytes, string fileName, string extension, string containerName)
        {
            extension = extension.Trim('.');
            Upload(connectionStringKey, bytes, fileName + "." + extension, containerName, "application/octet-stream");
        }

        private static void Upload(byte[] bytes, string fileName, string containerName, string contentType = null)
        {
            Upload(DefaultStorageConnectionStringKey, bytes, fileName, containerName, contentType);
        }

        private static void Upload(string connectionStringKey, byte[] bytes, string fileName, string containerName, string contentType = null)
        {
            try
            {

                BlobContainerClient container = GetContainer(connectionStringKey, containerName);
                BlobClient blob = container.GetBlobClient(fileName);
                var blobHttpHeaders = new BlobHttpHeaders();
                if (contentType != null)
                {
                    blobHttpHeaders.ContentType = contentType;
                }
                blob.UploadAsync(new MemoryStream(bytes), blobHttpHeaders).Wait();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                BlobContainerClient container = GetContainer(connectionStringKey, containerName);
                TryToCreateContainerAndSetAccess(container);
                BlobClient blockBlob = container.GetBlobClient(fileName);
                var blobHttpHeaders = new BlobHttpHeaders();
                if (contentType != null)
                {
                    blobHttpHeaders.ContentType = contentType;
                }
                blockBlob.UploadAsync(new MemoryStream(bytes), blobHttpHeaders).Wait();
            }
        }

        private static void TryToCreateContainerAndSetAccess(BlobContainerClient container)
        {
            Logger.LogInfo($"Trying to create container {container.Uri.AbsoluteUri} ...");
            var created = container.CreateIfNotExists();
            BlobContainerAccessPolicy accessPolicy = container.GetAccessPolicy();
            container.SetAccessPolicy(PublicAccessType.BlobContainer);
        }

        public static void SetContentType(string containerName, string fileName, string contentType)
        {
            var container = GetContainer(containerName);
            var blob = container.GetBlobClient(fileName);
            blob.SetHttpHeaders(new BlobHttpHeaders { ContentType = contentType });
        }

        public static List<string> GetBlobs(string containerName)
        {
            return GetBlobs(DefaultStorageConnectionStringKey, containerName);
        }

        public static List<string> GetBlobs(string connectionStringKey, string containerName)
        {
            BlobContainerClient container = GetContainer(connectionStringKey, containerName);
            var result = new List<string>();
            int i = 0;
            var resultSegment = container.GetBlobs().AsPages();
            foreach (var page in resultSegment)
            {
                if (page.Values.Count<BlobItem>() > 0) { Logger.LogInfo($"Page {++i}"); }
                foreach (var blobItem in page.Values)
                {
                    result.Add(((BlobItem)blobItem).Name);
                }
            }
            return result;
        }

        public static void Delete(string containerName, Guid token, string extension)
        {
            Delete(DefaultStorageConnectionStringKey, containerName, token, extension);
        }

        public static void Delete(string connectionStringKey, string containerName, Guid token, string extension)
        {
            var container = GetContainer(connectionStringKey, containerName);
            if (!extension.StartsWith("."))
            {
                extension = "." + extension;
            }
            var blockBlock = container.GetBlobClient(token.ToString() + extension);
            var deleted = blockBlock.DeleteIfExistsAsync().Result;
        }

        public static void DeleteUsingBlobName(string containerName, string blobName)
        {
            var container = GetContainer(DefaultStorageConnectionStringKey, containerName);
            var blockBlock = container.GetBlobClient(blobName);
            var deleted = blockBlock.DeleteIfExists(DeleteSnapshotsOption.IncludeSnapshots);
        }

        public static void DeleteImage(string containerName, Guid token)
        {
            DeleteImage(DefaultStorageConnectionStringKey, containerName, token);
        }

        public static void DeleteImage(string connectionStringKey, string containerName, Guid token)
        {
            Delete(containerName, token, ".png");
        }

        public static void DeleteAudio(string containerName, Guid token)
        {
            Delete(containerName, token, ".mp3");
        }

        public static void DeleteAudio(string connectionStringKey, string containerName, Guid token)
        {
            Delete(containerName, token, ".mp3");
        }

        public static void DeleteVideo(string containerName, Guid token)
        {
            Delete(containerName, token, ".mp4");
        }

        public static void DeleteVideo(string connectionStringKey, string containerName, Guid token)
        {
            Delete(containerName, token, ".mp4");
        }

        public static void Rename(string containerName, string oldName, string newName)
        {
            Rename(DefaultStorageConnectionStringKey, containerName, oldName, newName);
        }

        public static void Rename(string connectionStringKey, string containerName, string oldName, string newName)
        {
            var container = GetContainer(connectionStringKey, containerName);
            BlobClient newBlob = container.GetBlobClient(newName);
            if (!newBlob.ExistsAsync().Result)
            {
                BlobClient oldBlob = container.GetBlobClient(oldName);
                if (oldBlob.ExistsAsync().Result)
                {
                    var copyResult = newBlob.StartCopyFromUri(oldBlob.Uri);
                    while (!copyResult.HasCompleted)
                    {
                        Logger.LogInfo("Waiting for copying to complete...");
                        Thread.Sleep(500);
                    }
                    var deleteResult = oldBlob.DeleteIfExists();
                }
            }
        }

        public static void CreateContainerIfNotExists(string containerName)
        {
            CreateContainerIfNotExists(DefaultStorageConnectionStringKey, containerName);
        }

        public static void CreateContainerIfNotExists(string connectionStringKey, string containerName)
        {
            var container = GetContainer(connectionStringKey, containerName);
            container.CreateIfNotExists();
            container.SetAccessPolicy(PublicAccessType.BlobContainer);
        }

        public static byte[] Get(Guid token)
        {
            try
            {
                var bytes = new WebClient().DownloadData(GetUrl(null, token, null));
                return bytes;
            }
            catch
            {
                Logger.LogError($"Error in getting {token} from Azure");
                throw;
            }
        }

        public static string GetImageUrl(string container, Guid token)
        {
            return GetUrl(DefaultStorageConnectionStringKey, container, token, ".png");
        }

        public static string GetImageUrl(string connectionStringKey, string container, Guid token)
        {
            return GetUrl(connectionStringKey, container, token, ".png");
        }

        public static string GetImageThumbnailUrl(string container, Guid guid)
        {
            return GetUrl(DefaultStorageConnectionStringKey, container, guid.ToString() + "_thumbnail" + ".png");
        }

        public static string GetAudioUrl(string container, Guid token)
        {
            return GetUrl(DefaultStorageConnectionStringKey, container, token, ".mp3");
        }

        public static string GetAudioUrl(string connectionStringKey, string container, Guid token)
        {
            return GetUrl(connectionStringKey, container, token, ".mp3");
        }

        public static string GetVideoUrl(string container, Guid token)
        {
            return GetUrl(DefaultStorageConnectionStringKey, container, token, ".mp4");
        }

        public static string GetVideoUrl(string connectionStringKey, string container, Guid token)
        {
            return GetUrl(connectionStringKey, container, token, ".mp4");
        }

        private static string GetUrl(string container, Guid token, string extension)
        {
            return GetUrl(DefaultStorageConnectionStringKey, container, token, extension);
        }

        private static string GetUrl(string connectionStringKey, string container, Guid token, string extension)
        {
            var fileName = token.ToString();
            if (extension.IsSomething())
            {
                if (extension.StartsWith("."))
                {
                    fileName += extension;
                }
                else
                {
                    fileName += "." + extension;
                }
            }
            return GetUrl(connectionStringKey, container, fileName);
        }

        private static string GetUrl(string connectionStringKey, string container, string fileName)
        {
            var path = "";
            if (container.IsSomething())
            {
                path = "/" + container;
            }
            path += "/" + fileName;
            var url = $"{GetBaseUrl(connectionStringKey, container).TrimEnd('/')}/{path.TrimStart('/')}";
            return url;
        }

        private static string GetBaseUrl(string connectionStringKey, string container)
        {
            EnsureThisKeyIsDefined(connectionStringKey);
            var cacheKey = connectionStringKey.Replace("ConnectionString", "") + "Cache";
            var cachedContainersKey = connectionStringKey.Replace("ConnectionString", "") + "CachedContainers";
            if (Config.HasSetting(cacheKey))
            {
                if (Config.HasSetting(cachedContainersKey))
                {
                    if (Config.GetSetting(cachedContainersKey).Contains(container))
                    {
                        return Config.GetSetting(cacheKey);
                    }
                }
                else
                {
                    return Config.GetSetting(cacheKey);
                }
            }
            BlobServiceClient storageAccount = new BlobServiceClient(Config.GetSetting(connectionStringKey));
            return storageAccount.Uri.ToString();
        }

        private static BlobContainerClient GetContainer(string containerName)
        {
            return GetContainer(DefaultStorageConnectionStringKey, containerName);
        }

        private static BlobContainerClient GetContainer(string connectionStringKey, string containerName)
        {
            if (containerName.ToArray().Any(i => char.IsUpper(i)))
            {
                throw new ServerException($"container name should be all lowercase, and all alphanumeric. @{containerName} is not valid.");
            }
            EnsureThisKeyIsDefined(connectionStringKey);
            BlobServiceClient storageAccount = new BlobServiceClient(Config.GetSetting(connectionStringKey));
            BlobContainerClient container = storageAccount.GetBlobContainerClient(containerName);
            return container;
        }

        public static List<string> GetContainers()
        {
            return GetContainers(DefaultStorageConnectionStringKey);
        }

        public static List<string> GetContainers(string connectionStringKey)
        {
            EnsureThisKeyIsDefined(connectionStringKey);
            BlobServiceClient storageAccount = new BlobServiceClient(Config.GetSetting(connectionStringKey));
            var result = new List<string>();
            int i = 0;
            var pages = storageAccount.GetBlobContainers().AsPages();
            foreach (var page in pages)
            {
                if (page.Values.Count() > 0) { Logger.LogInfo($"Page {++i}"); }
                foreach (var container in page.Values)
                {
                    result.Add(container.Name);
                }
            }
            return result;
        }

        private static void EnsureThisKeyIsDefined(string connectionStringKey)
        {
            if (!connectionStringKey.EndsWith("ConnectionString"))
            {
                throw new ServerException("Any storage connection string key should end with ConnectionString");
            }
            Config.GetSetting(connectionStringKey);
        }

        public static void Copy(string sourceContainerName, string sourceBlobName, string destinationContainerName, string destinationBlobName)
        {
            var sourceContainer = GetContainer(sourceContainerName);
            var sourceBlob = sourceContainer.GetBlobClient(sourceBlobName);
            var destinationContainer = GetContainer(destinationContainerName);
            var destinationBlob = destinationContainer.GetBlobClient(destinationBlobName);
            var result = destinationBlob.StartCopyFromUri(sourceBlob.Uri);
        }

        public static void Download(string containerName, string blobName, string localFilePath)
        {
            var container = GetContainer(containerName);
            var blob = container.GetBlobClient(blobName);
            blob.DownloadTo(localFilePath);
        }
    }
}
