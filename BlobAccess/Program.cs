using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlobAccess
{
    class Program
    {
        static void Main(string[] args)
        {
            // Инициализация объектной модели хранилища BLOB-объектов

            // Получаем CloudStorageAccount
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(@"AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;DefaultEndpointsProtocol=http;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;");
            // Получаем CloudBlobClient
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            // Получаем CloudBlobContainer
            CloudBlobContainer container = blobClient.GetContainerReference("container1");

            Console.WriteLine(" Container Name: " + container.Name);
            Console.WriteLine(" Container AbsolutePath: " 
                + container.Uri.AbsolutePath);
            Console.WriteLine("====\n\n\n");

            

            Task taskDownload = Task.Run(() => DownloadFilesAndAllFolders(container));

            taskDownload.Wait();

            Console.WriteLine("Uri: " + GetBlobSasUri(container));

            
            Console.ReadKey();
        }

        private static async Task DownloadFilesAndAllFolders(CloudBlobContainer container)
        {
            int containerPathLength = container.Uri.AbsolutePath.Length;

            foreach (IListBlobItem blob in container.ListBlobs("", true))
            {
                Console.WriteLine("\n[---] Полный путь файла: " + blob.Uri.AbsolutePath);


                string blobName
                    = blob.Uri.AbsolutePath.Substring(
                        containerPathLength + 1);

                Console.WriteLine("> blobName = " + blobName);
                // Получение ссылки.
                var reference = container.GetBlobReferenceFromServer(
                    blobName);

                // Создание всех нужных директорий.
                CreateAllFolder(blobName);

                // Скачивание Blob.
                await reference.DownloadToFileAsync(blobName, FileMode.Create);
            }

            Console.WriteLine("\n\n\n>>> Download blob succeed\n\n\n");
        }

        /// <summary>
        /// Создание требуемых папок встречающихся в имени Blob.
        /// </summary>
        private static void CreateAllFolder(string blobName)
        {
            string[] folders = blobName.Split('/');
            string path = "";
            if (folders.Length > 1)
            {
                for (int i = 0; i < folders.Length - 1; i++)
                {
                    path += folders[i] + "/";
                    Console.WriteLine("> Create folder: " + path);
                    Directory.CreateDirectory(path);
                }
            }
        }

        static string GetBlobSasUri(CloudBlobContainer container)
        {
            // Получить ссылку на BLOB-объект внутри контейнера.
            CloudBlockBlob blob = container.GetBlockBlobReference("sasblob.txt");

            // Загрузить текст в BLOB-объект. Если блоб еще не существует,
            // он будет создан.
            // Если большой двоичный объект существует, 
            // его существующее содержимое будет перезаписано.
            string blobContent = "This blob will be accessible " +
                "to clients via a shared access signature (SAS).";
            blob.UploadText(blobContent);

            // Установка времени истечения и разрешения для BLOB - объекта.
            // В этом случае время запуска задается как несколько минут в прошлом, 
            // чтобы уменьшить перекос часов.
            // Подпись общего доступа будет действительной немедленно.
            SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy();
            sasConstraints.SharedAccessStartTime = DateTimeOffset.Now.AddMinutes(-5);
            sasConstraints.SharedAccessExpiryTime = DateTimeOffset.Now.AddHours(24 * 7);
            sasConstraints.Permissions = 
                SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.List;

            // Создание подписи общего доступа на BLOB-объекте,
            // установив ограничения непосредственно на подпись.
            string sasBlobToken = blob.GetSharedAccessSignature(sasConstraints);

            // Возвращает строку URI для контейнера, включая токен SAS.
            return blob.Uri + sasBlobToken;
        }

    }
}
