#define USE_FIXED_UPDATE
//------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.Scripting;
using Cinemachine;
using System;
//------------------------------------------------------------------------------

[RequireComponent(typeof(SpriteRenderer))]

public class MovementController : MonoBehaviour
{

    public enum MovementType
    {
        Layer,
        FollowTarget,
        CinemachineVCamera,
        UnityCamera,
    }

    public enum UpdateType
    {
        Update,
        FixedUpdate,
        LateUpdate,
        CVCUpdate
    }
    //------------------------------------------------------------------------------

    public UpdateType updateType = UpdateType.LateUpdate;
    public MovementType movementType = MovementType.FollowTarget;

    public Transform layer = null;
    public CinemachineVirtualCamera cinemachineVCamera = null;
    public Camera unityCamera = null;

    public Vector2 resolution = new Vector2(1920, 1200);

    public int movementSpeed = 4;

    public int targetFrameRate = 60;
    public bool useSystemFramePacing = false;
    //------------------------------------------------------------------------------

    private Transform movingTarget = null;
    private SpriteRenderer spriteRenderer = null;
    private CinemachineBrain cinemachineBrain = null;

    private bool isInitialized = false;
    private Vector3 initialPosition;
    private Vector3 initialLayerPosition;
    private Vector3 initialCVCamPosition;
    private Vector3 initialUnityCamPosition;
    //------------------------------------------------------------------------------

#if UNITY_EDITOR

    private void OnValidate()
    {
        if (UnityEngine.Application.isEditor)
        {
            Init();
            ApplySettings();
        }
    }
    //------------------------------------------------------------------------------
#endif // #if UNITY_EDITOR

    private void Init()
    {
        if (isInitialized)
            return;

        isInitialized = true;

#if ! UNITY_EDITOR
        GC.Collect();
        GarbageCollector.GCMode = GarbageCollector.Mode.Manual;
#endif // #if ! UNITY_EDITOR

        cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        initialPosition = transform.position;
        initialLayerPosition = layer.position;
        initialCVCamPosition = cinemachineVCamera.transform.position;
        initialUnityCamPosition = unityCamera.transform.position;

        Resolution currentResolution = Screen.currentResolution;
        targetFrameRate = (int)Math.Round((double)currentResolution.refreshRateRatio.value);
    }
    //------------------------------------------------------------------------------

    private void ApplySettings()
    {
        SetUpdateType(updateType);
        SetMovementType(movementType);
        SetScreenResolution((int)resolution.x, (int)resolution.y, Screen.fullScreen, Screen.currentResolution.refreshRateRatio);
        SetTargetFrameRate(targetFrameRate);
    }
    //------------------------------------------------------------------------------

    private void Start()
    {
        Init();
        ApplySettings();
    }
    //------------------------------------------------------------------------------

    private void OnEnable()
    {
        CinemachineCore.CameraUpdatedEvent.RemoveListener(OnCameraUpdated);
        CinemachineCore.CameraUpdatedEvent.AddListener(OnCameraUpdated);
    }
    //------------------------------------------------------------------------------

    private void OnDisable()
    {
        CinemachineCore.CameraUpdatedEvent.RemoveListener(OnCameraUpdated);
    }
    //------------------------------------------------------------------------------

    private void SetUpdateType(UpdateType type)
    {
        updateType = type;
    }
    //------------------------------------------------------------------------------

    private void SetMovementType(MovementType type)
    {
        movementType = type;

        switch (movementType)
        {
            case MovementType.Layer:
                movingTarget = layer;
                break;

            case MovementType.FollowTarget:
                movingTarget = transform;
                break;

            case MovementType.CinemachineVCamera:
                movingTarget = cinemachineVCamera.transform;
                break;

            case MovementType.UnityCamera:
                movingTarget = unityCamera.transform;
                break;

            default:
                movingTarget = null;
                break;
        }
        cinemachineVCamera.Follow = MovementType.FollowTarget == movementType ? movingTarget : null;
        cinemachineBrain.enabled = MovementType.UnityCamera != movementType;
        spriteRenderer.enabled = (MovementType.FollowTarget == movementType);
    }
    //------------------------------------------------------------------------------

