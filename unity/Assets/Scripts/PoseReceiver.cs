using System;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class PoseReceiver : MonoBehaviour
{
    TcpClient client;
    StreamReader reader;

    public float positionSmoothSpeed = 5f;
    public float rotationSmoothSpeed = 5f;

    private Vector3 targetPosition = Vector3.zero;
    private Quaternion targetRotation = Quaternion.identity;

    // ����� ������ ������ ����
    public Vector3 positionOffset = Vector3.zero;
    public Quaternion rotationOffset = Quaternion.identity;

    async void Start()
    {
        try
        {
            string serverIP = "192.168.1.41";
            int serverPort = 5005;

            client = new TcpClient();
            Debug.Log("Python ���� ���� �õ� ��...");
            await client.ConnectAsync(serverIP, serverPort);

            NetworkStream stream = client.GetStream();
            reader = new StreamReader(stream, Encoding.UTF8);

            Debug.Log("Python ���� ���� ����");

            _ = Task.Run(() => ReceiveLoop());
        }
        catch (Exception e)
        {
            Debug.LogError("Python ���� ���� ����: " + e.Message);
        }
    }

    async void ReceiveLoop()
    {
        while (client != null && client.Connected)
        {
            try
            {
                string json = await reader.ReadLineAsync();
                if (!string.IsNullOrEmpty(json))
                {
                    PoseData data = JsonUtility.FromJson<PoseData>(json);

                    // ���ŵ� ��ġ/ȸ��
                    Vector3 rawPosition = new Vector3(
                        data.position[0],
                        data.position[1],
                        data.position[2]
                    );

                    Quaternion rawRotation = new Quaternion(
                        data.rotation[0],
                        data.rotation[1],
                        data.rotation[2],
                        data.rotation[3]
                    );

                    // �������� ���� ��ġ/ȸ��
                    targetPosition = rawPosition + positionOffset;
                    targetRotation = rawRotation * rotationOffset;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("������ ���� ����: " + e.Message);
                break;
            }

            await Task.Delay(1);
        }
    }

    void Update()
    {
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            Time.deltaTime * positionSmoothSpeed
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * rotationSmoothSpeed
        );
    }

    void OnApplicationQuit()
    {
        if (reader != null) reader.Close();
        if (client != null) client.Close();
    }

    [Serializable]
    public class PoseData
    {
        public float[] position;
        public float[] rotation;
    }

    // �ܺο��� ������ ����
    public void SetPositionOffset(Vector3 offset)
    {
        positionOffset = offset;
    }

    public void SetRotationOffset(Quaternion offset)
    {
        rotationOffset = offset;
    }

    // �ܺο��� ������ ����
    public void SetScale(Vector3 scale)
    {
        transform.localScale = scale;
    }

    // �ʱ�ȭ ��ư
    public void ResetOffsets()
    {
        positionOffset = Vector3.zero;
        rotationOffset = Quaternion.identity;
    }
}
