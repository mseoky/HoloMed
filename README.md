# HoloMed: HoloLens 2와 OptiTrack을 활용한 의료 AR 애플리케이션

## 개요

**HoloMed**는 **Microsoft HoloLens 2**와 **OptiTrack** 모션 캡처 시스템을 결합한 의료 증강 현실(AR) 애플리케이션입니다. 이 프로젝트는 3D 장기 모델을 마네킹이나 실제 환자 위에 실시간으로 정밀하게 오버레이하여 의료 시뮬레이션 및 교육을 지원합니다.

-----

## 주요 기능

  - **실시간 데이터 스트리밍**: OptiTrack의 Rigid Body 추적 데이터를 HoloLens 2로 실시간 전송.
  - **인앱 보정(In-app Calibration)**: 증강 현실 환경에서 3D 모델의 위치, 회전, 크기를 수동으로 미세 조정하는 직관적인 UI 제공.
  - **시각화 제어**: 정확한 정렬 확인을 위해 마커와 기즈모(gizmo)를 켜고 끌 수 있는 기능.
  - **정밀한 공간 정렬**: 수술 보조 및 정확도 평가를 목표로 개발됨.

-----

## 시스템 아키텍처

HoloMed 시스템은 하드웨어와 소프트웨어의 긴밀한 통합을 통해 작동합니다. 시스템은 크게 세 부분으로 구성됩니다.

1.  **OptiTrack Motive**: 적외선 카메라를 이용해 물리적 마커의 3차원 위치와 회전 정보를 추적합니다.
2.  **Python TCP Server**: Motive에서 NatNet SDK를 통해 받은 데이터를 가공하여 홀로렌즈 2 앱으로 TCP 통신을 이용해 실시간으로 스트리밍합니다.
3.  **Unity HoloLens App**: 수신된 데이터를 기반으로 3D 장기 모델을 렌더링하고, 실제 마네킹 위에 정확히 정렬하여 증강 현실 경험을 제공합니다.

-----

## 프로젝트 구조

```
HoloMed/
├─ README.md             # 프로젝트 개요 및 빠른 시작 가이드
├─ /server/              # OptiTrack -> 홀로렌즈용 파이썬 TCP 서버
│ ├─ README.md
│ └─ HoloMedServer.py
├─ /unity/               # 홀로렌즈 2용 유니티 MRTK3 프로젝트
│ ├─ README.md
│ └─ (Assets, Scripts, MRTK settings 등)
├─ /docs/                # 문서, 스크린샷, gif
│ ├─ motive_calibration.md
│ ├─ demo.gif
│ └─ system_architecture.png
```

-----

## 요구 사항

  - **운영체제**: Windows 11
  - **하드웨어**: HoloLens 2 (개발자 모드 활성화)
  - **소프트웨어**:
      - **Unity**: 2022.3.62f1 LTS 버전 (MRTK3 및 OpenXR 플러그인 필요)
      - **Python**: 3.13.5
      - **OptiTrack Motive**: 3.1.0
      - **NatNet SDK**: 4.3.0

-----

## 설치 및 실행 방법

### 1\. OptiTrack 설정

1. **Motive**에서 카메라를 보정하고 추적 대상을 설정합니다.
2. 마네킹(또는 피험자)에 반사 마커를 부착하고, 이를 **Rigid Body**로 정의하여 추적이 잘 되는지 확인합니다.
3. **NatNet Streaming**을 활성화하고, Transmission Type을 **Unicast** 또는 **Multicast** 중 프로젝트 환경에 맞게 선택합니다.

### 2\. 파이썬 TCP 서버 실행 (Windows CMD)

NatNet SDK 설치 후, Python 샘플 클라이언트 폴더(NatNetSDK\Samples\PythonClient)에서 서버 스크립트를 실행합니다.

```cmd
cd C:\Users\<YourUsername>\NatNetSDK\Samples\PythonClient
python HoloMedServer.py
```
Tip: \<YourUsername\> 부분을 실제 Windows 사용자 계정명으로 바꿔주세요.

### 3\. 유니티 앱 빌드 및 배포

1.  Unity에서 `/unity` 폴더를 엽니다.
2.  MRTK3 및 OpenXR 설정을 완료한 후, 홀로렌즈 2용으로 앱을 빌드하고 배포합니다.

### 4\. AR 환경에서 보정

1.  홀로렌즈 앱을 실행하고, **Calibration UI**를 이용해 3D 모델의 위치, 회전, 스케일을 조정합니다.
2.  마커와 기즈모를 활용하여 오버레이가 정확히 정렬되었는지 확인합니다.

-----

## 정확도 측정

실제 마네킹과 오버레이된 3D 장기 모델을 비교하여 시스템의 정확도를 평가했습니다.

  - **평균 위치 오차**: 약 X mm (측정값으로 대체)

-----

## 현재 한계점

  - **트래킹 드리프트**: 사용자의 시야각이 크게 바뀌거나 움직임이 많을 경우 트래킹 오류가 발생할 수 있습니다.
  - **장치 발열**: 장시간 사용 시 홀로렌즈 장치의 발열로 인해 앱이 종료될 수 있습니다.
  - **수동 보정**: 매 세션마다 수동으로 좌표를 보정해야 합니다.

-----

## 향후 계획

  - OptiTrack과 홀로렌즈 좌표계를 자동으로 정렬하는 시스템 구현.
  - 여러 장기, 뼈, 근육을 함께 오브젝트로 등록하여 원하는 것들만 체크하여 실시간으로 사용자가 띄우거나 지울 수 있도록 하는 기능 개발.
  - 수술 도구에도 마커를 부착하여 증강된 3D 장기 모델과의 상대 거리를 측정하여 얼마나 접근하였는지 실시간으로 확인하는 기능 개발.
  - 수동 보정 UI의 편의성을 개선하고, 손이나 음성 입력으로도 보정할 수 있는 인터페이스 개발.
  - 장시간 사용에도 안정적인 시스템 구축.

-----

## 라이선스

이 프로젝트는 **MIT License**를 따릅니다.
