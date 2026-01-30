//------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using System.Text;
using Tayx.Graphy.Utils.NumString;
//------------------------------------------------------------------------------

public class UIController : MonoBehaviour {
    // Settings panel
    public GameObject settingsPanel;
    public InputField targetFrameRateEditField;
    public Toggle vSyncToggle;
    public Text vSyncToggleVSyncCaptionText;
    public InputField resolutionXEditField;
    public InputField resolutionYEditField;
    // Current settings panel
    public GameObject currentSettingsPanel;
    public Text targetFrameRateText;
    public Text vSyncText;
    //------------------------------------------------------------------------------

    private StringBuilder stringBuilder = null;
    //------------------------------------------------------------------------------

    private void Start()
    {
        stringBuilder = new StringBuilder();

        targetFrameRateEditField.text = Application.targetFrameRate.ToString();
        resolutionXEditField.text = "1920";  //Screen.currentResolution.width.ToString();
        resolutionYEditField.text = "1200";  //Screen.currentResolution.height.ToString();

#if UNITY_EDITOR
        vSyncToggleVSyncCaptionText.text = "Cannot set frame pacing in Editor";
        vSyncToggle.enabled = false;
        vSyncToggle.isOn = false;
#endif // #if UNITY_EDITOR
        OnPacingChanged(false);
    }
    //------------------------------------------------------------------------------

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G)) {
            OnGKeyPressed();
        }

        stringBuilder.Length = 0;
        stringBuilder.Append("QualitySettings.vSyncCount: ").Append(QualitySettings.vSyncCount.ToStringNonAlloc());
        vSyncText.text = stringBuilder.ToString();

        stringBuilder.Length = 0;
        stringBuilder.Append("Application.targetFrameRate: ").Append(Application.targetFrameRate.ToStringNonAlloc());
        targetFrameRateText.text = stringBuilder.ToString();
    }
    //------------------------------------------------------------------------------

    private void OnGKeyPressed()
    {
        settingsPanel.SetActive(false == settingsPanel.activeSelf);
        currentSettingsPanel.SetActive(false == currentSettingsPanel.activeSelf);
    }
    //------------------------------------------------------------------------------

    public void OnPacingChanged(bool useSystemFramePacing)
    {
        vSyncToggleVSyncCaptionText.gameObject.SetActive(false == useSystemFramePacing);
        targetFrameRateEditField.gameObject.SetActive(useSystemFramePacing);
    }
    //------------------------------------------------------------------------------
}
//------------------------------------------------------------------------------
