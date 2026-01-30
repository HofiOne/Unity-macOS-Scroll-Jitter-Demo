I have been fighting with this issue for a long time and have been considering it a bug.

The problem can be experienced only on macOS/tvOS (the worst is macOS) in standalone build targets using the Metal Graphics API; even high-performing modern Apple devices are affected (interestingly, the issue does not appear when running inside the Unity Editor, and of course, neither in the WebGL version).

The problem does not affect even the weakest PC hardware with Windows + DirectX, Linux + Vulkan, or WebGL targets, not even so old iOS + Metal devices; the demo project runs smoothly with no issues at all.

And now the problem.

It simply side-scrolls a tilemap that should be a smooth movement from right to left, but on macOS/tvOS (and only on those targets), it visibly jitters periodically.

Using the UI of the demo project, you can adjust several options that can affect how the scroll effect is generated, which might ultimately show that the actual method is not the cause of the problem, as almost all method combinations perform similarly. There is a good chance the issue lies somewhere in the Metal driver.

Of course, I've spent a huge amount of time trying various quality settings and profiles of the used URP renderer. The current macOS desktop default in the project should not use too many resources, as far as my knowledge goes.

Turning off the UI (press `G` for that) and checking the frame debugger also shows that only 3 render passes are needed to draw the scene, so that should not be the problem either.

Using the monitor's native resolution and/or Full Screen mode (when macOS Game Mode is activated) does not help either.

The worst scenario is when running the demo app on an external display (the built-in display of my MacBook Pro is affected as well, but shows slightly better results).

The profiler also directs the suspicion to the driver: in the CPU usage view, it seems everything is processed under 1 ms, but the rendering shows periodic spikes for some reason, like:

<img width="1776" height="183" alt="Screenshot 2026-01-30 at 18 24 14" src="https://github.com/user-attachments/assets/aa4f46c1-3096-4493-a1f2-2fc62ff395b7" />

Do not run the project from the editor; please build a standalone macOS app for testing.

On the UI:

1. You can select multiple methods to generate the scroll effect:
- `Follow target` — (default) uses the `CinemachineVirtualCamera` component, sets its `Follow` target, and adjusts its transform
- `Layer` — adjusts the transform of the GameObject that contains the tilemap
- `Unity Camera` — disables the `CinemachineVirtualCamera` component, gets the Main Camera, and adjusts the Main Camera transform
- `Cinemachine VCamera` — uses the `CinemachineVirtualCamera` component and adjusts the camera transform

2. You can select the update type, which determines where the above methods perform their transform adjustments:
- `Late update` — (default) from the Unity `LateUpdate()` function
- `Update` — from the Unity `Update()` function
- `Fixed update` — from the Unity `FixedUpdate()` function
- `Cinemachine Camera` — from a Unity `CinemachineCore.CameraUpdatedEvent` listener function (actually from `CinemachineBrain.LateUpdate()`)

3. You can adjust the window resolution (default is 1980x1200) and the scroll (transform change) speed, or you can restart the scrolling from the beginning.

4. The most important settings here, I believe, are `Use system frame pacing` and `Target Frame Rate`, which ultimately affect the Unity `Application.targetFrameRate` and `QualitySettings.vSyncCount` settings.

At startup, `Target Frame Rate` is set to -1 and frame pacing is turned off, which results in `Application.targetFrameRate` -1 and `QualitySettings.vSyncCount` 0. This means Unity will render as many frames as it can (on my old M1 MacBook Pro, it is somewhere around 1000 FPS).

To achieve the best settings, the [Unity documentation](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Application-targetFrameRate.html) suggests that for standalone desktop apps you turn on frame pacing, set the target frame rate to the actual refresh rate of your monitor, and then turn off frame pacing. This results in `Application.targetFrameRate` -1 and `QualitySettings.vSyncCount` 1, which means Unity will use vSync to control the frame rate.

Here you should experience the issue this demo is trying to showcase.

It is still possible that I missed something here, so any suggestions are very welcome!

Thanks in advance.
