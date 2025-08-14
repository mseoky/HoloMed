using MixedReality.Toolkit;
using MixedReality.Toolkit.UX;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

public class CalibrationUI : MonoBehaviour
{
    public PoseReceiver poseReceiver;

    public GameObject calibrationPanel;   // CalibrationPanel 오브젝트
    public GameObject calibrationButton;  // Calibrate 버튼 오브젝트
    public GameObject worldGizmo;         // World Gizmo 오브젝트
    public GameObject localGizmo;         // Local Gizmo 오브젝트
    public GameObject markerObject;       // Marker 오브젝트
    public Transform OrganModel;          // 장기 모델 Transform 참조

    public MRTKTMPInputField posXField, posYField, posZField;
    public MRTKTMPInputField rotXField, rotYField, rotZField, rotWField;
    public MRTKTMPInputField scaleXField, scaleYField, scaleZField, scaleAllField;

    public StatefulInteractable worldGizmoToggle;
    public StatefulInteractable localGizmoToggle;
    public StatefulInteractable markerToggle;

    private Vector3 defaultScale;

    void Start()
    {
        // 기본 스케일 저장
        defaultScale = poseReceiver.transform.localScale;

        // 리스너 연결 (실시간 반영용)
        posXField.onValueChanged.AddListener(_ => ApplyLivePosition());
        posYField.onValueChanged.AddListener(_ => ApplyLivePosition());
        posZField.onValueChanged.AddListener(_ => ApplyLivePosition());

        rotXField.onValueChanged.AddListener(_ => ApplyLiveRotation());
        rotYField.onValueChanged.AddListener(_ => ApplyLiveRotation());
        rotZField.onValueChanged.AddListener(_ => ApplyLiveRotation());
        rotWField.onValueChanged.AddListener(_ => ApplyLiveRotation());

        scaleXField.onValueChanged.AddListener(_ => ApplyLiveScale());
        scaleYField.onValueChanged.AddListener(_ => ApplyLiveScale());
        scaleZField.onValueChanged.AddListener(_ => ApplyLiveScale());
        scaleAllField.onValueChanged.AddListener(OnScaleAllChanged);

        // Toggle 이벤트 리스너 등록
        worldGizmoToggle.OnClicked.AddListener(() =>
        {
            if (worldGizmo != null) worldGizmo.SetActive(worldGizmoToggle.IsToggled);
        });

        localGizmoToggle.OnClicked.AddListener(() =>
        {
            if (localGizmo != null) localGizmo.SetActive(localGizmoToggle.IsToggled);
        });

        markerToggle.OnClicked.AddListener(() =>
        {
            if (markerObject != null) markerObject.SetActive(markerToggle.IsToggled);
        });

        // 시작할 때 패널은 비활성화, 버튼은 활성화
        if (calibrationPanel != null) calibrationPanel.SetActive(false);
        if (calibrationButton != null) calibrationButton.SetActive(true);

        ResetCalibration();
    }
    void Update()
    {
        if (worldGizmo.activeSelf)
        {
            // 폐 위치 + 로컬 x축 방향으로 0.024, 로컬 z축 방향으로 0.063 이동 (중심 보정)
            Vector3 offset = OrganModel.right * 0.024f + OrganModel.forward * 0.063f;
            worldGizmo.transform.position = OrganModel.position + offset;

            // 회전은 항상 월드 기준 (Quaternion.identity = 회전 없음)
            worldGizmo.transform.rotation = Quaternion.identity;
        }
    }

    // Calibrate 버튼에서 호출
    public void ShowPanel()
    {
        if (calibrationPanel != null) calibrationPanel.SetActive(true);
        if (calibrationButton != null) calibrationButton.SetActive(false);
        // Toggle 상태에 따라 기즈모 On/Off
        if (worldGizmo != null) worldGizmo.SetActive(worldGizmoToggle.IsToggled);
        if (localGizmo != null) localGizmo.SetActive(localGizmoToggle.IsToggled);
        if (markerObject != null) markerObject.SetActive(markerToggle.IsToggled);
    }

