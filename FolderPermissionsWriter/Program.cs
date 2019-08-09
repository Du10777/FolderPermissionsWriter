using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.AccessControl;

namespace FolderPermissionsWriter
{
    class Program
    {
        //Программа нужна для одной цели:
        //Обеспечить, что бы в указанном каталоге, во всех его подкаталогах были определенные папки с нужными правами
        //Принцип работы:
        //При запуске, программа проверяет все подкаталоги в указанном каталоге, и если какого-то нехватает, создаёт его
        //Так-же всем этим каталогам ставит указанные (в каталоге-шаблоне) права
        static void Main(string[] args)
        {
            Config.SilentMode = IsSilent(args);
            Config.Open();
            Log.Open(Config.LogsFileName);

            CheckFolder();

            Log.Close();
        }

        static bool IsSilent(string[] args)
        {
            if (args.Length < 1)
                return false;
            if (args[0].ToLower() == "-silent")
                return true;

            return false;
        }

        static void CheckFolder()
        {
            string[] list = Directory.GetDirectories(Config.Folder);
            
            foreach (string ToSubFolder in list)
            {
                if (IsFolderInExceptionsList(ToSubFolder))
                    continue;//Пропустить каталоги-исключения

                CopyFolders(ToSubFolder);
            }
        }

        static void CopyFolders(string ToFolder)
        {
            if (!Directory.Exists(ToFolder))////Если, пока работала программа, папка назначения был переименованна или удалена
                return;//Пропустить создание подкатологов

            CopyAccessRules(Config.TemplateFolder, ToFolder);

            string[] list = Directory.GetDirectories(Config.TemplateFolder);
            foreach (string folder in list)
            {
                string SubDirName = folder.Remove(0, Config.TemplateFolder.Length);

                CopyAccessRules(folder, Path.Combine(ToFolder, SubDirName));
            }
        }

        static bool IsFolderInExceptionsList(string folderName)
        {
            folderName = folderName.ToLower();
            foreach (string excl in Config.Exclusions)
            {
                if (folderName.StartsWith(excl))//Если файл является исключением или находится в папке исключения
                    return true;
            }
            return false;
        }

        static void CopyAccessRules(string From, string To)
        {
            try
            {
                DirectorySecurity Permisions = Directory.GetAccessControl(From);
                Permisions.SetAccessRuleProtection(true, false);

                Directory.CreateDirectory(To);
                Directory.SetAccessControl(To, Permisions);
            }
            catch (Exception ex)
            {
                string Message = "Source folder: " + From + "\r\n";
                Message += "Destination folder: " + To + "\r\n";
                Message += "Error: " + ex.Message;
                Log.Add(Message);
            }
        }
    }
}
