using Holism.Framework;
using Holism.Framework.Extensions;
using Holism.WindowsService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Holism.Azure.SyncOperator
{
    public class SyncOperationService : Service
    {
        public static Parameters Parameters { get; set; }

        public override void StartDoingTheJob()
        {
            ValidateParameters();
            while (IsUpAndRunning)
            {
                SyncStorage();
                var sleepTime = 5000;
                var sleepTimeConfigKey = "SleepTimeInMilliseconds";
                if (Config.HasSetting(sleepTimeConfigKey) && Config.GetSetting(sleepTimeConfigKey).IsNumeric())
                {
                    sleepTime = Config.GetSetting(sleepTimeConfigKey).ToInt();
                }
                Logger.LogInfo($"Synced storage. Now sleeping for {sleepTime} milliseconds");
                Thread.Sleep(sleepTime);
                GC.Collect();
            }
        }

        private void SyncStorage()
        {
            foreach (var container in Parameters.Containers)
            {
                try
                {
                    SyncContainer(container);
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Error in syncing container {container.Name}");
                    Logger.Log(ex);
                }
            }
        }

        private static void ValidateParameters()
        {
            if (Parameters.IsNull())
            {
                throw new FrameworkException($"{nameof(Parameters)} is null");
            }
            if (Parameters.Containers.Count == 0)
            {
                throw new FrameworkException($"No container is specified to be synced");
            }
            if (Parameters.LocalPath.IsNothing())
            {
                throw new FrameworkException($"Please specify a local path for syncing operation");
            }
            if (!Directory.Exists(Parameters.LocalPath))
            {
                throw new FrameworkException($"Local path {Parameters.LocalPath} does not exist");
            }
        }

        private void SyncContainer(Container container)
        {
            var directory = Path.Combine(Parameters.LocalPath, container.Name);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            var blobs = container.BlobListProvider().Where(i => i.IsSomething()).ToList();
            foreach (var blob in blobs)
            {
                try
                {
                    SyncBlob(container, blob);
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Error syncing blob {blob} from contianer {container.Name}");
                    Logger.Log(ex);
                }
            }
        }

        private void SyncBlob(Container container, string blob)
        {
            var blobLocalPath = Path.Combine(Parameters.LocalPath, container.Name, blob);
            if (File.Exists(blobLocalPath))
            {
                container.SyncedBlobs.Add(blob);
            }
            if (container.SyncedBlobs.Contains(blob))
            {
                //Logger.LogInfo($"Blob {blob} from container {container.Name} is already downloaded");
                return;
            }
            Logger.LogInfo($"Downloading blob {blob} from container {container.Name} into {blobLocalPath}...");
            Storage.Download(container.Name, blob, blobLocalPath);
            Logger.LogSuccess($"Blob {blob} from container {container.Name} is downloaded to {blobLocalPath}");
            container.SyncedBlobs.Add(blob);
        }

        public override void StopTheJob()
        {
            base.StopTheJob();
        }
    }
}