    // Done 버튼에서 호출 (실시간 반영이면 사실 의미는 줄지만 UI 닫기 용도로 유지)
    public void ApplyCalibration()
    {
        Debug.Log("Calibration Applied (from Done button)");

        if (calibrationPanel != null) calibrationPanel.SetActive(false);
        if (calibrationButton != null) calibrationButton.SetActive(true);
        if (worldGizmo != null) worldGizmo.SetActive(worldGizmoToggle.IsToggled);
        if (localGizmo != null) localGizmo.SetActive(localGizmoToggle.IsToggled);
        if (markerObject != null) markerObject.SetActive(markerToggle.IsToggled);
    }

    // 실시간 위치 반영 함수
    public void ApplyLivePosition()
    {
        float x = ParseFloat(posXField.text);
        float y = ParseFloat(posYField.text);
        float z = ParseFloat(posZField.text);
        poseReceiver.SetPositionOffset(new Vector3(x, y, z));

        Debug.Log("실시간 Position 반영됨");
    }

    // 실시간 회전 반영 함수 (도 단위 입력 → 쿼터니언 변환)
    public void ApplyLiveRotation()
    {
        float x = ParseFloat(rotXField.text);
        float y = ParseFloat(rotYField.text);
        float z = ParseFloat(rotZField.text);

        // Quaternion.Euler는 도 단위를 받아 쿼터니언으로 변환
        Quaternion rot = Quaternion.Euler(x, y, z);
        poseReceiver.SetRotationOffset(rot);

        Debug.Log($"실시간 Rotation 반영됨 (Euler): ({x}, {y}, {z})");
    }

    // 실시간 스케일 반영 함수
    public void ApplyLiveScale()
    {
        float x = ParseFloat(scaleXField.text);
        float y = ParseFloat(scaleYField.text);
        float z = ParseFloat(scaleZField.text);
        poseReceiver.SetScale(new Vector3(
            defaultScale.x * x,
            defaultScale.y * y,
            defaultScale.z * z
        ));

        Debug.Log("실시간 Scale 반영됨");
    }
    private void OnScaleAllChanged(string allValue)
    {
        float uniform = ParseFloat(allValue);

        scaleXField.SetTextWithoutNotify(uniform.ToString("F2"));
        scaleYField.SetTextWithoutNotify(uniform.ToString("F2"));
        scaleZField.SetTextWithoutNotify(uniform.ToString("F2"));

        ApplyLiveScale();
    }

    // Reset 버튼에서 호출
    public void ResetCalibration()
    {
        posXField.SetTextWithoutNotify("0.00");
        posYField.SetTextWithoutNotify("0.00");
        posZField.SetTextWithoutNotify("0.00");

        rotXField.SetTextWithoutNotify("0.00");
        rotYField.SetTextWithoutNotify("0.00");
        rotZField.SetTextWithoutNotify("0.00");
        // rotWField.SetTextWithoutNotify("1.00");

        scaleXField.SetTextWithoutNotify("1.00");
        scaleYField.SetTextWithoutNotify("1.00");
        scaleZField.SetTextWithoutNotify("1.00");
        scaleAllField.SetTextWithoutNotify("");

        ApplyLivePosition();
        ApplyLiveRotation();
        ApplyLiveScale();

        Debug.Log("Calibration Reset (UI only)");
    }

    private float ParseFloat(string text)
    {
        // 제어 문자 제거 및 공백 제거
        text = Regex.Replace(text, @"[\u200B-\u200F\uFEFF]", "").Trim();

        Debug.Log($"입력값(정제 후): [{text}], 길이: {text.Length}");

        float result;
        if (!float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
        {
            Debug.LogWarning("입력값 파싱 실패: '" + text + "'");
            result = 0f;
        }
        return result;
    }
}
