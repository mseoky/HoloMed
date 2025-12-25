# Unity (MRTK3) + HoloLens 2 Setup & Deployment Guide

이 문서는 **HoloMed 프로젝트의 Unity(MRTK3) 환경 설정부터 HoloLens 2 배포까지의 전체 과정**을 정리한 가이드입니다.  
MRTK와 HoloLens 개발이 처음인 경우에도, 이 문서만 따라가면 실행 및 배포가 가능하도록 구성했습니다.

---

## 1. 개발 환경 개요

### 필수 환경
- **OS**: Windows 10/11
- **Unity Hub**
- **Unity Editor**: 2022 LTS 권장
- **Visual Studio 2022**
- **HoloLens 2 (Developer Mode 활성화)**

### 사용 기술
- Unity
- MRTK3 (Mixed Reality Toolkit)
- OpenXR
- UWP (Universal Windows Platform)

---

## 2. Unity 프로젝트 열기

1. Unity Hub 실행
2. `Add` → `/unity` 폴더 선택
3. Unity Editor 실행

> ⚠️ Unity 버전이 다를 경우 경고가 발생할 수 있으나, 2022 LTS 기준으로 정상 동작함

---

## 3. MRTK3 설정

### 3.1 MRTK3 패키지 설치

- **Mixed Reality Feature Tool** 사용 권장

설치 항목:
- MRTK3 Core
- MRTK3 UX Components
- MRTK3 Input
- MRTK3 Spatial Manipulation

📷 *[여기에 MRTK Feature Tool 캡처 추가]*

---

### 3.2 OpenXR 설정

1. `Edit → Project Settings → XR Plug-in Management`
2. **OpenXR** 활성화
3. OpenXR Feature Groups:
   - Microsoft Hand Interaction
   - Eye Gaze Interaction
   - Motion Controller (필요 시)

📷 *[OpenXR 설정 캡처]*

---

## 4. Project Settings 필수 체크 항목

### 4.1 Player Settings

`Edit → Project Settings → Player`

- **Scripting Backend**: IL2CPP
- **API Compatibility Level**: .NET Standard 2.1
- **Target Architecture**: ARM64
- **Color Space**: Linear
- **Graphics API**: Direct3D11

📷 *[Player Settings 캡처]*

---

### 4.2 UWP Capabilities

- InternetClient
- InternetClientServer
- PrivateNetworkClientServer
- SpatialPerception

> NatNet 스트리밍 및 네트워크 통신에 필수

📷 *[UWP Capabilities 캡처]*

---

## 5. 씬 구성 (HoloMed 기준)

기본 씬 구성 예시:
- MRTK XR Rig
- Input System
- Calibration UI
- 3D Organ Model
- Gizmo / Marker Visualization

📷 *[Hierarchy 구조 캡처]*

---

## 6. 빌드 설정 (HoloLens 2)

### 6.1 Build Settings

1. `File → Build Settings`
2. Platform: **Universal Windows Platform**
3. Target Device: HoloLens
4. Architecture: ARM64
5. Build Type: D3D Project

📷 *[Build Settings 캡처]*

---

### 6.2 빌드 실행

- `Build` 클릭
- 출력 폴더 지정 (예: `/Builds/HoloMed/`)

---

## 7. Visual Studio 배포

1. 생성된 `.sln` 파일 열기
2. Configuration:
   - **Release**
   - **ARM64**
3. Target:
   - Device (USB 연결)
   - 또는 Remote Machine (Wi-Fi)

📷 *[Visual Studio 배포 화면 캡처]*

4. `Deploy` 실행

---

## 8. HoloLens 실행 후 보정(Calibration)

1. 앱 실행
2. Calibration UI 활성화
3. 모델의 **위치 / 회전 / 스케일** 조정
4. 마커 및 Gizmo 표시로 정렬 상태 확인

📷 *[HoloLens 캘리브레이션 화면 GIF or 캡처]*

---

## 9. 문제 해결 (Troubleshooting)

### 앱이 실행되지 않을 때
- UWP Capability 누락 여부 확인
- ARM64 빌드 여부 확인

### 스트리밍 연결 안 될 때
- 동일 네트워크 여부 확인
- 방화벽 포트(1510, 1511) 확인
- Motive Streaming 활성화 여부 확인

---

## 10. 참고 자료

- MRTK3 공식 문서  
  https://learn.microsoft.com/ko-kr/windows/mixed-reality/mrtk-unity/mrtk3-overview
- HoloLens Unity 개발 가이드  
  https://learn.microsoft.com/ko-kr/windows/mixed-reality/develop/unity/unity-development-overview
- OptiTrack Calibration & Data Guide  
  👉 `docs/motive_calibration.md`

---

## ✅ 요약

- Unity + MRTK3 + OpenXR 세팅은 **초기 진입 장벽이 높음**
- 본 문서는 **실제 HoloLens 배포까지 검증된 설정**을 기준으로 작성됨
- OptiTrack 연동 전, **Unity 환경 안정화가 가장 중요**
