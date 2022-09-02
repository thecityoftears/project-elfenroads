namespace Elfencore.Shared.Messages
{

    /// <summary> Used to decode/encode messages to/from server </summary>
    public class Message
    {
        public Message(string messageName, string value)
        {
            this.Tag = messageName;
            this.Content = value;
        }
        //public long pID { get; set; }
        public string Tag { get; set; }
        public string Content { get; set; }
    }
}