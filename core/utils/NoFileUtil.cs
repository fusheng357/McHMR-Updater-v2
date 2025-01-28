using McHMR_Updater_v2.core.entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace McHMR_Updater_v2.core.utils
{
    public class NoFileUtil
    {
        public async Task<List<string>> CheckFiles(List<HashEntity> hashPath, string whiteList, string fileDir)
        {
            List<string> notInListFiles = new List<string>();
            HashSet<string> tempWhiteListSet = new HashSet<string>();
            string[] whitelistArrayBefore = whiteList.Split(Environment.NewLine.ToCharArray());
            string[] whitelistArrayAfter = whitelistArrayBefore.Where(s => !string.IsNullOrEmpty(s)).ToArray();
            HashSet<string> hashPathSet = new HashSet<string>();
            HashSet<string> whiteListSet = new HashSet<string>();

            foreach (string entry in whitelistArrayAfter)
            {
                string tempPath = entry;
                if (tempPath.Length > 0 && (tempPath[0] == '/' || tempPath[0] == '\\'))
                {
                    tempPath = entry.Substring(1);
                }
                tempPath = tempPath.Replace('/', '\\');
                string filePath = Path.Combine(fileDir, tempPath);

                string directoryPath = Path.GetDirectoryName(filePath);
                string pattern = Path.GetFileName(filePath);

                if (string.IsNullOrEmpty(directoryPath))
                {
                    directoryPath = fileDir;
                }

                // 获取匹配模式的文件
                if (Directory.Exists(directoryPath))
                {
                    foreach (var file in Directory.GetFiles(directoryPath, pattern))
                    {
                        tempWhiteListSet.Add(file);
                    }

                    // 如果匹配模式也是目录，还需要继续递归处理该目录下的所有文件
                    foreach (var dir in Directory.GetDirectories(directoryPath, pattern))
                    {
                        AddDirectoryFilesToSet(dir, tempWhiteListSet);
                    }
                }
            }

            foreach (string entry in tempWhiteListSet)
            {
                whiteListSet.Add(entry.Replace("/", "\\"));
            }

            foreach (HashEntity entry in hashPath)
            {
                string relativePath = fileDir + entry.filePath;
                hashPathSet.Add(relativePath.Replace("/", "\\"));
            }

            string[] filesInParam3 = Directory.GetFiles(fileDir, "*", SearchOption.AllDirectories);

            foreach (string file in filesInParam3)
            {
                if (!hashPathSet.Contains(file) && !whiteListSet.Contains(file))
                {
                    notInListFiles.Add(file);
                }
            }

            return notInListFiles;
        }
        private void AddDirectoryFilesToSet(string directory, HashSet<string> fileSet)
        {
            string[] files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);

            foreach (string file in files)
            {
                fileSet.Add(file);
            }
        }

        public async Task RemoveEmptyDirectories(string startLocation)
        {
            foreach (var directory in Directory.GetDirectories(startLocation))
            {
                // 递归调用以检查子目录
                await RemoveEmptyDirectories(directory);

                // 如果目录是空的（没有文件和子目录），则删除它
                if (Directory.GetFiles(directory).Length == 0 && Directory.GetDirectories(directory).Length == 0)
                {
                    Directory.Delete(directory);
                    Console.WriteLine($"Deleted empty directory: {directory}");
                }
            }
        }
    }
}
