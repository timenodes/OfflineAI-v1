

namespace OfflineAI.Models
{
    public class FileOperationModel
    {
        /// <summary>
        /// 是否生成目录
        /// </summary>
        public bool IsGenerateDirectory {  get; set; }
        /// <summary>
        /// 文件目录
        /// </summary>
        public string Directory {  get; set; }
        /// <summary>
        /// 日期目录（生成的目录）
        /// </summary>
        public string DirectoryDateTime { get; set; }
        /// <summary>
        /// 文件名称（全路径）
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 文件名称（生成文件全路径）
        /// </summary>
        public string FileNameDateTime { get; set; }
    }
}
