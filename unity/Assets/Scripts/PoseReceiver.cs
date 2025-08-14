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

    // 사용자 보정용 오프셋 변수
    public Vector3 positionOffset = Vector3.zero;
    public Quaternion rotationOffset = Quaternion.identity;

    async void Start()
    {
        try
        {
            string serverIP = "192.168.1.41";
            int serverPort = 5005;

            client = new TcpClient();
            Debug.Log("Python 서버 연결 시도 중...");
            await client.ConnectAsync(serverIP, serverPort);

            NetworkStream stream = client.GetStream();
            reader = new StreamReader(stream, Encoding.UTF8);

            Debug.Log("Python 서버 연결 성공");

            _ = Task.Run(() => ReceiveLoop());
        }
        catch (Exception e)
        {
            Debug.LogError("Python 서버 연결 실패: " + e.Message);
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

                    // 수신된 위치/회전
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

                    // 오프셋을 더한 위치/회전
                    targetPosition = rawPosition + positionOffset;
                    targetRotation = rawRotation * rotationOffset;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("데이터 수신 오류: " + e.Message);
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

    // 외부에서 오프셋 설정
    public void SetPositionOffset(Vector3 offset)
    {
        positionOffset = offset;
    }

    public void SetRotationOffset(Quaternion offset)
    {
        rotationOffset = offset;
    }

    // 외부에서 스케일 설정
    public void SetScale(Vector3 scale)
    {
        transform.localScale = scale;
    }

    // 초기화 버튼
    public void ResetOffsets()
    {
        positionOffset = Vector3.zero;
        rotationOffset = Quaternion.identity;
    }
}
