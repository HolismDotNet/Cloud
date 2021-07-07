using System;
using System.Collections.Generic;
using System.Text;

namespace Holism.Azure.SyncOperator
{
    public class Container
    {
        public Container()
        {
            SyncedBlobs = new List<string>();
        }

        public string Name { get; set; }

        public Func<List<string>> BlobListProvider { get; set; }

        public List<string> SyncedBlobs { get; set; }
    }
}
