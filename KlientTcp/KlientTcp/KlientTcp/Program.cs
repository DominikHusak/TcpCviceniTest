namespace KlientTcp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            MyTcpClient client = new MyTcpClient("127.0.0.1", 65525);
        }
    }
}