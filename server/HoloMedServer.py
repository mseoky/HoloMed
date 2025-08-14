import socket
import json
import time
from NatNetClient import NatNetClient

# -------------------------------
# Unity(HoloLens) TCP 서버 설정
HOST = "0.0.0.0"  # 모든 IP에서 접속 허용
PORT = 5005       # Unity(HoloLens)가 접속할 포트

# Unity(HoloLens) TCP 서버 소켓 열기
server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server_socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)  # 포트 재사용
server_socket.bind((HOST, PORT))
server_socket.listen(1)
print(f"[INFO] HoloLens 연결 대기 중... (포트 {PORT})")

# HoloLens 클라이언트 접속 대기
unity_socket, unity_addr = server_socket.accept()
print(f"[INFO] HoloLens 클라이언트 연결됨: {unity_addr}")

# -------------------------------
# 멀티캐스트 or 유니캐스트 선택
while True:
    mode = input("Select 0 for multicast and 1 for unicast: ").strip()
    if mode in ["0", "1"]:
        use_multicast = (mode == "0")
        break
    else:
        print("Invalid input. Enter 0 (multicast) or 1 (unicast)")

if use_multicast:
    print("멀티캐스트 모드 선택됨")
else:
    print("유니캐스트 모드 선택됨")

# -------------------------------
# 변수 초기화
last_pose = None  # 마지막으로 보낸 PoseData 저장
last_send_time = time.time()

# -------------------------------
# Rigid Body 콜백 함수
def receive_rigid_body_frame(new_id, position, rotation):
    global last_pose

    # 좌표계 변환 (OptiTrack Right-handed → Unity Left-handed)
    converted_position = [-position[0], position[1], position[2]]
    converted_rotation = [-rotation[0], rotation[1], rotation[2], -rotation[3]]

    # Unity(HoloLens)로 보낼 데이터
    last_pose = {
        "id": new_id,
        "position": converted_position,
        "rotation": converted_rotation
    }

# -------------------------------
# Unity(HoloLens)로 PoseData 전송 함수
def send_pose_to_unity(pose_data):
    try:
        json_data = json.dumps(pose_data) + "\n"  # Unity ReadLine용
        unity_socket.sendall(json_data.encode('utf-8'))
        print(f"[SEND] {json_data}")
    except (BrokenPipeError, ConnectionResetError):
        print("[ERROR] HoloLens 연결 끊김")
        exit(1)
    except Exception as e:
        print(f"[ERROR] HoloLens 전송 실패: {e}")

# -------------------------------
# Main
if __name__ == "__main__":
    motive_ip = "192.168.1.46"  # 연구실 PC IP
    client_ip = "192.168.1.41"  # 내 노트북 IP

    # NatNet 클라이언트 초기화
    natnet_client = NatNetClient()
    natnet_client.set_client_address(client_ip)
    natnet_client.set_server_address(motive_ip)
    natnet_client.set_use_multicast(use_multicast)  # 선택한 모드로 설정
    natnet_client.rigid_body_listener = receive_rigid_body_frame

    # NatNet 시작
    print("[INFO] Motive 연결 중...")
    success = natnet_client.run("d")  # datastream

    if not success:
        print("[ERROR] Motive 연결 실패")
        unity_socket.close()
        server_socket.close()
        exit(1)

    try:
        while True:
            if last_pose is not None:
                send_pose_to_unity(last_pose)

            # 전송 속도: 120fps
            time.sleep(1 / 120.0)
    except KeyboardInterrupt:
        print("\n[INFO] 종료 신호 감지, 소켓 닫는 중...")
        natnet_client.shutdown()
        unity_socket.close()
        server_socket.close()
        print("[INFO] Python 서버 종료 완료")
