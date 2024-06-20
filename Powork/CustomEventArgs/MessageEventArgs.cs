namespace Powork.CustomEventArgs
{
    public class MessageEventArgs : EventArgs
    {
        public bool Received { get; set; } = false;

        public MessageEventArgs()
        {
        }
    }
}
