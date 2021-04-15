using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FolderPermissionsWriter
{
    class DB
    {
        static List<string> FoldersList = new List<string>();
        static List<string> newFoldersList = new List<string>();

        static bool[] FoundedFoldersMarks = new bool[0];
        static bool FoldersListChanged = false;

        public static void Open()
        {
            if (!File.Exists(Config.DBLocation))//Если базы нет
                return;//Выйти. Будем работать с пустой базой, а потом просто запишем новую

            string[] list = File.ReadAllLines(Config.DBLocation);

            FoldersList = new List<string>(list);
            FoundedFoldersMarks = new bool[FoldersList.Count];
        }

        public static void Save()
        {
            DeleteOldRecords();
            FoldersList.AddRange(newFoldersList);
            if(FoldersListChanged)
                FoldersList.Sort();

            string[] list = FoldersList.ToArray();

            File.WriteAllLines(Config.DBLocation, list);
        }

        public static bool IsFolderAlreadyProcessed(string FolderPath)
        {
            //FolderPath: D:\Folder1\Folder2
            //FoldersList[0]: Folder2

            //Strip Folder Path
            string FolderName = Path.GetFileName(FolderPath);

            int FoundedIndex = FoldersList.BinarySearch(FolderName);
            if (FoundedIndex >= 0)//Founded
            {
                FoundedFoldersMarks[FoundedIndex] = true;//Mark as founded
                return true;
            }

            Log.Add("Не могу найти в базе, и поэтому обработаю Папку: " + FolderPath);
            newFoldersList.Add(FolderName);
            FoldersListChanged = true;//Пометить, что список был изменен. Значит, потом его надо будет отсортировать

            return false;
        }


        public static void DeleteOldRecords()
        {
            if (FoundedFoldersMarks.Length != FoldersList.Count)
            {
                Log.Add("Размер массива базы был изменен во время работы программы. Это не даёт удалить не актуальные записи из этой базы");
                return;
            }


            for (int i = 0; i < FoldersList.Count; i++)
            {
                if (FoundedFoldersMarks[i] == false)//Если в процессе работы эта запись ни разу не была найдена
                {//Эта запись старая, и её надо удалить

                    FoldersListChanged = true;//Пометить, что список был изменен. Значит, потом его надо будет отсортировать

                    FoldersList.RemoveAt(i);//Удалить старую запись из базы
                    i--;
                }
            }
        }
    }
}
