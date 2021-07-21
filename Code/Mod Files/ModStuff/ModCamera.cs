using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace ModStuff
{
    public class ModCamera : Singleton<ModCamera>
    {
        //References
        Entity _player;
        GameObject wobblecam;
        PlayerController playercont;

        //Camera transforms
        public Vector3 cameraRotation;
        public Vector3 cameraPosition;

        //Current mode
        public int fpsmode;

        //Debug commands variables
        public float cam_fov = 35f;
        public float default_cam_fov = 35f;
        public float optional_cam_fov = 65f;
        public float cam_accel = 0.5f;
        public float free_cam_factor = 1.15f;

        public float cam_sens = 5f;
        public bool lock_vertical;
        public int cam_fc_unlock_player;
        public string[] cam_controls = new string[] { "W", "S", "A", "D", "Alpha1", "Alpha2", "Alpha3", "Alpha4" };
        public float cam_farclip = 1000000f;

        //Main bool variables
        bool ittleUsesCameraRot;
        bool thirdperson;
        bool freecam;
        bool hideHead;

        //Anti pause jump
        int pauseLateUpdate;
        const int MAX_PAUSE_DELAY = 10;

        //Mouse calibration
        bool centerfound;
        int center_counter;
        Vector3 oldPosition;
        Vector3 screencenter;
        float screenWidth;
        float screenHeight;

        //Transforms
        Transform ittle_head;
        Transform playerTransform;
        Transform playerMesh;
        Transform mainCamera;
        Transform outlineCamera;
        Transform audioListener;
        Transform camFocus;

        //For creative menu
        Vector3 prevCamPosition;
        Vector3 prevCamRotation;

        //Control keys
        KeyCode fps_right;
        KeyCode fps_left;
        KeyCode fps_forward;
        KeyCode fps_backward;

        //Extra controls
        float flyingSpeed;
        float currentSpeed;
        const float FLYING_ACCEL_FACTOR = 12f;

        //Third person variables
        float camTargetDistance;
        float camDistance;
        AnimationCurve zoomInCurve;

        //Lerp
        bool lerping;
        bool lerpingPanOverride;
        public enum LerpType { LINEAL, EASY, EASYIN, EASYOUT }
        AnimationCurve[] lerpCurves;
        float lerpTargetTime;
        float lerpCurrentTime;

        //Awake
        void Awake()
        {
            GameStateNew.OnPlayerSpawn += OnLoadNewScene;
            if (_player == null && GameObject.Find("PlayerEnt") != null) OnLoadNewScene(false);
            //Configure animation curves
            zoomInCurve = AnimationCurve.EaseInOut(0f, 0.5f, 10f, 1.5f);
            zoomInCurve.preWrapMode = WrapMode.ClampForever;
            zoomInCurve.postWrapMode = WrapMode.ClampForever;
        }

        //Called on each new scene
        void OnLoadNewScene(bool respawn)
        {
            if (respawn) return;

            //Main objects
            _player = GameObject.Find("PlayerEnt").GetComponent<Entity>();
            wobblecam = GameObject.Find("Cameras").transform.parent.gameObject;
            playercont = GameObject.Find("PlayerController").GetComponent<PlayerController>();
            //Get transforms
            playerTransform = _player.RealTransform;
            playerMesh = playerTransform.Find("Ittle");
            ittle_head = playerMesh.Find("ittle").Find("Armature").Find("root").Find("chest").Find("head"); ;
            //Find camera transforms
            mainCamera = GameObject.Find("Main Camera").transform;
            outlineCamera = GameObject.Find("OutlineCamera").transform;
            audioListener = GameObject.Find("Main Camera").transform.GetChild(0);
            //Reset creative menu focus
            camFocus = playerTransform;

            if (fpsmode != 0) { FPSToggle(fpsmode, true); } else { CamConfig(); }
        }

        //Change ittle's head size for fps mode
        void LateUpdate()
        {
            pauseLateUpdate++;
            if (_player != null && hideHead) { ittle_head.localScale = Vector3.zero; }
        }

        //Set up mode
        public void FPSToggle(int mode, bool update)
        {
            fpsmode = mode;
            bool nonDefaultCamera = (fpsmode != 0 ? true : false);

            cam_fov = (mode > 0 ? optional_cam_fov : default_cam_fov);
            Transform cameraTransf = wobblecam.transform.Find("Cameras");
            prevCamPosition = mainCamera.position;
            prevCamRotation = mainCamera.eulerAngles;
            //First person and free camera
            if (mode == 1 || mode == 3)
            {
                //Move Cameras, gameobject parent of the cameras
                cameraTransf.localPosition = Vector3.zero;

                //Change MainCamera and OutlineCamera positions
                mainCamera.localPosition = Vector3.zero;
                outlineCamera.localPosition = Vector3.zero;

                //Change AudioListener position
                audioListener.localPosition = Vector3.zero;

                // Disable culling for pathfinding ents
                OnScreenCanceller waitState;
                OnScreenCanceller walkState;
                if (GameObject.Find("SecretPathController(Clone)") != null)
                {
                    Transform pathController = GameObject.Find("SecretPathController(Clone)").transform;
                    waitState = pathController.Find("WaitState").GetComponent<OnScreenCanceller>();
                    walkState = pathController.Find("WalkState").GetComponent<OnScreenCanceller>();

                    waitState._minTime = 0.1f;
                    walkState._minTime = 999f;
                }
            }
            //Third person
            else if (mode == 2)
            {
                //Move Cameras, gameobject parent of the cameras
                cameraTransf.localPosition = Vector3.zero;
            }
            //Default
            else
            {
                //Move Cameras, gameobject parent of the cameras
                cameraTransf.localPosition = new Vector3(0f, 1f, 0f);

                //Change MainCamera and OutlineCamera positions
                mainCamera.localPosition = new Vector3(0f, -0.5f, -2.3f);
                outlineCamera.localPosition = new Vector3(0f, -0.5f, -2.3f);

                //Setup AudioListener
                audioListener.localPosition = new Vector3(0f, 0f, 15f);
            }

            //Remove art exhibit reflection when outside default mode
            GameObject artExhibitReflection = GameObject.Find("Refl");
            if (artExhibitReflection != null)
            {
                UnityEngine.MeshRenderer reflection = artExhibitReflection.GetComponent<UnityEngine.MeshRenderer>();
                if (reflection != null) { reflection.enabled = !nonDefaultCamera; }
            }

            //Setup flags
            switch (mode)
            {
                //Default mode
                case 0:
                    Cursor.visible = true;
                    hideHead = false;
                    freecam = false;
                    thirdperson = false;
                    ittleUsesCameraRot = false;

                    // If camera light was added, disable it
                    //Light camLight = mainCamera.gameObject.GetComponent<Light>();
                    //if (camLight != null) camLight.enabled = false;
                    break;
                //First person mode
                case 1:
                    hideHead = true;
                    freecam = false;
                    thirdperson = false;
                    ittleUsesCameraRot = true;
                    break;
                //Third person mode
                case 2:
                    hideHead = false;
                    freecam = false;
                    thirdperson = true;
                    ittleUsesCameraRot = false;
                    camDistance = -5f;
                    camTargetDistance = camDistance;
                    break;
                //Free mode
                case 3:
                    hideHead = false;
                    freecam = true;
                    thirdperson = false;
                    ittleUsesCameraRot = false;
                    cameraPosition.y += 1f;
                    flyingSpeed = 0.2f;
                    break;
                default:
                    break;
            }

            if (nonDefaultCamera)
            {
                //If third mode, set the camera
                if (mode == 2) { UpdateThirdPersonCameraPosition(); }

                //Reset mouse calibration
                if (screenWidth != Screen.width || screenHeight != Screen.height)
                {
                    center_counter = 0;
                    centerfound = false;
                    screenWidth = Screen.width;
                    screenHeight = Screen.height;
                }

                //Set initial rotation and position for the camera
                if (update)
                {
                    if (thirdperson)
                    {
                        cameraRotation = new Vector3(45f, playerTransform.localRotation.eulerAngles.y, 0f);
                        cameraPosition = new Vector3(playerTransform.position.x,
                                                     playerTransform.position.y + playerMesh.localScale.y * 1.40f,
                                                     playerTransform.position.z);
                    }
                    else if (freecam)
                    {
                        cameraRotation = new Vector3(45f, 0f, 0f);
                        cameraPosition = new Vector3(playerTransform.position.x,
                                                     playerTransform.position.y + playerMesh.localScale.y * 1.40f + 5f,
                                                     playerTransform.position.z - 5f);

                        // Add a spotlight

                        /*
                        Light camLight = mainCamera.gameObject.GetComponent<Light>();
                        if (camLight == null)
                        {
                            camLight = mainCamera.gameObject.AddComponent<Light>();
                            camLight.type = LightType.Spot;
                            camLight.color = Color.white;
                            camLight.range = 1000;
                        }
                        camLight.enabled = true;*/
                    }
                    else //First person
                    {
                        cameraRotation = new Vector3(0f, playerTransform.localRotation.eulerAngles.y, 0f);
                        cameraPosition = new Vector3(playerTransform.position.x,
                                                     playerTransform.position.y + 1.05f,
                                                     playerTransform.position.z);
                    }
                }
                else
                {
                    cameraPosition = prevCamPosition;
                    cameraRotation = prevCamRotation;
                }
            }
            camFocus = playerTransform;

            //Setup camera movement
            wobblecam.GetComponent<FollowTransform>().SetupFPSMode(nonDefaultCamera);
            wobblecam.GetComponent<LevelCamera>().SetupFPSMode(nonDefaultCamera);
            wobblecam.GetComponent<CameraContainer>().SetupFPSMode(nonDefaultCamera);

            //Setup player controller
            playercont.SetupFPSMode(mode);

            //Disable lerp
            lerping = false;

            //Setup camera attributes
            CamConfig();
        }

        //Configure cameras and controls
        public void CamConfig()
        {
            //Setup cameras
            Camera[] scenecameras = new Camera[]
            {
                mainCamera.gameObject.GetComponent<Camera>(),
                outlineCamera.gameObject.GetComponent<Camera>()
            };
            foreach (Camera cam in scenecameras)
            {
                cam.fieldOfView = cam_fov;
                cam.nearClipPlane = fpsmode != 0 ? 0.1f : 0.3f;
                cam.farClipPlane = cam_farclip;
            }

            //Setup PlayerController
            if (lock_vertical) { cameraRotation.x = 0f; }
            playercont.FreeCamPlayerMovement(cam_fc_unlock_player);
            SetFpsControls();
        }

        void UpdateThirdPersonCameraPosition()
        {
            mainCamera.localPosition = new Vector3(0f, -0.5f, camDistance);
            outlineCamera.localPosition = mainCamera.localPosition;
            audioListener.localPosition = new Vector3(0f, 0f, -camDistance);
        }

        static public string PrintCamControls(string[] controls)
        {
            return "Forward: " + controls[0] + " - Backward: " + controls[1] + " - Left: " + controls[2] + " - Right: " + controls[3] + "\n" +
                  "Stick: " + controls[4] + " - Force wand: " + controls[5] + " - Dynamite: " + controls[6] + " - Ice ring: " + controls[7] + "\n";
        }

        //Creative menu functions

        public void ChangeFocus(Transform focus)
        {
            if (focus == null || !thirdperson) return;
            camFocus = focus;
            cameraPosition = new Vector3(camFocus.position.x,
                                            camFocus.position.y,
                                            camFocus.position.z);
        }

        public void CMenuZoomIn()
        {
            cameraPosition += mainCamera.transform.forward * 6f;
        }

        //Setup rotation key
        public void SetFpsControls()
        {
            fps_forward = (KeyCode)Enum.Parse(typeof(KeyCode), cam_controls[0]);
            playercont.fps_forward = fps_forward;
            fps_backward = (KeyCode)Enum.Parse(typeof(KeyCode), cam_controls[1]);
            playercont.fps_backward = fps_backward;
            fps_left = (KeyCode)Enum.Parse(typeof(KeyCode), cam_controls[2]);
            playercont.fps_left = fps_left;
            fps_right = (KeyCode)Enum.Parse(typeof(KeyCode), cam_controls[3]);
            playercont.fps_right = fps_right;

            playercont.weapon_buttons = new KeyCode[4];
            playercont.weapon_buttons[0] = (KeyCode)Enum.Parse(typeof(KeyCode), cam_controls[4]);
            playercont.weapon_buttons[1] = (KeyCode)Enum.Parse(typeof(KeyCode), cam_controls[5]);
            playercont.weapon_buttons[2] = (KeyCode)Enum.Parse(typeof(KeyCode), cam_controls[6]);
            playercont.weapon_buttons[3] = (KeyCode)Enum.Parse(typeof(KeyCode), cam_controls[7]);
        }

        //Update camera's position and rotation
        public void ModCamUpdate()
        {
            if (lerping)
            {
                LerpUpdate();
            }
            else
            {
                if (freecam && cam_fc_unlock_player == 3)
                {
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                }
                else
                {
                    //Pan camera if fpsmode is on, then move if freecam is on
                    CameraPan();
                    if (freecam)
                    {
                        FreeCamMove();
                    }
                }
            }
        }

        //Free camera movement
        void FreeCamMove()
        {
            // If space key is pressed, teleport player to camera position
            //Disabling this for now, since it could interfere with UI
            /*
            if (Input.GetKeyDown(KeyCode.Space))
            {
                GameObject.Find("PlayerEnt").transform.position = cameraPosition;
                CameraContainer roomCamChanger = GameObject.Find("Cameras").transform.parent.gameObject.GetComponent<CameraContainer>();
                LevelRoom newRoom = LevelRoom.GetRoomForPosition(cameraPosition);
                roomCamChanger.SetRoom(newRoom);free_cam_factor
            }*/

            //Check if the scroll wheel moved
            if (Input.mouseScrollDelta.y < 0 || Input.mouseScrollDelta.y > 0)
            {
                flyingSpeed *= (Input.mouseScrollDelta.y > 0f) ? free_cam_factor : 1f / free_cam_factor;
            }

            //Check the movement direction with the keys
            Vector2 vector;
            vector.x = (Input.GetKey(fps_right) ? 1f : 0f) - (Input.GetKey(fps_left) ? 1f : 0f);
            vector.y = (Input.GetKey(fps_forward) ? 1f : 0f) - (Input.GetKey(fps_backward) ? 1f : 0f);

            //This is the vector pointing in the camera direction
            Vector3 movementdirection = new Vector3
            (
                Mathf.Cos(cameraRotation.x * 0.0174532924f) * Mathf.Sin(cameraRotation.y * 0.0174532924f),
                -Mathf.Sin(cameraRotation.x * 0.0174532924f),
                Mathf.Cos(cameraRotation.x * 0.0174532924f) * Mathf.Cos(cameraRotation.y * 0.0174532924f)
            );
            //Set the direction (positive goes forward, negative backward)
            movementdirection *= vector.y;

            //Remove y component of keys vector and rotate it according to the camera direction
            vector.y = 0f;
            vector = RotByDegrees(vector, -cameraRotation.y);
            movementdirection.x += vector.x;
            movementdirection.z += vector.y;

            //Move
            if (movementdirection.sqrMagnitude == 0) currentSpeed = 0f;
            else
            {
                currentSpeed += flyingSpeed / FLYING_ACCEL_FACTOR;
                if (currentSpeed > flyingSpeed) currentSpeed = flyingSpeed;
            }
            cameraPosition += movementdirection * currentSpeed;
        }

        //Camera pan
        void CameraPan()
        {
            //Lock and unlock the mouse, check how much the mouse moved in a frame and use it as the mouse axis
            Cursor.visible = false;
            Vector3 newPosition = Input.mousePosition;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.lockState = CursorLockMode.None;

            //Calibration code. If the mouse stays in the same place for 5 frames, take its current position as the middle of the screen
            if (!centerfound)
            {
                if (newPosition == oldPosition)
                {
                    center_counter++;
                    if (center_counter >= 5)
                    {
                        centerfound = true;
                        screencenter = newPosition;
                    }
                }
                else
                {
                    center_counter = 0;
                    oldPosition = newPosition;
                }
                return;
            }
            //If the the game was paused, negate this frame movement to avoid pan jumps when unpausing
            if (pauseLateUpdate > MAX_PAUSE_DELAY)
            {
                pauseLateUpdate = 0;
                return;
            }
            pauseLateUpdate = 0;

            //Calculate mouse axis
            Vector3 deltaPosition = newPosition - screencenter;

            //Calculate Y rotation
            cameraRotation.y += Time.deltaTime * cam_sens * deltaPosition.x;

            //If vertical axis is not locked, update it
            if (!lock_vertical) { cameraRotation.x = Mathf.Clamp(cameraRotation.x - cam_sens * Time.deltaTime * deltaPosition.y, -90f, +90f); }

            //Make the player turn to the camera's direction
            if (ittleUsesCameraRot)
            {
                _player.TurnTo(cameraRotation.y, 0f);
            }
            //Update third person camera distance to player
            if (thirdperson)
            {
                if ((camFocus != playerTransform) || Input.GetKey(KeyCode.Mouse1))
                {
                    if (Input.mouseScrollDelta.y < 0 || Input.mouseScrollDelta.y > 0)
                    {
                        int scrollDirection = (Input.mouseScrollDelta.y < 0 ? 1 : -1);
                        camTargetDistance = Mathf.Clamp(camTargetDistance - scrollDirection * 0.4f, -25f, -0.5f);
                    }
                }
                if (camTargetDistance != camDistance)
                {
                    //Damp the third person camera zoom in/out
                    float deltaDistance = camTargetDistance - camDistance;
                    camDistance = camDistance + Mathf.Sign(deltaDistance) * zoomInCurve.Evaluate(Mathf.Abs(deltaDistance)) * 0.15f;
                    if (Math.Abs(camDistance - camTargetDistance) < 0.05) { camDistance = camTargetDistance; }
                    UpdateThirdPersonCameraPosition();
                }
            }

            //If the camera is not free, update its position to the camfocus
            if (!freecam)
            {
                cameraPosition = new Vector3(camFocus.position.x,
                                             camFocus.position.y + ((camFocus == playerTransform) ? (playerMesh.localScale.y * (thirdperson ? 1.40f : 1.05f)) : 0f),
                                             camFocus.position.z);
            }
        }

        public string GetModCameraTransform()
        {
            string output = cameraPosition.x + " " + cameraPosition.y + " " + cameraPosition.z + " " + cameraRotation.x + " " + cameraRotation.y + " " + cameraRotation.z;
            GUIUtility.systemCopyBuffer = output;
            return output;
        }

        public void SetModCameraTransform(float xPos, float yPos, float zPos, float xRot, float yRot)
        {
            if (fpsmode == 3)
            {
                cameraRotation = new Vector3(xRot, yRot, 0f);
                cameraPosition = new Vector3(xPos, yPos, zPos);
            }
        }

        public void SetModCameraTransform(float xPos, float yPos, float zPos, float xRot, float yRot, float zRot)
        {
            if (fpsmode == 3)
            {
                cameraRotation = new Vector3(xRot, yRot, zRot);
                cameraPosition = new Vector3(xPos, yPos, zPos);
            }
        }

        public void LerpTo(float time, LerpType type, float xPos, float yPos, float zPos, float xRot, float yRot)
        {
            if (fpsmode != 3) return;
            lerpCurves = new AnimationCurve[5];
            lerpCurves[0] = CreateCurve(time, type, cameraPosition.x, xPos);
            lerpCurves[1] = CreateCurve(time, type, cameraPosition.y, yPos);
            lerpCurves[2] = CreateCurve(time, type, cameraPosition.z, zPos);
            lerpCurves[3] = CreateCurve(time, type, cameraRotation.x, xRot);
            lerpCurves[4] = CreateCurve(time, type, cameraRotation.y, yRot);
            lerping = true;
            lerpTargetTime = time;
            lerpCurrentTime = 0f;
        }

        public void LerpTo(float time, LerpType type, float xPos, float yPos, float zPos, float xRot, float yRot, float zRot)
        {
            if (fpsmode != 3) return;
            lerpCurves = new AnimationCurve[6];
            lerpCurves[0] = CreateCurve(time, type, cameraPosition.x, xPos);
            lerpCurves[1] = CreateCurve(time, type, cameraPosition.y, yPos);
            lerpCurves[2] = CreateCurve(time, type, cameraPosition.z, zPos);
            lerpCurves[3] = CreateCurve(time, type, cameraRotation.x, xRot);
            lerpCurves[4] = CreateCurve(time, type, cameraRotation.y, yRot);
            lerpCurves[5] = CreateCurve(time, type, cameraRotation.z, zRot);
            lerping = true;
            lerpTargetTime = time;
            lerpCurrentTime = 0f;
        }

        AnimationCurve CreateCurve(float time, LerpType type, float start, float end)
        {
            AnimationCurve curve = new AnimationCurve();
            float interpStart = 0f;
            float interpEnd = 0f;
            switch (type)
            {
                case LerpType.LINEAL:
                    interpStart = (end - start) / time;
                    interpEnd = interpStart;
                    break;
                case LerpType.EASYIN:
                    interpStart = (end - start) / time;
                    interpEnd = 0f;
                    break;
                case LerpType.EASYOUT:
                    interpStart = 0f;
                    interpEnd = (end - start) / time;
                    break;
                default:
                    break;
            }

            curve.AddKey(new Keyframe(0, start, interpStart, interpEnd));
            curve.AddKey(new Keyframe(time, end, interpStart, interpEnd));

            return curve;
        }
        
        public void LerpTo(params LerpPoint[] lerpPoints)
        {
            if (fpsmode != 3 || lerpPoints == null || lerpPoints.Length == 0) return;
            lerpCurves = new AnimationCurve[6];
            lerpCurves[0] = CreateCurve(cameraPosition.x, lerpPoints, x => x._px);
            lerpCurves[1] = CreateCurve(cameraPosition.y, lerpPoints, x => x._py);
            lerpCurves[2] = CreateCurve(cameraPosition.z, lerpPoints, x => x._pz);
            lerpCurves[3] = CreateCurve(cameraRotation.x, lerpPoints, x => x._rx);
            lerpCurves[4] = CreateCurve(ClampTo360(cameraRotation.y), lerpPoints, x => ClampTo360(x._ry), true);
            lerpCurves[5] = CreateCurve(ClampTo360(cameraRotation.z), lerpPoints, x => ClampTo360(x._rz), true);
            lerping = true;
            lerpCurrentTime = 0f;
        }

        float ClampTo180(float angle)
        {
            return Mathf.Repeat(angle + 180f, 360f) - 180f;
        }

        float ClampTo360(float angle)
        {
            return Mathf.Repeat(angle, 360f);
        }

        float AngleDif180(float angleTo, float angleFrom)
        {
            return Mathf.Repeat(angleTo - angleFrom + 180f, 360f) - 180f;
        }
        
        AnimationCurve CreateCurve(float start, LerpPoint[] lerpPoints, Func<LerpPoint,float> func, bool isRot = false)
        {
            AnimationCurve curve = new AnimationCurve();
            float currentTime = 0f;
            float lastTime = 0f;
            float lastPoint = start;
            float accumulatedRotValue = start;
            float finalAccumulatedRotValue = start;
            float initialTangent = isRot ? AngleDif180(func(lerpPoints[0]), start) / lerpPoints[0]._time : (func(lerpPoints[0]) - start) / lerpPoints[0]._time;
            curve.AddKey(new Keyframe(0, start, 0f, 0f));
            //if (evaluate) ModText.QuickText(string.Format("i: {0} / time: {1} / Value {2} / InTangent: {3} / OutTangent {4}", "nada", 0, start, 0f, 0f));

            if(isRot)
            {
                for (int i = 0; i < lerpPoints.Length; i++)
                {
                    finalAccumulatedRotValue += AngleDif180(func(lerpPoints[i]), finalAccumulatedRotValue);
                    //ModText.QuickText(finalAccumulatedRotValue.ToString());
                }
            }

            for (int i = 0; i < lerpPoints.Length; i++)
            {
                lastTime = currentTime;
                currentTime += lerpPoints[i]._time;
                float endPoint = func(lerpPoints[i]);
                if(isRot)
                {
                    //Calculate difference between accumulated and next
                    float dif = AngleDif180(endPoint, accumulatedRotValue);
                    endPoint = accumulatedRotValue + dif;
                    accumulatedRotValue = endPoint;
                    /*if(dif < 0)
                    {
                        ModText.QuickText("Negative rotation next!");
                        endPoint -= 360f;
                    }*/
                }
                float startingStartPoint;
                float finalEndPoint;
                float timeTaken = 0f;

                float tangent;
                if (isRot)
                {
                    //Not starting point
                    if (i > 0)
                    {
                        startingStartPoint = lastPoint;
                        timeTaken += lerpPoints[i]._time;
                    }
                    else
                    {
                        startingStartPoint = start;
                    }
                    //Not ending point
                    if (i < lerpPoints.Length - 1)
                    {
                        finalEndPoint = AngleDif180(func(lerpPoints[i + 1]), accumulatedRotValue) + accumulatedRotValue;
                        timeTaken += lerpPoints[i + 1]._time;
                    }
                    else
                    {
                        finalEndPoint = finalAccumulatedRotValue;
                    }
                    //tangent = AngleDif180(finalEndPoint, startingStartPoint) / timeTaken;
                    tangent = (finalEndPoint - startingStartPoint) / timeTaken;
                }
                else
                {
                    
                    //Not starting point
                    if (i > 0)
                    {
                        startingStartPoint = lastPoint;
                        timeTaken += lerpPoints[i]._time;
                    }
                    else
                    {
                        startingStartPoint = start;
                    }
                    //Not ending point
                    if (i < lerpPoints.Length - 1)
                    {
                        finalEndPoint = func(lerpPoints[i + 1]);
                        timeTaken += lerpPoints[i + 1]._time;
                    }
                    else
                    {
                        finalEndPoint = func(lerpPoints[lerpPoints.Length - 1]);
                    }

                    tangent = (finalEndPoint - startingStartPoint) / timeTaken;
                }

                lastPoint = endPoint;
                curve.AddKey(new Keyframe(currentTime, endPoint, i == lerpPoints.Length - 1 ? 0f : tangent, tangent));
                //if (isRot) ModText.QuickText(string.Format("i: {0} / time: {1} / Value {2} / InTangent: {3} / OutTangent {4}", i, currentTime, endPoint, i == lerpPoints.Length - 1 ? 0f : tangent, tangent));

            }
            lerpTargetTime = currentTime; //Yeah, it will be written 5 times. oh well

            return curve;
        }

        void LerpUpdate()
        {
            Cursor.visible = false;
            lerpCurrentTime += Time.deltaTime;
            cameraPosition.x = lerpCurves[0].Evaluate(lerpCurrentTime);
            cameraPosition.y = lerpCurves[1].Evaluate(lerpCurrentTime);
            cameraPosition.z = lerpCurves[2].Evaluate(lerpCurrentTime);
            cameraRotation.x = lerpCurves[3].Evaluate(lerpCurrentTime);
            cameraRotation.y = ClampTo360(lerpCurves[4].Evaluate(lerpCurrentTime));
            cameraRotation.z = ClampTo360(lerpCurves[5].Evaluate(lerpCurrentTime));
            if (lerpCurrentTime >= lerpTargetTime)
            {
                lerping = false;
                lerpingPanOverride = false;
            }
        }

        public Vector3 PlayerDirectionToVector3()
        {
            return new Vector3(Mathf.Sin(cameraRotation.y * 0.0174532924f), 0f, Mathf.Cos(cameraRotation.y * 0.0174532924f));
        }

        public static Vector2 RotByDegrees(Vector2 v, float degrees)
        {
            float num = Mathf.Sin(degrees * 0.0174532924f);
            float num2 = Mathf.Cos(degrees * 0.0174532924f);
            float x = v.x;
            float y = v.y;
            v.x = num2 * x - num * y;
            v.y = num * x + num2 * y;
            return v;
        }

        public class LerpPoint
        {
            public float _time;
            public float _px;
            public float _py;
            public float _pz;
            public float _rx;
            public float _ry;
            public float _rz;

            public LerpPoint(float time, float px, float py, float pz, float rx, float ry)
            {
                _px = px;
                _py = py;
                _pz = pz;
                _rx = rx;
                _ry = ry;
                _time = time;
            }
            
            public LerpPoint(float time, float px, float py, float pz, float rx, float ry, float rz)
            {
                _px = px;
                _py = py;
                _pz = pz;
                _rx = rx;
                _ry = ry;
                _rz = rz;
                _time = time;
            }

            override public string ToString()
            {
                return string.Format("Time: {0}. Position: ({1}, {2}, {3}). Rotation : ({4}, {5}, {6})", _time, _px, _py, _pz, _rx, _ry, _rz);
            }
        }
    }
}