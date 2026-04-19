using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class GestureReceiver : MonoBehaviour
{
    UdpClient client;

    public float moveX;
    public float moveY;

    void Start()
    {
        client = new UdpClient(5052);
        client.BeginReceive(ReceiveData,null);
    }

    void ReceiveData(System.IAsyncResult result)
    {
        IPEndPoint ep = new IPEndPoint(IPAddress.Any,5052);

        byte[] data = client.EndReceive(result,ref ep);

        string msg = Encoding.UTF8.GetString(data);

        string[] parts = msg.Split(',');

        float.TryParse(parts[0],out moveX);
        float.TryParse(parts[1],out moveY);

        client.BeginReceive(ReceiveData,null);
    }
}