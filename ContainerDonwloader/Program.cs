using Holism.Azure.SyncOperator;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Holism.Azure.ContainerDonwloader
{
    public class Program
    {
        public static void Main(string[] args)
        {
            SyncOperationService.Parameters = new Parameters
            {
                Containers = new List<Container>
                {
                    new Container
                    {
                        Name = "databasepermanentbackups",
                        BlobListProvider = () => Storage.GetBlobs("databasepermanentbackups")
                    }
                },
                LocalPath = Framework.Config.GetSetting("LocalFolder")
            };
            WindowsService.Helper.Run<SyncOperationService>();
        }
    }
}
