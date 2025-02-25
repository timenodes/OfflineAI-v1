using OfflineAI.Models;
using System.IO;

namespace OfflineAI.Services
{
    /// <summary>
    /// 文件操作类：
    /// 1、2025-02-24：添加创建日期目录方法。输入文件名，添加时间目录。
    /// 2、2025-02-24：添加写入数据到文件方法（.txt格式）
    /// </summary>
    public class FileOperation
    {
        private FileOperationModel _fileOperation;

        #region 构造函数
        public FileOperation(string fileName)
        {
            _fileOperation = new FileOperationModel();
            _fileOperation.IsGenerateDirectory = true;
            UpdataFileName(fileName);
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 更新文件名
        /// </summary>
        public void UpdataFileName(string fileName)
        {
            if (Path.GetExtension(fileName).ToLower().Equals("txt"))
                _fileOperation.FileName = fileName;
            else
                _fileOperation.FileName = fileName + ".txt";
            _fileOperation.Directory = Path.GetDirectoryName(fileName);
            CreateDateTime();
            _fileOperation.FileNameDateTime = $"{_fileOperation.DirectoryDateTime}\\{Path.GetFileName(_fileOperation.FileName)}";
        }

        /// <summary>
        /// 写入文本
        /// </summary>
        public void WriteTxt(string data)
        {
            SaveDataAsTxt(data);
        }

        /// <summary>
        /// 写入文本，指定文件名
        /// </summary>
        public void WriteTxt(string fileName, string data)
        {
            UpdataFileName(fileName);
            SaveDataAsTxt(data);
        }

        public string ReadTxt(string fileName)
        {
            // 使用 using 语句确保资源被正确释放
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            using (StreamReader sr = new StreamReader(fs))
            {
                return sr.ReadToEnd();
            }
        }

        /// <summary>
        /// 获取指定目录下的所有文件（*.txt）
        /// </summary>
        public string[] GetFiles()
        {
            string[] files = Directory.GetFiles(_fileOperation.Directory, "*.txt", SearchOption.AllDirectories);
            return files;
        }
        /// <summary>
        /// 获取指定目录下的所有文件（*.txt）
        /// </summary>
        public static string[] GetFiles(string directory)
        {
            string[] files = Directory.GetFiles(directory, "*.txt", SearchOption.AllDirectories);
            return files;
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 保存数据为Txt类型的文本
        /// </summary>
        private void SaveDataAsTxt(string data)
        {
            if (_fileOperation.IsGenerateDirectory)
            {
                try
                {
                    string fileName = _fileOperation.FileName;
                    if (_fileOperation.IsGenerateDirectory)
                    {
                        fileName = _fileOperation.FileNameDateTime;
                    }
                    using (FileStream fileStream = new FileStream(fileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    {
                        using (StreamWriter writer = new StreamWriter(fileStream))
                        {
                            writer.Write(data);
                        }
                    }
                    Console.WriteLine("数据已成功写入文件。");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("写入文件时发生错误: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// 创建日期目录
        /// </summary>
        private void CreateDateTime()
        {
            if (_fileOperation.IsGenerateDirectory)
            {
                string path = $"{_fileOperation.Directory}\\{DateTime.Now.ToString("yyyy")}";
                Directory.CreateDirectory($"{path}");
                path = $"{path}\\{DateTime.Now.ToString("yyyyMMdd")}\\";
                Directory.CreateDirectory($"{path}");
                _fileOperation.DirectoryDateTime = path;

            }
        }
        #endregion
    }
}
