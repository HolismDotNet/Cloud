// using Holism.Console;
// using Infra;

// namespace Holism.Azure.ContentTypeFixer
// {
//     public class Program
//     {
//         static void Main(string[] args)
//         {
//             InfraConsole.Configure();

//             var containers = Storage.GetContainers();
//             foreach (var container in containers)
//             {
//                 SetCotnainerBlobsContentType(container);
//             }
//         }

//         private static void SetCotnainerBlobsContentType(string container)
//         {
//             var blobs = Storage.GetBlobs(container);
//             foreach (var blob in blobs)
//             {
//                 SetBlobContentType(container, blob);
//             }
//         }

//         private static void SetBlobContentType(string container, string blob)
//         {
//             if (blob.EndsWith(".png"))
//             {
//                 Storage.SetContentType(container, blob, "image/png");
//                 Logger.LogSuccess($"Set content type of {container}/{blob}");
//             }
//             else if (blob.EndsWith(".mp3"))
//             {
//                 Storage.SetContentType(container, blob, "audio/mpeg");
//                 Logger.LogSuccess($"Set content type of {container}/{blob}");
//             }
//         }
//     }
// }
