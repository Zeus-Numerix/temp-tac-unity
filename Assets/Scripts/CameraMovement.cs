using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using IPRObjectNameSpace;
using SuperImposeNameSpace;

namespace CameraScript
{
    public class CameraMovement : MonoBehaviour {
        //the center of the camera rotate sphere
        // public Transform target;
        public static Camera sceneCamera;
        public Light light;
        public Light sun;
        public GameObject background;

        public static GameObject upper;
        public static GameObject lower;

        public Material backgroundMaterial;
    
        [Range(0f, 15f)]
        [Tooltip("How sensitive the mouse drag to camera rotation")]
        public float mouseRotateSpeed = 5;
    
        [Range(0f, 50f)]
        [Tooltip("How sensitive the touch drag to camera rotation")]
        public float touchRotateSpeed = 1;
    
        [Tooltip("Smaller positive value means smoother rotation, 1 means no smooth apply")]
        public float slerpSmoothValue = 0.3f;
        [Tooltip("How long the smoothDamp of the mouse scroll takes")]
        public float scrollSmoothTime = 0.12f;
        public float editorFOVSensitivity = 5;
        public float touchFOVSensitivity = 5;
        public float zoomSensitivity = 50;
        public float mobileZoomSensitivity = 10;
        public float maxMobileZoom = 50;
        public float minMobileZoom = 1000;
        public float maxZoom = 50;
        public float minZoom = 500;
        public float maxPanX = 38f;
        public float minPanX = -38f;
        public float maxPanY = 15f;
        public float minPanY = -25f;
        [SerializeField]
        private float backgroundDistance = 100;
        
    
        //Can we rotate camera, which means we are not blocking the view
        private bool canRotate = true;
    
        private Vector2 swipeDirection; //swipe delta vector2
        private Vector2 swipeDirectionPan;
        private Vector2 touch1OldPos;
        private Vector2 touch2OldPos;
        private Vector2 touch1CurrentPos;
        private Vector2 touch2CurrentPos;

        private Vector2 initialTouch1Pos;
        private Vector2 initialTouch2Pos;
        private Quaternion currentRot; // store the quaternion after the slerp operation
        private Quaternion targetRot;
        private Touch touch;
    
        //Mouse rotation related
        private float rotX = 0f; // around x
        private float rotY = 0f; // around y

        private float panX = 0f; // pan on X axis
        private float panY = 0f; // pan on Y axis

        [SerializeField]
        [Range(0f, 15f)]
        private float panFactor = 1f; // 
        [SerializeField]
        [Range(0f, 15f)]
        private float rotateFactor = 1f; // 
        [SerializeField]
        [Range(0f, 15f)]
        private float baseOffset = 15f;
        [SerializeField]
        [Range(0f, 50f)]
        private float shadowOffset = 27f;

        //Mouse Scroll
        [SerializeField]
        private float cameraFieldOfView;
        private float cameraFOVDamp; //Damped value
        private float fovChangeVelocity = 0;
        private static float distanceBetweenCameraAndTarget;
        //Clamp Value
        [SerializeField]
        private float minXRotAngle = -60; //min angle around x axis
        [SerializeField]
        private float maxXRotAngle = 80; // max angle around x axis

        [SerializeField] private float minXRotAngleOneJaw = -80;
        [SerializeField] private float maxXRotAngleOneJaw = 80;

        [SerializeField]
        private float MaxilaryViewAngle = 80; // Angle at which Maxillary view is set
        [SerializeField]
        private float MandibularViewAngle = -60; // Angle at which Madibular view is set

        private float maxZoomTouchPanFactor = 0.02f;
        private float minZoomTouchPanFactor = 0.25f;
    
        [SerializeField]
        private float minCameraFieldOfView = 6;
        [SerializeField]
        private float maxCameraFieldOfView = 30;

        [SerializeField]
        private bool isMobile = false;

        private bool baseState = true;

        public static bool upperViewActive = true;
        public static bool lowerViewActive = true;

        public static List<GameObject> UpperObjects = new List<GameObject>(); 
        public static List<GameObject> LowerObjects = new List<GameObject>(); 
        private List<GameObject> Attachments = new List<GameObject>();

        public static Bounds bounds;
        enum DragModes
        {
            rotate,
            pan
        }

        enum PinchModes {
            NA,
            zoom,
            pan
        }

        [SerializeField]
        private DragModes dragMode = DragModes.rotate;

        private PinchModes pinchMode = PinchModes.NA;

        private Vector3 lookAtPt = new Vector3();

        Vector3 dir;

