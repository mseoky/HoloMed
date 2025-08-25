# OptiTrack Motive Calibration & Data Usage Guide

이 문서는 OptiTrack 시스템을 활용해 마커를 추적하고, Unity/HoloLens와 연동하기 위해 필요한 **Motive Calibration 절차와 데이터 활용 방법**을 정리한 것입니다.

참고 영상: [OptiTrack - Motive 3.0 | Quick Start Guide](https://www.youtube.com/watch?v=HyrHhaRVOaM&t=140s)

---

## 1. OptiTrack 시스템 소개

- **구성 요소**
  - IR 카메라
  - 반사 마커
  - PoE 허브 및 PC
  - Motive 소프트웨어

- **동작 원리**
  - IR 카메라로 마커의 위치를 캡처
  - 이를 통해 3D 위치 및 자세(orientation) 계산

- **활용 사례**
  - AR/VR
  - 의료
  - 애니메이션 등

![system_overview](docs/images/motive_system.png)

---

## 2. 카메라 Calibration 절차

### (1) 초기 설정
- PoE 허브 전원을 켜고 PC에서 **Motive 실행**
- **Device 탭**에서 IR 카메라들이 Tracking되는지 확인  
  (모든 카메라가 Object 모드여야 함)
- **Calibration 탭 → New Calibration** 클릭

![calibration1](docs/images/motive_calibration1.png)

---

### (2) 불필요한 반사체 제거
- **Cameras View**로 전환
- **Mask** 기능을 사용하여 불필요한 노이즈(반사체)를 제거

![calibration2](docs/images/motive_calibration2.png)

---

### (3) Wanding
- **Start Wanding** 클릭 후, Wanding 장치를 이용해 3D 공간 내를 움직임
- Wanding은 카메라들이 공간 좌표계를 정확히 인식할 수 있도록 하는 동적 캘리브레이션 과정
- 완료 후 **Start Calculation** 클릭 → 카메라 간 상대적 위치와 방향 계산

![calibration3](docs/images/motive_calibration3.png)

---

### (4) Ground Plane 설정
- Calibration Square를 최소 2대 이상의 카메라가 보이도록 위치
- **Set Ground Plane** 클릭
- Long arm → +z, Short arm → +x, +y up
- Calibration 결과는 `.cal` 형식으로 지정 경로에 자동 저장됨

![calibration4](docs/images/motive_calibration4.png)  

---

## 3. Rigid Body 등록

### (1) 마커 부착
- Rigid Body로 사용할 대상에 3개 이상의 마커 부착
- (예시: 테이프 바깥쪽 면에 8개 마커 부착)

![rigidbody1](docs/images/rigidbody1.png)

---

### (2) Rigid Body 생성
- Perspective View로 전환 후, 인식된 마커들을 마우스로 드래그
- 마우스 우클릭 → **Create Rigid Body** 클릭

![rigidbody2](docs/images/rigidbody2.png)  

---

### (3) 등록 확인
- **Assets 탭**에서 Rigid Body 등록 여부 확인

![rigidbody3](docs/images/rigidbody3.png)

---

## 4. Rigid Body 추적

- 하단의 **빨간색 버튼(Record/Live Capture)** 클릭
- 객체 움직임을 추적

![rigidbody_tracking](docs/images/rigidbody_tracking.gif)

---

## 5. Rigid Body 데이터 활용 방법

Rigid Body 추적 데이터는 **두 가지 방식**으로 외부에서 활용할 수 있습니다.

---

### (A) CSV 파일 Export & 결과 분석 (오프라인)

- **Data 탭**에서 추적 데이터 마우스 우클릭 → **Solve All Assets** 클릭  
  (이 과정을 거치지 않으면 Export 시 `Unsolved Assets` 오류 발생)
- 이후 **Export Tracking Data** 선택
- Export 형식: `.csv` 권장
- 내보낼 데이터 선택 (Rigid Body 중심 좌표, Rotation, Marker 좌표 등)

![export](docs/images/motive_export.png)  

CSV 파일에는 다음과 같은 정보가 포함됩니다:

![csv_capture](docs/images/motive_csv.png)

#### CSV 열 구조 설명
| 열 번호 | 열 이름(예시)                | 설명 |
|---------|------------------------------|------|
| 1       | **Frame**                    | 프레임 번호 (0, 1, 2 …) |
| 2       | **Time (s)**                 | 시간 (초 단위, 100 Hz 주기 → 0.01초 간격) |
| 3–5     | **Position X, Y, Z**         | Rigid Body 중심 좌표 (3D 위치) |
| 6–9     | **Rotation Qx, Qy, Qz, Qw**  | Rigid Body 회전(Quaternion) |
| 10–12   | **Marker1 X, Y, Z**          | 첫 번째 마커 좌표 |
| 13–15   | **Marker2 X, Y, Z**          | 두 번째 마커 좌표 |
| 16–18   | **Marker3 X, Y, Z**          | 세 번째 마커 좌표 |
| …       | …                            | (마커 개수에 따라 반복) |
| N       | **Marker8 X, Y, Z**          | 여덟 번째 마커 좌표 |

📌 요약  
- **Rotation (4열)**: 객체 방향 정보 (Quaternion X, Y, Z, W)  
- **Position (3열)**: Rigid Body 중심 좌표 (X, Y, Z)  
- **Marker Position (3 × 8열)**: 부착된 8개 마커 좌표  
- **Time**: 전체 데이터의 기준 시간축 (정밀 동기화용)

즉, CSV 데이터는 객체 중심 좌표와 마커별 좌표까지 모두 포함하고,  
100 Hz로 시간별 세밀한 움직임을 기록합니다.

![rigidbody_analysis](docs/images/rigidbody_analysis.png)

---

### (B) 실시간 스트리밍 (Unity/HoloLens 연동)

Rigid Body 등록 이후, **NatNet Streaming** 기능을 활성화하여 외부 프로그램(Unity, Python Server 등)으로 데이터를 전송할 수 있습니다.

#### 기본 설정
1. Motive 실행 → 상단 메뉴 **View → Settings → Streaming** 탭으로 이동
2. **Enable**: 켜기 (스트리밍 활성화)
3. **Local Interface**: 서버 PC 네트워크 어댑터 IP 자동 지정 (예: 192.168.1.46)
4. **Transmission Type**
   - **Unicast**: 특정 IP로 송출 (Unity/Python 클라이언트에서 해당 IP로 접속)
   - **Multicast**: 239.255.42.99 그룹으로 송출, 다수 클라이언트 수신 가능
5. **Rigid Bodies**: 체크
6. **Command Port**: 1510
7. **Data Port**: 1511
8. **Up Axis**: Y Axis

![motive_streaming](docs/images/motive_streaming.png)

#### Windows 방화벽 설정
- **제어판 → Windows Defender 방화벽 → 고급 설정 → 인바운드 규칙 추가**
  - 포트: TCP & UDP, 번호 1510, 1511 허용

#### Unity / Python 연동 시 주의점
- 클라이언트는 반드시 동일 네트워크 대역에 있어야 함
- Unicast는 Motive Local Interface IP로 접속
- Multicast는 기본 그룹(239.255.42.99) 사용
- 방화벽이 열려 있지 않으면 연결 불가

#### 참고 문서
- [OptiTrack Motive Streaming Settings](https://docs.optitrack.com/motive-ui-panes/settings/settings-streaming)

---

## ✅ 전체 요약
1. **Motive 실행** → 카메라 Tracking 확인  
2. **Calibration** → Noise 제거 → Wanding → Ground Plane 설정  
3. **Rigid Body 등록** → 마커 드래그 & 등록 확인  
4. **Tracking 실행**  
5. **데이터 활용**  
   - (A) CSV Export & 결과 분석 → 후처리/연구용  
   - (B) 실시간 Streaming → Unity/HoloLens 연동