    private void SetScreenResolution(int width, int height, bool fullScreen, RefreshRate refreshRate)
    {
        // A width by height resolution is used. If no matching resolution is supported, the closest one is used.
        // If preferredRefreshRate is 0 (default) Unity switches to the highest refresh rate that the monitor supports.
        // If preferredRefreshRate is not 0 Unity uses it if the monitor supports it, otherwise it chooses the highest supported one.
        // Changing refresh rate is only supported when using exclusive full-screen mode.
        //
        Screen.SetResolution(width,
                             height,
                             fullScreen ? Screen.fullScreenMode : FullScreenMode.Windowed,
                             refreshRate
                            );
    }
    //------------------------------------------------------------------------------

    /*
        Application.targetFrameRate
            specifies the target frame rate at which Unity tries to render your game.
            An integer > 0, or special value -1 (default).

        Both Application.targetFrameRate and QualitySettings.vSyncCount let you control your game's frame rate for smoother performance.
        targetFrameRate controls the frame rate by specifying the number of frames your game tries to render per second,
        whereas vSyncCount specifies the number of screen refreshes to allow between frames.

        Desktop and Web:
            If QualitySettings.vSyncCount is set to 0, then Application.targetFrameRate chooses a target frame rate for the game. If vSyncCount != 0, then targetFrameRate is ignored.

        Android and iOS:
            Mobile platforms always ignore QualitySettings.vSyncCount and instead use Application.targetFrameRate to choose a target frame rate for the game. Use targetFrameRate to
            control the frame rate of your game. This is useful for capping your game's frame rate to make sure your game displays smoothly and consistently under heavy rendering workloads.
            You can also reduce your game's frame rate to conserve battery life and avoid overheating on mobile devices.

        VR platforms:
            Ignore both QualitySettings.vSyncCount and Application.targetFrameRate and instead, the VR SDK controls the frame rate.

        Unity Editor:
            Application.targetFrameRate affects only the Game view. It has no effect on other Editor windows.

        When QualitySettings.vSyncCount = 0 and Application.targetFrameRate = -1:
            - Desktop: Content is rendered unsynchronized as fast as possible.
            - Web: Content is rendered at the native display refresh rate.
            - Android and iOS: Content is rendered at fixed 30 fps to conserve battery power, independent of the native refresh rate of the display.

        Desktop and Web: It is recommended to use QualitySettings.vSyncCount over Application.targetFrameRate because vSyncCount implements a hardware-based synchronization mechanism,
        whereas targetFrameRate, which is a software-based timing method is subject to microstuttering. In other words, on Desktop and Web platforms, setting vSyncCount = 0 and using targetFrameRate
        will not produce a completely stutter-free output. Always use vSyncCount > 0 when smooth frame pacing is needed.
        So, because Application.targetFrameRate causes frame pacing issues on desktop, if you are targeting a desktop platform, use QualitySettings.vSyncCount instead of this property.

        Web, Android and iOS: Rendering is always limited by the maximum refresh rate of the display. Setting vSyncCount = 0 and targetFrameRate to an arbitrary high value will not exceed
        the display's native refresh rate, even if the rendering workload is sufficiently low.

        Android and iOS: To render at the native refresh rate of the display, set Application.targetFrameRate to the value from the Resolution.refreshRateRatio field of the Screen.currentResolution property.

        iOS: The native refresh rate of the display is controlled by the Apple ProMotion feature. When ProMotion is disabled in the project (default for new projects), the native refresh rate is 60 Hz.
        When ProMotion is enabled, the native refresh rate is 120 Hz on the iOS displays that support ProMotion, 60 Hz otherwise.

        Android and iOS: If the specified rate does not evenly divide the current refresh rate of the display, then the value of Application.targetFrameRate is rounded down to the nearest number that does.
        For example, when running on a 60Hz Android display, and Application.targetFrameRate = 25, then content is effectively rendered at 20fps, since 20 is the highest number below 25 that divides 60 evenly.

        Note that platform and device capabilities affect the frame rate at runtime, so your game might not achieve the target frame rate.
    */

