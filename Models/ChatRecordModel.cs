namespace OfflineAI.Models
{
    /// <summary>
    /// 聊天记录模型
    /// </summary>
    public class ChatRecordModel
    {
        public ChatRecordModel(int id, string dateTime, string name,string fullName, string data)
        {
            Id = id;
            DateTime = dateTime;
            Name = name;
            FullName = fullName;
            Data = data;
        }
        /// <summary>
        /// ID
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 日期
        /// </summary>
        public string DateTime { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 完整名称
        /// </summary>
        public string FullName { get; set; }
        /// <summary>
        /// 数据
        /// </summary>
        public string Data { get; set; }
    }
}