        // JavaScript Callbacks Declaration
        #if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void getBoundsSuccess();
        [DllImport("__Internal")]
        private static extern void getUpperObjectsSuccess();
        [DllImport("__Internal")]
        private static extern void getLowerObjectsSuccess();

        // // for debug and testing purpose 
        // [DllImport("__Internal")]
        // private static extern void getDeltaDistance(string value);
        // [DllImport("__Internal")]
        // private static extern void getPincMode(string value);


        #endif


        private void Awake()
        {
            GetCameraReference();
    
        }
        // Start is called before the first frame update
        void Start()
        {
            #if !UNITY_EDITOR && UNITY_WEBGL
                WebGLInput.captureAllKeyboardInput = false;
            #endif
            distanceBetweenCameraAndTarget = Vector3.Distance(sceneCamera.transform.position, new Vector3(0, 0, 0));
            // setCameraClipingPlane();
            dir = new Vector3(0, 0, distanceBetweenCameraAndTarget);//assign value to the distance between the maincamera and the target
            sceneCamera.transform.position = new Vector3(0, 0, 0) + dir; //Initialize camera position
            cameraFOVDamp = sceneCamera.fieldOfView;
            // sun.transform.position = new Vector3(500, 500, 500);
            // sun.transform.LookAt(new Vector3());
            // cameraFieldOfView = sceneCamera.fieldOfView;
            
        }
    
