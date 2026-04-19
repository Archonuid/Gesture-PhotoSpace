using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UDPReceiver : MonoBehaviour
{
    UdpClient client;

    public bool leftHandDetected;
    public bool rightHandDetected;
    public float receivedAngle;
    public float pinchDistance;
    public bool imageOpen;
    public float rightDX;
    public float rightDY;

    void Start()
    {
        client = new UdpClient(5055);
        client.BeginReceive(ReceiveData, null);
    }

    void ReceiveData(System.IAsyncResult result)
    {
        IPEndPoint ep = new IPEndPoint(IPAddress.Any, 5055);

        byte[] data = client.EndReceive(result, ref ep);

        string message = Encoding.UTF8.GetString(data);

        ParseMessage(message);

        client.BeginReceive(ReceiveData, null);
    }

    void ParseMessage(string message)
    {
        if (message.StartsWith("LEFT:"))
        {
            rightHandDetected = false;

            string value = message.Replace("LEFT:", "");

            float.TryParse(value, out receivedAngle);

            leftHandDetected = true;
        }

        else if (message.StartsWith("RIGHT:"))
        {
            leftHandDetected = false;

            string value = message.Replace("RIGHT:", "");

            string[] parts = value.Split(',');

            if (parts.Length == 2)
            {
                float.TryParse(parts[0], out rightDX);
                float.TryParse(parts[1], out rightDY);
            }

            rightHandDetected = true;
        }

        else if (message.StartsWith("PINCH:"))
        {
            string value = message.Replace("PINCH:", "");

            float.TryParse(value, out pinchDistance);
        }

        else if (message == "IMAGE_OPEN")
        {
            imageOpen = true;
        }

        else if (message == "IMAGE_CLOSE")
        {
            imageOpen = false;
        }

        else if (message == "NO_HAND")
        {
            leftHandDetected = false;
            rightHandDetected = false;
            imageOpen = false;
        }
    }
}