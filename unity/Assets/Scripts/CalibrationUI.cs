using MixedReality.Toolkit;
using MixedReality.Toolkit.UX;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

public class CalibrationUI : MonoBehaviour
{
    public PoseReceiver poseReceiver;

    public GameObject calibrationPanel;   // CalibrationPanel ������Ʈ
    public GameObject calibrationButton;  // Calibrate ��ư ������Ʈ
    public GameObject worldGizmo;         // World Gizmo ������Ʈ
    public GameObject localGizmo;         // Local Gizmo ������Ʈ
    public GameObject markerObject;       // Marker ������Ʈ
    public Transform OrganModel;          // ��� �� Transform ����

    public MRTKTMPInputField posXField, posYField, posZField;
    public MRTKTMPInputField rotXField, rotYField, rotZField, rotWField;
    public MRTKTMPInputField scaleXField, scaleYField, scaleZField, scaleAllField;

    public StatefulInteractable worldGizmoToggle;
    public StatefulInteractable localGizmoToggle;
    public StatefulInteractable markerToggle;

    private Vector3 defaultScale;

    void Start()
    {
        // �⺻ ������ ����
        defaultScale = poseReceiver.transform.localScale;

        // ������ ���� (�ǽð� �ݿ���)
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

        // Toggle �̺�Ʈ ������ ���
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

        // ������ �� �г��� ��Ȱ��ȭ, ��ư�� Ȱ��ȭ
        if (calibrationPanel != null) calibrationPanel.SetActive(false);
        if (calibrationButton != null) calibrationButton.SetActive(true);

        ResetCalibration();
    }
    void Update()
    {
        if (worldGizmo.activeSelf)
        {
            // �� ��ġ + ���� x�� �������� 0.024, ���� z�� �������� 0.063 �̵� (�߽� ����)
            Vector3 offset = OrganModel.right * 0.024f + OrganModel.forward * 0.063f;
            worldGizmo.transform.position = OrganModel.position + offset;

            // ȸ���� �׻� ���� ���� (Quaternion.identity = ȸ�� ����)
            worldGizmo.transform.rotation = Quaternion.identity;
        }
    }

    // Calibrate ��ư���� ȣ��
    public void ShowPanel()
    {
        if (calibrationPanel != null) calibrationPanel.SetActive(true);
        if (calibrationButton != null) calibrationButton.SetActive(false);
        // Toggle ���¿� ���� ����� On/Off
        if (worldGizmo != null) worldGizmo.SetActive(worldGizmoToggle.IsToggled);
        if (localGizmo != null) localGizmo.SetActive(localGizmoToggle.IsToggled);
        if (markerObject != null) markerObject.SetActive(markerToggle.IsToggled);
    }

    // Done ��ư���� ȣ�� (�ǽð� �ݿ��̸� ��� �ǹ̴� ������ UI �ݱ� �뵵�� ����)
    public void ApplyCalibration()
    {
        Debug.Log("Calibration Applied (from Done button)");

        if (calibrationPanel != null) calibrationPanel.SetActive(false);
        if (calibrationButton != null) calibrationButton.SetActive(true);
        if (worldGizmo != null) worldGizmo.SetActive(worldGizmoToggle.IsToggled);
        if (localGizmo != null) localGizmo.SetActive(localGizmoToggle.IsToggled);
        if (markerObject != null) markerObject.SetActive(markerToggle.IsToggled);
    }

    // �ǽð� ��ġ �ݿ� �Լ�
    public void ApplyLivePosition()
    {
        float x = ParseFloat(posXField.text);
        float y = ParseFloat(posYField.text);
        float z = ParseFloat(posZField.text);
        poseReceiver.SetPositionOffset(new Vector3(x, y, z));

        Debug.Log("�ǽð� Position �ݿ���");
    }

    // �ǽð� ȸ�� �ݿ� �Լ� (�� ���� �Է� �� ���ʹϾ� ��ȯ)
    public void ApplyLiveRotation()
    {
        float x = ParseFloat(rotXField.text);
        float y = ParseFloat(rotYField.text);
        float z = ParseFloat(rotZField.text);

        // Quaternion.Euler�� �� ������ �޾� ���ʹϾ����� ��ȯ
        Quaternion rot = Quaternion.Euler(x, y, z);
        poseReceiver.SetRotationOffset(rot);

        Debug.Log($"�ǽð� Rotation �ݿ��� (Euler): ({x}, {y}, {z})");
    }

    // �ǽð� ������ �ݿ� �Լ�
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

        Debug.Log("�ǽð� Scale �ݿ���");
    }
    private void OnScaleAllChanged(string allValue)
    {
        float uniform = ParseFloat(allValue);

        scaleXField.SetTextWithoutNotify(uniform.ToString("F2"));
        scaleYField.SetTextWithoutNotify(uniform.ToString("F2"));
        scaleZField.SetTextWithoutNotify(uniform.ToString("F2"));

        ApplyLiveScale();
    }

    // Reset ��ư���� ȣ��
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
        // ���� ���� ���� �� ���� ����
        text = Regex.Replace(text, @"[\u200B-\u200F\uFEFF]", "").Trim();

        Debug.Log($"�Է°�(���� ��): [{text}], ����: {text.Length}");

        float result;
        if (!float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
        {
            Debug.LogWarning("�Է°� �Ľ� ����: '" + text + "'");
            result = 0f;
        }
        return result;
    }
}