        // Update is called once per frame
        void Update()
        {
            if (!canRotate)
            {
                return;
            }
            
            //We are in editor
            // EditorCameraInput();
            // if (Application.isEditor || Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WebGLPlayer)
            // {
            //     EditorCameraInput();
            // }
            // else //We are in mobile mode
            // {
            //     TouchCameraInput();
            // }
            if(isMobile){
                TouchCameraInput();
            }
            else{
                EditorCameraInput();
            }
            #if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.F))
            {
                setPlatformMode("mobile");
            }
            if (Input.GetKeyDown(KeyCode.G))
            {
                setPlatformMode("desktop");
            }
            if (Input.GetKeyDown(KeyCode.T))
            {
                if(Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift)){
                    setDarkTheme(0);
                    return;
                }
                setDarkTheme(1);

            }
            if(Input.GetKeyDown(KeyCode.RightArrow)){
                RightView();
            }
            if(Input.GetKeyDown(KeyCode.LeftArrow)){
                LeftView();
            }
            if(Input.GetKeyDown(KeyCode.Space)){
                // if(Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift)){
                //     ToggleAttachments(0);
                //     return;
                // }
                // ToggleAttachments(1);
                // getLowerObjects();
                // getUpperObjects();
                // getBounds();
                getUpperObjects();
                getLowerObjects();
                getBounds();
                baseState = !baseState;
                toggleBaseShadow(baseState ? 1 : 0);
            }
            if(Input.GetKeyDown(KeyCode.UpArrow)){
                MaxillaryView();
            }
            if(Input.GetKeyDown(KeyCode.DownArrow)){
                MandibularView();
            }
            if(Input.GetKeyDown(KeyCode.U)){
                if(Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift)){
                    UpperView(0);
                    return;

                }
                UpperView(1);
            }
            if(Input.GetKeyDown(KeyCode.D)){
                if(Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift)){
                    LowerView(0);
                    return;
                }
                LowerView(1);
            }
            if(Input.GetKeyDown(KeyCode.KeypadPlus) || Input.GetKeyDown(KeyCode.Equals)){
                zoomIn();
            }
            if(Input.GetKeyDown(KeyCode.KeypadMinus) || Input.GetKeyDown(KeyCode.Minus)){
                zoomOut();
            }
            #endif
            // if (Input.GetKeyDown(KeyCode.T))
            // {
            //     TopView();
            // }
            // if (Input.GetKeyDown(KeyCode.L))
            // {
            //     LeftView();
            // }
    
        }
        
        // void OnDrawGizmos(){
        //     getUpperObjects();
        //     getLowerObjects();
        //     getBounds();
        // }

        public void OnDrawGizmos() {
            Gizmos.DrawWireCube( bounds.center, bounds.size );
        }

        private void LateUpdate()
        {
            RotateCamera();
            panFactor = calculatePanFactor(distanceBetweenCameraAndTarget);
            mouseRotateSpeed = calculateRotateFactor(distanceBetweenCameraAndTarget);
            touchRotateSpeed = calculateRotateFactor(distanceBetweenCameraAndTarget);
            // if(dragMode == DragModes.rotate){
            // } else {
            //     panCamera();
            // }
            
            // SetCameraFOV();
            // light.transform.position = sceneCamera.transform.position;
            light.transform.position = (sceneCamera.transform.forward * -1) * 300;
            light.transform.LookAt(new Vector3(0, 0, 0));
            // setBasePosition();
            GameObject[] billboards = GameObject.FindGameObjectsWithTag("Billboard");
            foreach(GameObject bilboard in billboards){
                bilboard.transform.LookAt(sceneCamera.transform.position);
                // bilboard.transform.localRotation = Quaternion.Euler(0,0,90);
            }
            if(UpperObjects.Count > 0 || LowerObjects.Count > 0){
                bool state = lowerViewActive || upperViewActive;
                this.toggleBaseShadow(state ? 1 : 0) ;
            }
            // sun.transform.position = sceneCamera.transform.position + sceneCamera.transform.forward * (distanceBetweenCameraAndTarget - 500) + (sceneCamera.transform.up * 30) + (sceneCamera.transform.right * -80);
            // sun.transform.LookAt(new Vector3());
            // background.transform.position = sceneCamera.transform.position + sceneCamera.transform.forward * (distanceBetweenCameraAndTarget + backgroundDistance);
            // background.transform.LookAt(sceneCamera.transform.position);
        }
    
        public void GetCameraReference()
        {
            if (sceneCamera == null)
            {
                sceneCamera = Camera.main;
            }
    
        }
    
        public void getUpperObjects(){
            // List<GameObject> upperObjects = new List<GameObject>();
            UpperObjects = null;
            UpperObjects = new List<GameObject>();
            GameObject[] objects = GameObject.FindGameObjectsWithTag("Jaw");
            foreach(GameObject obj in objects){
                if(obj.name.StartsWith("upper")){
                    UpperObjects.Add(obj);
                }
            }
            #if UNITY_WEBGL && !UNITY_EDITOR
            getUpperObjectsSuccess();
            #endif
        }

        public void getLowerObjects(){
            LowerObjects = null;
            LowerObjects = new List<GameObject>();
            GameObject[] objects = GameObject.FindGameObjectsWithTag("Jaw");
            foreach(GameObject obj in objects){
                if(obj.name.StartsWith("lower")){
                    LowerObjects.Add(obj);
                }
            }
            #if UNITY_WEBGL && !UNITY_EDITOR
            getLowerObjectsSuccess();
            #endif
        }

        public void getBounds() {
            bounds = new Bounds(Vector3.zero, Vector3.zero);
            foreach(GameObject lower in LowerObjects){
                foreach (Renderer r in lower.GetComponentsInChildren<Renderer>()) {
                    bounds.Encapsulate(r.bounds);
                }
            }
            // JavaScript Callbacks Declaration
            #if UNITY_WEBGL && !UNITY_EDITOR
            getBoundsSuccess();
            #endif
            // foreach(GameObject upper in UpperObjects) {
            //     foreach (Renderer r in upper.GetComponentsInChildren<Renderer>()){
            //         bounds.Encapsulate(r.bounds);
            //     }
            // }
            // Debug.Log(bounds);
            // Gizmos.DrawWireCube( bounds.center, bounds.size );
            // Debug.Log("inside getbounds");
        }

        public void setBasePosition() {
            float edgeY = (bounds.center.y - (bounds.size.y / 2));
            float edgeZ = (bounds.center.z - (bounds.size.z / 2));

            float x = bounds.center.x;
            float y = edgeY - baseOffset;
            float z = bounds.center.z;
            // background.transform.position = new Vector3(bounds.center.x, (bounds.center.y - (bounds.size.y / 2)) - baseOffset, bounds.center.z);
            background.transform.position = new Vector3(x, y, z);
            background.transform.position -= background.transform.forward * (shadowOffset + edgeZ);
        }

        // ########################## Pre set views functions
        public void MaxillaryView()
        {
            // UpperView(1);
            // rotX = MaxilaryViewAngle;
            rotX = 80;
            rotY = 0;

            swipeDirection.x = 0;
            swipeDirection.y = MaxilaryViewAngle;
            // LowerView(0);
        }

        public void MandibularView()
        {
            // LowerView(1);
            // rotX = MandibularViewAngle;
            rotX = -60;
            rotY = 0;

            swipeDirection.x = 0;
            swipeDirection.y = MandibularViewAngle;
            // UpperView(0);
        }

        public void UpperView(int aState){
            bool state = aState == 1;
            if(IPRObjectNameSpace.IPRObject.activeIPR){
                IPRObjectNameSpace.IPRObject.toggleUpperIpr(aState);
            }
            if(SuperImpose.isActive) {
                SuperImpose.toggleUpperSuperImposition(state);
            }
            foreach(GameObject upper in UpperObjects){
                upper.SetActive(state);
            }

            upperViewActive = state;
            // return;
            // if(!state){
            //     getUpperObjects();
            // }
            // foreach(GameObject upper in UpperObjects){
            //     upper.SetActive(state);
            // }
        }

        public void LowerView(int aState){
            bool state = aState == 1;
            if(IPRObjectNameSpace.IPRObject.activeIPR){
                IPRObjectNameSpace.IPRObject.toggleLowerIpr(aState);
            }
            if(SuperImpose.isActive){
                SuperImpose.toggleLowerSuperImposition(state);
            }
            foreach(GameObject lower in LowerObjects){
                lower.SetActive(state);
            }
            lowerViewActive = state;
            // if(!state){
            //     getLowerObjects();
            //     return;
            // }
            // foreach(GameObject lower in LowerObjects){
            //     lower.SetActive(state);
            // }
        }
    
        public void LeftView()
        {
            rotY = -90;
            rotX = 0;

            swipeDirection.y = 0;
            swipeDirection.x = -90;
        }
        public void RightView()
        {
            rotY = 90;
            rotX = 0;

            swipeDirection.y = 0;
            swipeDirection.x = 90;
        }
        
        public void FrontView()
        {
            rotX = 0;
            rotY = 0;

            swipeDirection.y = 0;
            swipeDirection.x = 0;
        }

        // ########################## End Pre set views
    
        private void ToggleAttachments(int aState){
            bool state = aState == 1;

            if(!state){
                foreach(GameObject upper in UpperObjects){
                    foreach(var obj in upper.GetComponentsInChildren<Transform>()){
                        if(obj.name.StartsWith("Attachment")){
                            obj.gameObject.SetActive(false);
                            Attachments.Add(obj.gameObject);
                        }
                    }
                }
                foreach(GameObject lower in LowerObjects){
                    foreach(var obj in lower.GetComponentsInChildren<Transform>()){
                        if(obj.name.StartsWith("Attachment")){
                            obj.gameObject.SetActive(false);
                            Attachments.Add(obj.gameObject);
                        }
                    }
                }
                return;
            }
            foreach(GameObject obj in Attachments){
                obj.SetActive(true);
            }
            Attachments = null;
            Attachments = new List<GameObject>();
            
        }

        private void resetAttachments() {
            Attachments = null;
            Attachments = new List<GameObject>();
        }

        private float calculateRotateFactor(float x) {
            float maxZoomMouseRotateFactor = 0.15f;
            float minZoomMouseRotateFactor = 3f;

            float maxZoomTouchRotateFactor = 0.02f;
            float minZoomTouchRotateFactor = 0.2f;

            float x1 = maxZoom;
            float x2 = minZoom;

            float y1 = isMobile ? maxZoomTouchRotateFactor : maxZoomMouseRotateFactor;
            float y2 = isMobile ? minZoomTouchRotateFactor : minZoomMouseRotateFactor;

            float y = y1 + (x-x1)*(y2-y1)/(x2-x1);

            return y;
        }

        private float calculatePanFactor(float x){
            // calculating and return panFactor Value for distanceBetweenCameraAndTarget

            float maxZoomPanFactor = 0.15f;
            float minZoomPanFactor = 1.5f;

            float maxZoomTouchPanFactor = 0.02f;
            float minZoomTouchPanFactor = 0.1f; 

            float x1 = maxZoom;
            float x2 = minZoom;

            // float y1 = isMobile ? 0.05f : 0.15f;
            // float y2 = isMobile ? 0.5f : 1.5f;

            float y1 = isMobile ? maxZoomTouchPanFactor : maxZoomPanFactor;
            float y2 = isMobile ? minZoomTouchPanFactor : minZoomPanFactor;

            float y = y1 + (x-x1)*(y2-y1)/(x2-x1);

            return y;
        }

        private void EditorCameraInput()
        {
            //Camera Rotation
            if (Input.GetMouseButton(0))
            {
                if(dragMode == DragModes.rotate){
                    rotX += Input.GetAxis("Mouse Y") * mouseRotateSpeed; // around X
                    rotY += Input.GetAxis("Mouse X") * mouseRotateSpeed;
                    
                    // if(!this.upperViewActive || !this.lowerViewActive) {
                    //     return;
                    // }
                    float minRotAngle = lowerViewActive && upperViewActive ? 
                            minXRotAngle : minXRotAngleOneJaw;
                    float maxRotAngle = lowerViewActive && upperViewActive ?
                            maxXRotAngle : maxXRotAngleOneJaw;

                    if (rotX < minRotAngle)
                    {
                        rotX = minRotAngle;
                    }
                    else if (rotX > maxRotAngle)
                    {
                        rotX = maxRotAngle;
                    }
                } else if(dragMode == DragModes.pan){
                    panX += Input.GetAxis("Mouse X") * panFactor;
                    panY += Input.GetAxis("Mouse Y") * panFactor;

                    
                    

                    // if(panX > maxPanX){
                    //     panX = maxPanX;
                    // }
                    // if(panX < minPanX) {
                    //     panX = minPanX;
                    // }
                    // if(panY > maxPanY) {
                    //     panY = maxPanY;
                    // }
                    // if(panY < minPanY) {
                    //     panY = minPanY;
                    // }

                }
            }
            //Camera Field Of View
            if (Input.mouseScrollDelta.magnitude > 0)
            {
                // cameraFieldOfView += Input.mouseScrollDelta.y * editorFOVSensitivity * -1;//-1 make FOV change natual
                // if(distanceBetweenCameraAndTarget - Input.mouseScrollDelta.y * 50 <= 20 || distanceBetweenCameraAndTarget - Input.mouseScrollDelta.y * 50 >= 500){
                //     return;
                // }
                
                
                
                if(Input.mouseScrollDelta.y > 0){
                    // if(distanceBetweenCameraAndTarget - zoomSensitivity < maxZoom){
                    //     float delta = distanceBetweenCameraAndTarget - zoomSensitivity;
                    //     sceneCamera.transform.position += sceneCamera.transform.forward * delta;
                    //     // sceneCamera.transform.position = new Vector3() + sceneCamera.transform.forward * maxZoom;

                    // }
                    // else{
                    //     sceneCamera.transform.position += sceneCamera.transform.forward * zoomSensitivity;
                    // }
                    zoomIn();
                }
                else{
                    // if(distanceBetweenCameraAndTarget + zoomSensitivity > minZoom){
                    //     float delta = minZoom - distanceBetweenCameraAndTarget;
                    //     sceneCamera.transform.position -= sceneCamera.transform.forward * delta;
                    //     // sceneCamera.transform.position = new Vector3() + sceneCamera.transform.forward * minZoom;
                    // }
                    // else{
                    //     sceneCamera.transform.position -= sceneCamera.transform.forward * zoomSensitivity;
                    // }

                    zoomOut();
                }

                // // creating temp vector which store camera position without any paning
                // Vector3 tempV = sceneCamera.transform.position + (sceneCamera.transform.right * panX) + (sceneCamera.transform.up * panY);
                // 
                // 
                // 
                // 
                // // distanceBetweenCameraAndTarget = Vector3.Distance(sceneCamera.transform.position, lookAtPt);
                
                // distanceBetweenCameraAndTarget = Vector3.Distance(tempV, lookAtPt);

                // //assign value to the distance between the maincamera and the target
                // dir = new Vector3(0, 0, distanceBetweenCameraAndTarget);
            }
        }
    
        private void TouchCameraInput()
        {
            if (Input.touchCount > 0)
            {
                if (Input.touchCount == 1)
                {
                    touch = Input.GetTouch(0);
                    #if UNITY_WEBGL && !UNITY_EDITOR
                    // getDeltaDistance(touch.phase.ToString());
                    // getPincMode(touch2.phase.ToString());
                    #endif
                    if(dragMode == DragModes.rotate){
                        if (touch.phase == TouchPhase.Began)
                        {
                            // user touuch the screen
        
                        }
                        else if (touch.phase == TouchPhase.Moved)  // the problem lies in we are still rotating object even if we move our finger toward another direction
                        {
                            swipeDirection += -touch.deltaPosition * touchRotateSpeed; //-1 make rotate direction natural
                        }
                        else if (touch.phase == TouchPhase.Ended)
                        {
                            // user released the screen
                        }
                    } else if(dragMode == DragModes.pan) {
                        if (touch.phase == TouchPhase.Began)
                        {
                            // user touuch the screen
        
                        }
                        else if (touch.phase == TouchPhase.Moved)  // the problem lies in we are still rotating object even if we move our finger toward another direction
                        {
                            panX -= touch.deltaPosition.x * panFactor; //-1 make rotate direction natural
                            panY -= touch.deltaPosition.y * panFactor;

                            
                            
                        }
                        else if (touch.phase == TouchPhase.Ended)
                        {
                            // user released the screen
                        }
                    }
                    
                }
                else if (Input.touchCount == 2)
                {
                    Touch touch1 = Input.GetTouch(0);
                    Touch touch2 = Input.GetTouch(1);

                    if (touch1.phase == TouchPhase.Began && touch2.phase == TouchPhase.Began)
                    {
    
                        touch1OldPos = touch1.position;
                        touch2OldPos = touch2.position;

                        initialTouch1Pos = touch1.position;
                        initialTouch2Pos = touch2.position;
                    }
                    if (touch1.phase == TouchPhase.Moved && touch2.phase == TouchPhase.Moved)
                    {
                        touch1CurrentPos = touch1.position;
                        touch2CurrentPos = touch2.position;
                        float deltaDistance = Vector2.Distance(touch1CurrentPos, touch2CurrentPos) - Vector2.Distance(touch1OldPos, touch2OldPos);
                        // #if UNITY_WEBGL && !UNITY_EDITOR
                        // getDeltaDistance(deltaDistance.ToString());
                        // #endif

                        // float deltaOffset = 100f;

                        // if(pinchMode == PinchModes.NA) {
                        //     // setting pinch mode
                        //     if(deltaDistance < 0 + deltaOffset && deltaDistance > 0 - deltaOffset) {
                        //         pinchMode = PinchModes.pan;
                        //     }
                        //     else {
                        //         pinchMode = PinchModes.zoom;
                        //     }
                        //     // #if UNITY_WEBGL && !UNITY_EDITOR
                        //     // getPincMode(touch1.phase.ToString());
                        //     // #endif
                        // }

                        // if(pinchMode == PinchModes.pan) {
                        //     panX -= touch1.deltaPosition.x * panFactor; //-1 make rotate direction natural
                        //     panY -= touch1.deltaPosition.y * panFactor;
                        // }
                        // else {
                        //     if(deltaDistance > 0){
                        //         zoomIn();
                        //     }
                        //     if(deltaDistance < 0){
                        //         zoomOut();
                        //     }
                        // }

                        //new strategy make zoom and pan together

                        float pinchOffset = 5f;


                        if(deltaDistance > 0 + pinchOffset){
                            zoomIn();
                        }
                        if(deltaDistance < 0 - pinchOffset){
                            zoomOut();
                        }

                        float avgPanX = (touch1.deltaPosition.x + touch2.deltaPosition.x) / 2; 
                        float avgPanY = (touch1.deltaPosition.y + touch2.deltaPosition.y) / 2; 

                        panX -= avgPanX * panFactor; //-1 make rotate direction natural
                        panY -= avgPanY * panFactor;

                        touch1OldPos = touch1CurrentPos;
                        touch2OldPos = touch2CurrentPos;
                        
                        
                        // cameraFieldOfView += deltaDistance * -1 * touchFOVSensitivity; // Make rotate direction natual
                        

                        // // creating temp vector which store camera position without any paning
                        // Vector3 tempV = sceneCamera.transform.position + (sceneCamera.transform.right * panX) + (sceneCamera.transform.up * panY);

                        // // distanceBetweenCameraAndTarget = Vector3.Distance(sceneCamera.transform.position, new Vector3(0, 0, 0));
                        // distanceBetweenCameraAndTarget = Vector3.Distance(tempV, lookAtPt);
                        // dir = new Vector3(0, 0, distanceBetweenCameraAndTarget);//assign value to the distance between the maincamera and the target
                    }
                    else if (touch1.phase == TouchPhase.Ended || touch2.phase == TouchPhase.Ended)
                    {
                        // user released the screen

                        // pinchMode = PinchModes.NA;
                        
                        // #if UNITY_WEBGL && !UNITY_EDITOR
                        // getPincMode(pinchMode.ToString());
                        // #endif
                    }
                }
    
            }

            // if(!this.lowerViewActive || !this.upperViewActive) return;
    
            float minRotAngle = upperViewActive && lowerViewActive ?
                    minXRotAngle : minXRotAngleOneJaw;
            float maxRotAngle = upperViewActive && lowerViewActive ?
                    maxXRotAngle : maxXRotAngleOneJaw;
                    
            if (swipeDirection.y < minRotAngle)
            {

                swipeDirection.y = minRotAngle;
                
            }
            else if (swipeDirection.y > maxRotAngle)
            {
                swipeDirection.y = maxRotAngle;
                
            }
    
    
        }
    
        private void RotateCamera()
        {
    
            // Vector3 tempV = new Vector3(rotX, rotY, 0);
            // targetRot = Quaternion.Euler(tempV); //We are setting the rotation around X, Y, Z axis respectively
            // if (Application.isEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            // {
            //     Vector3 tempV = new Vector3(rotX, rotY, 0);
            //     targetRot = Quaternion.Euler(tempV); //We are setting the rotation around X, Y, Z axis respectively
            // }
            // else
            // {
            //     targetRot = Quaternion.Euler(-swipeDirection.y, swipeDirection.x, 0);
            // }
            Vector3 tempV;
            
            if(isMobile){
                tempV = new Vector3(swipeDirection.y, swipeDirection.x, 0);
                // targetRot = Quaternion.Euler(swipeDirection.y, swipeDirection.x, 0);
            }
            else{
                tempV = new Vector3(rotX, rotY, 0);
            }
            targetRot = Quaternion.Euler(tempV); //We are setting the rotation around X, Y, Z axis respectively
            currentRot = Quaternion.Slerp(currentRot, targetRot, Time.smoothDeltaTime * slerpSmoothValue * 50);  //let cameraRot value gradually reach newQ which corresponds to our touch
            //Rotate Camera
            
            //Multiplying a quaternion by a Vector3 is essentially to apply the rotation to the Vector3
            //This case it's like rotate a stick the length of the distance between the camera and the target and then look at the target to rotate the camera.
            sceneCamera.transform.position = new Vector3(0, 0, 0) + currentRot * dir;
            
            sceneCamera.transform.LookAt(lookAtPt);

            // Panning ######

            if(panX > maxPanX){
                panX = maxPanX;
            }
            if(panX < minPanX) {
                panX = minPanX;
            }
            if(panY > maxPanY) {
                panY = maxPanY;
            }
            if(panY < minPanY) {
                panY = minPanY;
            }

            sceneCamera.transform.position -= sceneCamera.transform.up * panY;
            sceneCamera.transform.position -= sceneCamera.transform.right * panX;

            // 
            
        }

        void panCamera(){
            Vector3 tempVPan;
            if(isMobile){

            }
            else{
                // tempVPan = new Vector3(0, 90 + panY, 0); // Position for pan perpose
                
                // Quaternion Qpan = Quaternion.Euler(tempVPan);
                // Quaternion Qpan2;
                
                // Qpan2 = Quaternion.Slerp(currentRot, Qpan, Time.smoothDeltaTime * slerpSmoothValue * 50);
                // lookAtPt = new Vector3() + Qpan2 * new Vector3(0,0,100);

                // lookAtPt 

                sceneCamera.transform.LookAt(lookAtPt);
            }
        }
    
    
        void SetCameraFOV()
        {
            //Set Camera Field Of View
            //Clamp Camera FOV value
            if (cameraFieldOfView <= minCameraFieldOfView)
            {
                cameraFieldOfView = minCameraFieldOfView;
            }
            else if (cameraFieldOfView >= maxCameraFieldOfView)
            {
                cameraFieldOfView = maxCameraFieldOfView;
            }
            // cameraFieldOfView = cameraFieldOfView <= minCameraFieldOfView ? 
    
            cameraFOVDamp = Mathf.SmoothDamp(cameraFOVDamp, cameraFieldOfView, ref fovChangeVelocity, scrollSmoothTime);
            sceneCamera.fieldOfView = cameraFOVDamp;
    
        }

        public void setPlatformMode(string mode){
            isMobile = mode == "mobile" ? true : false;
            float min_zoom = isMobile ? minMobileZoom : minZoom;
            float fov = isMobile ? 20 : 10;
            sceneCamera.fieldOfView = fov;
            float delta = min_zoom - distanceBetweenCameraAndTarget;
            sceneCamera.transform.position -= sceneCamera.transform.forward * delta;
            distanceBetweenCameraAndTarget = Vector3.Distance(sceneCamera.transform.position, new Vector3(0, 0, 0));
            dir = new Vector3(0, 0, distanceBetweenCameraAndTarget);//assign value to the distance between the maincamera and the target
            // maxCameraFieldOfView = isMobile ? 20 : 10;
            // cameraFieldOfView = maxCameraFieldOfView;
            // SetCameraFOV();
        }

        public void setDarkTheme(int aState){
            
            Color color;
            bool state = aState == 1;
            string colorStr = state ? "#464646" : "#FFFFFF";
            if(ColorUtility.TryParseHtmlString(colorStr, out color)){
                backgroundMaterial.color = color;
                sceneCamera.backgroundColor = color;
            }
            background.GetComponent<Renderer>().material = backgroundMaterial;
            string lineColor = state ? "#FFFFFF" : "#000000";
            if(ColorUtility.TryParseHtmlString(lineColor, out color)){
                IPRObject.setLineColor(color);
                
            }

        }

        public void setDragMode(string aDragMode){
            // if(aDragMode == "rotate") {
            //     sceneCamera.transform.position += sceneCamera.transform.up * panY;
            //     sceneCamera.transform.position += sceneCamera.transform.right * panX;
            //     panX = 0;
            //     panY = 0;
            // }


            // switch (aDragMode)
            // {
            //     case "rotate":
            //         dragMode = DragModes.rotate;
            //         break;
            //     case "pan":
            //         dragMode = DragModes.pan;
            //         break;
            //     default:
            //         break;
            // }

            dragMode = aDragMode == "rotate" ? DragModes.rotate : DragModes.pan;
        }

        public static void setCameraClipingPlane() {
            // Adjusting Near and far plane of camera

            GameObject root = GameObject.Find("MeshRoot");
            
            Renderer[] childs = root.GetComponentsInChildren<Renderer>();
            System.Collections.IEnumerator childsEnumerator = childs.GetEnumerator();

            while(childsEnumerator.MoveNext()) {
                Renderer child = (Renderer)childsEnumerator.Current;
                bounds.Encapsulate(child.bounds);
            }
            // foreach (Renderer r in childs){
            // }

            float diagonalLength = Mathf.Sqrt(Mathf.Pow(bounds.size.x, 2) + Mathf.Pow(bounds.size.y, 2) + Mathf.Pow(bounds.size.z, 2));

            // float boundsMaxLength;
            
            // if(bounds.size.x > bounds.size.y) {
            //     if(bounds.size.z>bounds.size.x) {
            //         boundsMaxLength = bounds.size.z;
            //     }
            //     else {
            //         boundsMaxLength = bounds.size.x;
            //     }
            // }
            // else {
            //     boundsMaxLength = bounds.size.y;
            // }

            float offset = diagonalLength / 2;

            float nearPlaneDistance = distanceBetweenCameraAndTarget - offset > 0 ? 
                    distanceBetweenCameraAndTarget - offset : 0.1f;
            float farPlaneDistance = distanceBetweenCameraAndTarget + offset; 

            sceneCamera.nearClipPlane = nearPlaneDistance;
            sceneCamera.farClipPlane = farPlaneDistance;
        }

        public void zoomIn(){

            float sensitivity = isMobile ? mobileZoomSensitivity : zoomSensitivity;
            float maxZoomLimit = isMobile ? maxMobileZoom : maxZoom;

            if(distanceBetweenCameraAndTarget - sensitivity < maxZoom){
                float delta = distanceBetweenCameraAndTarget - sensitivity;
                sceneCamera.transform.position += sceneCamera.transform.forward * delta;
                // sceneCamera.transform.position = new Vector3() + sceneCamera.transform.forward * maxZoom;

            }
            else{
                sceneCamera.transform.position += sceneCamera.transform.forward * sensitivity;
            }

            // creating temp vector which store camera position without any paning
            Vector3 tempV = sceneCamera.transform.position + (sceneCamera.transform.right * panX) + (sceneCamera.transform.up * panY);

            // distanceBetweenCameraAndTarget = Vector3.Distance(sceneCamera.transform.position, new Vector3(0, 0, 0));
            distanceBetweenCameraAndTarget = Vector3.Distance(tempV, lookAtPt);
            
            setCameraClipingPlane();

            dir = new Vector3(0, 0, distanceBetweenCameraAndTarget);//assign value to the distance between the maincamera and the target
            
        }

        public void zoomOut() {
            float sensitivity = isMobile ? mobileZoomSensitivity : zoomSensitivity;
            float minZoomLimit = isMobile ? minMobileZoom : minZoom;

            if(distanceBetweenCameraAndTarget + sensitivity > minZoomLimit){
                float delta = minZoomLimit - distanceBetweenCameraAndTarget;
                sceneCamera.transform.position -= sceneCamera.transform.forward * delta;
                // sceneCamera.transform.position = new Vector3() + sceneCamera.transform.forward * minZoom;
            }
            else{
                sceneCamera.transform.position -= sceneCamera.transform.forward * sensitivity;
            }

            // creating temp vector which store camera position without any paning
            Vector3 tempV = sceneCamera.transform.position + (sceneCamera.transform.right * panX) + (sceneCamera.transform.up * panY);

            // distanceBetweenCameraAndTarget = Vector3.Distance(sceneCamera.transform.position, new Vector3(0, 0, 0));
            distanceBetweenCameraAndTarget = Vector3.Distance(tempV, lookAtPt);

            setCameraClipingPlane();

            dir = new Vector3(0, 0, distanceBetweenCameraAndTarget);//assign value to the distance between the maincamera and the target

            
        }

        // Temp ///////////////

        private void setMaxZoomTouchPanFactor(float factor){
            maxZoomTouchPanFactor = factor;
        }
        private void setMinZoomTouchPanFactor(float factor){
            minZoomTouchPanFactor = factor;
        }

        private void toggleBaseShadow(int aState) {
            bool state = aState == 1;
            background.SetActive(state);
        }

    }
    
}
