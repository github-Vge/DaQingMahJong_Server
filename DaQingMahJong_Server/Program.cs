namespace DaQingMahJong_Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //启动服务器
            VgeGameServer.Network.Network.Start(8888);
        }
    }
}