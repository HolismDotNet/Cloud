using System;
using System.Collections.Generic;
using System.Text;

namespace Holism.Azure.SyncOperator
{
    public class Parameters
    {
        public string StorageKey { get; set; }

        public List<Container> Containers { get; set; }

        public string LocalPath { get; set; }
    }
}
