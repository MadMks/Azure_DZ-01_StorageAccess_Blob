﻿using Microsoft.WindowsAzure.Storage;
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

            Console.WriteLine(container.Name);
            Console.WriteLine(container.Uri.AbsolutePath);
            Console.WriteLine("====\n\n\n");


            TestDownload(container);

            Console.WriteLine("\n\n\n>>> Succeed");
            Console.ReadKey();
        }

        private static async Task TestDownload(CloudBlobContainer container)
        {
            Console.WriteLine("[---] container.Uri.AbsolutePath: " 
                + container.Uri.AbsolutePath + "\n\n");

            int containerPathLength = container.Uri.AbsolutePath.Length;

            foreach (IListBlobItem blob in container.ListBlobs("", true))
            {
                Console.WriteLine("[---] Полный путь файла: " + blob.Uri.AbsolutePath);


                string blobName
                    = blob.Uri.AbsolutePath.Substring(
                        containerPathLength + 1);

                Console.WriteLine("blobName = " + blobName);
                // Получение ссылки.
                var reference = container.GetBlobReferenceFromServer(
                    blobName);

                // Создание всех нужных директорий.
                CreateAllFolder(blobName);

                // Скачивание Blob.
                await reference.DownloadToFileAsync(blobName, FileMode.Create);
            }
        }

        private static void CreateAllFolder(string blobName)
        {
            string[] folders = blobName.Split('/');
            string path = "";
            if (folders.Length > 1)
            {
                for (int i = 0; i < folders.Length - 1; i++)
                {
                    path += folders[i] + "/";
                    Console.WriteLine("path = " + path);
                    Directory.CreateDirectory(path);
                }
            }
        }

    }
}