    private void SetTargetFrameRate(int frameRate)
    {
#if UNITY_EDITOR
        // Editor, always disable VSync and use targetFrameRate only
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = frameRate;  // -1;
#else
        if (useSystemFramePacing) {
            // Not divisible → Disable VSync, use targetFrameRate only
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = frameRate;
        }
        else {
            // Divisible → Disable targetFrameRate, VSync controls frame rate
            QualitySettings.vSyncCount = (int) Math.Round(Screen.currentResolution.refreshRateRatio.value / (double) frameRate);
            Application.targetFrameRate = -1;
        }
#endif // #if UNITY_EDITOR
    }
    //------------------------------------------------------------------------------

    private void DoMove(int speed, float deltaTime)
    {
        Vector3 newPos = movingTarget.position;
        newPos.x += speed * deltaTime;
        movingTarget.Translate(newPos - movingTarget.position);
    }
    //------------------------------------------------------------------------------

    //------------------------------------------------------------------------------
    // IMPORTANT: To get these updates work well when using the cinemachine camera
    //            this script must be executed BEFORE CinemachineBrain, so
    //            put before it in "Settings/Script Execution Order"
    //------------------------------------------------------------------------------

    void LateUpdate()
    {
        if (updateType == UpdateType.LateUpdate)
        {
            int multiplier = movementType == MovementType.Layer ? -1 : 1;
            DoMove(multiplier * movementSpeed, Time.deltaTime);
        }
    }
    //------------------------------------------------------------------------------

    void FixedUpdate()
    {
        if (updateType == UpdateType.FixedUpdate)
        {
            int multiplier = movementType == MovementType.Layer ? -1 : 1;
            DoMove(multiplier * movementSpeed, Time.fixedDeltaTime);
        }
    }
    //------------------------------------------------------------------------------

    void Update()
    {
        if (updateType == UpdateType.Update)
        {
            int multiplier = movementType == MovementType.Layer ? -1 : 1;
            DoMove(multiplier * movementSpeed, Time.deltaTime);
        }
    }
    //------------------------------------------------------------------------------

    // Actually called from CinemachineBrain.LateUpdate
    protected void OnCameraUpdated(CinemachineBrain brain)
    {
        if (updateType == UpdateType.CVCUpdate)
        {
            int multiplier = movementType == MovementType.Layer ? -1 : 1;
            DoMove(multiplier * movementSpeed, Time.deltaTime);
        }
    }
    //------------------------------------------------------------------------------

    public void OnSpeedChanged(float speed)
    {
        movementSpeed = (int)speed;
    }
    //------------------------------------------------------------------------------

    public void OnFrameRateChanged(string frameRate)
    {
        targetFrameRate = int.Parse(frameRate);
        SetTargetFrameRate(targetFrameRate);
    }
    //------------------------------------------------------------------------------

    public void OnResolutionXChanged(string resolutionX)
    {
        resolution.x = int.Parse(resolutionX);
        SetScreenResolution((int)resolution.x, (int)resolution.y, Screen.fullScreen, Screen.currentResolution.refreshRateRatio);
    }
    //------------------------------------------------------------------------------

    public void OnResolutionYChanged(string resolutionY)
    {
        resolution.y = int.Parse(resolutionY);
        SetScreenResolution((int)resolution.x, (int)resolution.y, Screen.fullScreen, Screen.currentResolution.refreshRateRatio);
    }
    //------------------------------------------------------------------------------

    public void OnPacingChanged(bool useSystemFramePacing)
    {
        this.useSystemFramePacing = useSystemFramePacing;
        SetTargetFrameRate(targetFrameRate);
    }
    //------------------------------------------------------------------------------

    public void OnUpdateTypeChanged(int updateTypeNdx)
    {
        SetUpdateType((UpdateType)updateTypeNdx);
    }
    //------------------------------------------------------------------------------

    public void OnMovementTypeChanged(int movementTypeNdx)
    {
        SetMovementType((MovementType)movementTypeNdx);
    }
    //------------------------------------------------------------------------------

    public void OnRestartPressed()
    {
        transform.position = initialPosition;
        layer.position = initialLayerPosition;
        cinemachineVCamera.transform.position = initialCVCamPosition;
        unityCamera.transform.position = initialUnityCamPosition;
    }
    //------------------------------------------------------------------------------
}
//------------------------------------------------------------------------------
