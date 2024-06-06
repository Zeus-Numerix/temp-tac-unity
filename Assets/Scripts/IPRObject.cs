using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using TMPro;
using UnityEngine;
using CameraScript;
using Ipr;
using ImportOBJNameSpace;

namespace IPRObjectNameSpace {
    public class IPRObject : MonoBehaviour
{

    #if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void iprCreateSuccess();
    #endif

    // static members declarations
    [SerializeField]
    private Sprite billboardImage;

    [SerializeField]
    private Color billboardColor = Color.white;

    [SerializeField]
    private float billboardWidth = 15f;
    [SerializeField]
    private float billboardHeight = 10f;

    [SerializeField][Range(0f, 1f)]
    [Tooltip("Width of line")]
    private float lineWidth = 2f;

    [SerializeField][Range(0f, 50f)]
    [Tooltip("Length of line")]
    private float lineLength = 30f;

    [SerializeField][Range(0f, 50f)]
    private float hBoundryBuffer = 0.0f;
    [SerializeField][Range(0f, 50f)]
    private float vBoundryBuffer = 0.0f;

    private float adjustLineLength;

    [SerializeField][Range(0f, 50f)]
    [Tooltip("Height of line from line start")]
    private float lineHeight = 15f;

    [SerializeField]
    private static Color lineColor = Color.white;

    private Camera camera;

    private Plane[] cameraBoundryPlanes; // planes for declring the boundry of viewport

    // private List<GameObject> billboards = new List<GameObject>();

    // private Dictionary<string, GameObject> billboards = new Dictionary<string, GameObject>();

    // private List<GameObject> line = new List<GameObject>();

    public static List<IPR> upperIprs = new List<IPR>();
    public static List<IPR> lowerIprs = new List<IPR>();

    public static bool activeIPR = false;

    private GameObject upper;
    private GameObject lower;
    // private Shader lineShader = ;
    private Material mat;

    private Ray ray;

    [System.Serializable]
    public class IprData {
        public string name;
        public string value;
        public string maxValue;
        public string maxStage;
        public Vector3 startPos;
        public Vector3 direction;
        public string jawType;
        public string billBoardColor = "#ffffff";
    }


    // Start is called before the first frame update
    void Awake() {
        mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        ray = new Ray(Vector3.zero ,Vector3.forward);
        cameraBoundryPlanes = new Plane[] {
            new Plane(Vector3.zero, 0.0f),
            new Plane(Vector3.zero, 0.0f),
            new Plane(Vector3.zero, 0.0f),
            new Plane(Vector3.zero, 0.0f)
        };
        // cameraBoundryPlanes[0] = new Plane(Vector3.zero, 0.0f);
        // cameraBoundryPlanes[1] = new Plane(Vector3.zero, 0.0f);
        // cameraBoundryPlanes[2] = new Plane(Vector3.zero, 0.0f);
        // cameraBoundryPlanes[3] = new Plane(Vector3.zero, 0.0f);
    }
    void Start()
    {
        #if !UNITY_EDITOR && UNITY_WEBGL
            WebGLInput.captureAllKeyboardInput = false;
        #endif
        // line = DrawLine(new Vector3(0, 0, 50), new Vector3(0,50,0));
        // createBillboard(new Vector3(0,30,50), "12");
        adjustLineLength = lineLength;
        camera = Camera.main;
        // Vector3 zero = new Vector3(1,0,0);
        // Vector3 one = new Vector3(0,0,1);


    }

    // Update is called once per frame
    void Update()
    {
        #if UNITY_EDITOR
        if(Input.GetKeyDown(KeyCode.Alpha9)){
            // if(Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift)){
            //     return;
            // }

                IprData iprData = new IprData();
                iprData.name = "ipr-Tooth_7-Tooth_8";
                iprData.value = "0.1";
                iprData.maxValue = "0.2";
                iprData.maxStage = "5";
                iprData.startPos = new Vector3(0.03433806635439396f, -9.733338296413422f, 21.596144676208496f);
                iprData.direction = new Vector3(0.02105629610202234f, 0.07004748915753142f, 0.9973214033886917f);
                iprData.jawType = "upper";

                string ss = JsonUtility.ToJson(iprData);
                createIPR(ss);

                IprData iprData2 = new IprData();
                iprData2.name = "ipr-Tooth_23-Tooth_24";
                iprData2.value = "0.1";
                iprData2.maxValue = "0.2";
                iprData2.maxStage = "5";
                iprData2.startPos = new Vector3(4.756975889205933f, -12.303279399871826f,
                        15.972066879272461f);
                iprData2.direction = new Vector3( 0.13683115605442406f, 0.06297676946649745f,
                        0.988590492185907f);
                iprData2.jawType = "lower";

                string ss2 = JsonUtility.ToJson(iprData2);
                
                createIPR(ss2);
        }

        if(Input.GetKeyDown(KeyCode.Alpha7)) {
            IprData iprData = new IprData();
            iprData.name = "tempIpr";
            iprData.value = "0.1";
            iprData.maxValue = "0.2";
            iprData.maxStage = "1";
            iprData.startPos = new Vector3(5.366544961929321f, 0.9143915474414825f, 5.734277963638306f);
            iprData.direction = new Vector3(-0.006075531720376569f, -0.10470591017796171f, 0.9944846707155015f);
            iprData.jawType = "lower";
            string ss = JsonUtility.ToJson(iprData);
            
            updateStartPos(ss);
        }

        if(Input.GetKeyDown(KeyCode.Alpha8)) {
            if (Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift)) {
                toggleIpr(0);
                return;
            }
            toggleIpr(1);
        }

        if(Input.GetKeyDown(KeyCode.X)) {
            // isInsideView(new Vector3(0,0,50));
            IPR ipr = lowerIprs.First();

            updateIprLineLength(ipr, 15);
        }

        #endif
    }

    private void LateUpdate() {
        // setUpper();
        // setLower();
        updateBillboard();
        // updateViewBasedVisibility();
        // setCanvsPosition();
        // updateLine();
        // transform.LookAt(Camera.main.transform.position);

    }

    public void OnDrawGizmos() {
        Gizmos.color = Color.green;
        // Gizmos.DrawWireCube(ImportOBJ.teethBounds.center, ImportOBJ.teethBounds.size);
        Gizmos.DrawRay(ray);
        
    }

    /// convert and return world position coordinates of given point
    /// 
    /// @param[in] point
    ///     a point to be converted
    /// 
    /// @return converted point coordinate
    private Vector3 convertToWorldPosition(Vector3 point) {
        GameObject meshRoot = GameObject.Find("MeshRoot");

        Vector3  worldPosition = meshRoot.transform.TransformPoint(point);

        return worldPosition;
    }

    /// update the line length of ipr
    /// 
    /// @param[in] ipr
    ///     IPR object which length to be updated
    /// 
    /// @param[in] offset
    ///     value added to the length; a negative offset indicates a
    ///     decrease in length, while a positive offset results in an 
    ///     increase in length.
    /// 
    public void updateIprLineLength(IPR ipr, float offset) {
        Vector3 endPos = ipr.getInitialEndPos();
        // Vector3 endPos = ipr.getEndPos();
        Vector3 startPos = ipr.getStartPos();
        Vector3 direction = ipr.getDirection();
        string jawType = ipr.getJawType();

        float currentLen = Vector3.Distance(endPos, startPos);

        // calculate new length by adding offset to current length .
        // new length is should be between 5 to default length.
        float newLen = Mathf.Max(
            5.0f, Mathf.Min(currentLen + offset, ipr.getDefaultLineLength())); 

        float theta = (
                Mathf.Asin((startPos.y - endPos.y) / currentLen));

        float yLen = (Mathf.Sin(Mathf.Abs(theta)) * newLen);

        float newY = jawType == "upper" ? startPos.y + yLen : startPos.y - yLen;

        Vector3 newEndPos = startPos + direction * newLen;

        newEndPos.y = newY;

        ipr.updateIprPos(startPos, newEndPos, direction);
        
    }

    /// adjust the ipr lines such that ipr always inside viewport
    /// @param[in] ipr 
    ///     IPR object to be checked.
    /// @return 
    ///     A boolean value specified ipr is in the view or not.
    private void keepIprInside(IPR ipr){
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        Vector3 worldPosition = convertToWorldPosition( ipr.getInitialEndPos() );
        
        float enter = 0.0f;

        ray.origin = convertToWorldPosition(ipr.getStartPos());
        ray.direction = (ipr.getEndPos() - ipr.getStartPos()).normalized;

        float distance = float.NaN;
        float offset = 0;
        int hitPlane = 0;
        Vector3 hitPoint = Vector3.zero;
        int[] verticalPlanes = {2, 3};
        for ( int i=0; i <= 3; i++ ) {
            Plane plane = cameraBoundryPlanes[i];

            // setting plane normal i distance relative to camera frustum
            plane.normal = planes[i].normal;
            plane.distance = planes[i].distance - 
                    (verticalPlanes.Contains(i) ? 
                    vBoundryBuffer : hBoundryBuffer);

            // if ipr ray intersects with current plane
            if( plane.Raycast(ray, out enter) ) {
                if(distance.Equals(float.NaN)) {
                    distance = plane.GetDistanceToPoint( worldPosition );
                    hitPoint = ray.GetPoint( enter );
                    offset = Vector3.Distance(hitPoint, worldPosition);
                    hitPlane = i;
                } 

                if(distance < plane.GetDistanceToPoint( worldPosition )) continue;
                
                distance = Mathf.Min(
                    distance, plane.GetDistanceToPoint( worldPosition ));
                hitPoint = ray.GetPoint( enter );
                offset = Vector3.Distance(hitPoint, worldPosition);
                hitPlane = i;
                
            }
        }

        if( 0 > distance ) { 
            // if ipr get otside of plane distance get negetive.

            updateIprLineLength(ipr, offset * -1);
        } else {
            // handles if plane get outside of initial end position.
            float incrementOffset = Vector3.Distance(ipr.getEndPos(), ipr.getInitialEndPos());

            updateIprLineLength(ipr, incrementOffset);
        }
    }

    private void setCanvsPosition(){
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Jaw");
        if(objects.Length == 0) return;
        transform.position = objects[0].transform.position;
    }


    private void updateBillboard( ) {

        if(upperIprs  == null) return;

        if(lowerIprs  == null) return;

        if(upperIprs.Count <= 0) return;

        if(lowerIprs.Count <= 0) return;
        
        /* 
         In C# (Mono compiler) foreach statement allocates memmory,
         that cause memmory leaks in webgl build, because of wasm not releasing memmory,
         sp trying new method to iterate iprs 
        */
        List<IPR>.Enumerator upperEnumerator = upperIprs.GetEnumerator();
        List<IPR>.Enumerator lowerEnumerator = lowerIprs.GetEnumerator();
        try {
            while(upperEnumerator.MoveNext()){
                IPR ipr = upperEnumerator.Current;

                if(!ipr.iprIsActive()) {
                    continue;
                }
                ipr.lookAt(Camera.main.transform.position, Camera.main.transform.up);

                GameObject line = ipr.getLine();

                ipr.updatLine(lineColor, lineWidth);
                // // Angle Approch
                Vector3 forward = camera.transform.forward;
                // Vector3 endToStart = endPos - startPos;
                Vector3 direction = ipr.getDirection();

                float angle = Vector3.Angle(forward, direction);
                if(angle > 90) {
                    ipr.toggle(true);
                }
                else {
                    ipr.toggle(false);
                }

                keepIprInside(ipr);


                // float endY = ipr.getEndPos().y; 
                // Vector3 endPos;
                // if( !isInsideView( ipr ) ){
                //     Debug.LogFormat("outside ipr: {0}, ", ipr.name);
                //     float temporaryLineLength = Vector3.Distance( 
                //         ipr.getStartPos(), 
                //         ipr.getEndPos() 
                //     ) - 10;
                //     endPos = ipr.getStartPos() + temporaryLineLength * 
                //         ipr.getDirection();
                //     ipr.updateIprPos( ipr.getStartPos(), endPos, ipr.getDirection() );
                // } else {
                //     endPos = ipr.getStartPos() + lineLength * ipr.getDirection();
                // }
                
                // endPos.y = endY;
                // ipr.updateIprPos( ipr.getStartPos(), endPos, ipr.getDirection() );

            }
        }
        finally {
            IDisposable disposable = upperEnumerator as System.IDisposable;
            if (disposable != null) disposable.Dispose();
        }

        try {
            while(lowerEnumerator.MoveNext()){
                IPR ipr = lowerEnumerator.Current;

                if(!ipr.iprIsActive()) {
                    continue;
                }
                ipr.lookAt(Camera.main.transform.position, Camera.main.transform.up);

                ipr.updatLine(lineColor, lineWidth);

                // Angle Approch

                // Vector3 cameraPos = camera.transform.position;
                // Vector3 startPos = ipr.getStartPos();
                // Vector3 endPos = ipr.getEndPos();
                // Vector3 startVect = new Vector3(startPos.x, cameraPos.y, startPos.z);
                // Vector3 endVect = new Vector3(endPos.x, cameraPos.y, endPos.z);

                // Vector3 camToStart = cameraPos - startPos;
                Vector3 forward = camera.transform.forward;
                // Vector3 endToStart = endPos - startPos;
                Vector3 direction = ipr.getDirection();

                // ipr.setBillbordImage(billboardImage, billboardWidth, billboardHeight, billboardColor);

                float angle = Vector3.Angle(forward, direction);
                if(angle > 90) {
                    ipr.toggle(true);
                }
                else {
                    ipr.toggle(false);
                }

                keepIprInside(ipr);

            }
        }
        finally {
            IDisposable disposable = lowerEnumerator as System.IDisposable;
            if (disposable != null) disposable.Dispose();
        }
    }   

    public static void setLineColor(Color aColor) {
        lineColor = aColor;
    }

    private void updateLine(){
        // if(upper == null || lower == null) return;
        if(upperIprs  == null) return;
        if(lowerIprs  == null) return;
        if(upperIprs.Count <= 0) return;
        if(lowerIprs.Count <= 0) return;
        foreach(IPR ipr in upperIprs){
            ipr.updatLine(lineColor, lineWidth);
        }
        foreach(IPR ipr in lowerIprs){
            ipr.updatLine(lineColor, lineWidth);
        }
        // mat.color = lineColor;
        // LineRenderer lr = line.GetComponent<LineRenderer>();
        // lr.startWidth = lineWidth;
        
    }

    private void updateStartPos(string aIprData){
        IprData iprData = JsonUtility.FromJson<IprData>(aIprData);
        Bounds bounds = ImportOBJ.teethBounds;
        
        Vector3 startPos = iprData.startPos;

        float lineOffset = iprData.jawType == "upper" ? lineHeight : lineHeight * -1;

        float endUpperY = bounds.center.y + (bounds.size.y * 0.5f) + 4f;
        float endLowerY = bounds.center.y - (bounds.size.y * 0.5f);

        float endY = iprData.jawType == "upper" ? endUpperY : endLowerY;
        
        Vector3 endPos = (startPos +  lineLength * iprData.direction);
        
        endPos.y = endY;
        // Debug.Log("Updating.....");
        // Debug.LogFormat("endY: {0}, endLowerY: {1}, endUpperY: {2}, bound size: {3}, bound center: {4}", endY, endLowerY, endUpperY, bounds.size, bounds.center);

        IPR ipr = null;

        if(iprData.jawType == "lower") {
            ipr = lowerIprs.FirstOrDefault(iprObj => iprData.name == iprObj.name);
            // ipr.updateIprPos(startPos, endPos, iprData.direction);
            // ipr.updateValue(iprData.value);
        }
        else if(iprData.jawType == "upper") {
            ipr = upperIprs.FirstOrDefault(iprObj => iprData.name == iprObj.name);
            // ipr.updateIprPos(startPos, endPos, iprData.direction);
            // ipr.updateValue(iprData.value);
        }
        ipr.updateIprPos(startPos, endPos, iprData.direction);
        ipr.updateValue(iprData.value);
        
        Color color;
        if(ColorUtility.TryParseHtmlString(iprData.billBoardColor, out color)){
            ipr.setBillbordImage(billboardImage, billboardWidth, billboardHeight, color);
        }
        
    }

    
    // public void setBillboardColor(string aIprData) {
    //     IprData iprData = JsonUtility.FromJson<IprData>(aIprData);

    //     IPR ipr = null;

    //     if(iprData.jawType == "lower") {
    //         ipr = lowerIprs.FirstOrDefault(iprObj => iprData.name == iprObj.name);
    //     }
    //     else if(iprData.jawType == "upper") {
    //         ipr = upperIprs.FirstOrDefault(iprObj => iprData.name == iprObj.name);
    //     }


        

        
    // }

    private void setUpper() {
        if(CameraScript.CameraMovement.UpperObjects.Count <= 0) return;
        upper = CameraScript.CameraMovement.UpperObjects[0];
    }

    private void setLower() {
        if(CameraScript.CameraMovement.LowerObjects.Count <= 0) return;
        lower = CameraScript.CameraMovement.LowerObjects[0];
    }

    // private Vector3 calculateStartPoint(Vector3 aStartPoint) {
    //     // calculte adjusted start point

    //     Bounds bounds = ImportOBJNameSpace.ImportOBJ.bounds;
    //     Vector3 boundCenter = ImportOBJNameSpace.ImportOBJ.boundCenter;


    //     float edgeZ = (bounds.size.z / 2);


    //     float yOffset = (float)Math.Tan(9) * edgeZ;

    //     Vector3 startPos = aStartPoint - boundCenter;

    //     // startPos.y = startPos.y - yOffset;

    //     return startPos;
        
    // }

    // private 

    private void createIPR(string aIprData) {
        IprData iprData = JsonUtility.FromJson<IprData>(aIprData);

        Bounds bounds = ImportOBJ.teethBounds;

        Vector3 startPos = iprData.startPos;
    
        // Debug.LogFormat("bound center: {0}", ImportOBJ.boundCenter);

        float lineOffset = iprData.jawType == "upper" ? lineHeight : lineHeight * -1;
        Vector3 endPos = (startPos +  lineLength * iprData.direction);

        float endUpperY = bounds.center.y + (bounds.size.y * 0.5f) + 4f;
        float endLowerY = bounds.center.y - (bounds.size.y * 0.5f);

        float endY = iprData.jawType == "upper" ? endUpperY : endLowerY;


        // endPos.y = lineOffset;
        // endPos.y += lineOffset;
        endPos.y = endY;


        Debug.LogFormat("endY: {0}, endLowerY: {1}, endUpperY: {2}, bound size: {3}, bound center: {4}", endY, endLowerY, endUpperY, bounds.size, bounds.center);
        // Bounds bounds = ImportOBJNameSpace.ImportOBJ.bounds;
        // endPos.y = iprData.jawType == "upper" ? bounds.size.y /2 : bounds.size.y /2 * -1;
        IPR ipr = new IPR(iprData.name, iprData.value, iprData.maxValue, iprData.maxStage, startPos , endPos, iprData.direction, iprData.jawType, transform);
        Color color;
        if(ColorUtility.TryParseHtmlString(iprData.billBoardColor, out color)){
            ipr.setBillbordImage(billboardImage, billboardWidth, billboardHeight, color);
        }
        // ipr.updatLine(lineColor, lineWidth);
        // ipr.toggle(true);
        if(iprData.jawType == "upper") {
            upperIprs.Add(ipr);
        }
        else if(iprData.jawType == "lower") {
            lowerIprs.Add(ipr);
        }

        #if UNITY_WEBGL && !UNITY_EDITOR
            iprCreateSuccess();
        #endif
    }

        
    // private List<Transform> getAllChilds(Transform t) {
    //     List<Transform> childs = new List<Transform>();
    //     int childCount = t.childCount;
    //     for(int i = 0; i< childCount; i++ ){
    //         childs.Add(t.GetChild(i));
    //     }
    //     return childs;
    // }

    private void destroyBillboards() {
        if(upperIprs  == null) return;
        if(lowerIprs  == null) return;
        if(upperIprs.Count <= 0) return;
        if(lowerIprs.Count <= 0) return;
        // foreach(string key in billboards.Keys){
        //     Destroy(billboards[key]);
        // }
        // billboards.Clear();
        // billboards = null;
        // billboards = new Dictionary<string, GameObject>();
        foreach(IPR ipr in upperIprs) {
            ipr.Dispose();
        }
        foreach(IPR ipr in lowerIprs) {
            ipr.Dispose();
        }
        upperIprs = null;
        lowerIprs = null;
        upperIprs = new List<IPR>();
        lowerIprs = new List<IPR>();

        upperIprs.Clear();
        lowerIprs.Clear();
        GC.Collect();

    }

    public static void toggleUpperIpr(int aState) {
        bool state = aState == 1;
        
        // foreach(var billboardKeys in billboards.Keys){
        //     billboards[billboardKeys].SetActive(state);
        // }
        foreach(IPR ipr in upperIprs) {
            ipr.toggle(state);
            ipr.setIprIsActive(state);
        }
    }

    public static void toggleLowerIpr(int aState) {
        bool state = aState == 1;
        if(lowerIprs.Count <= 0) return;
        // foreach(var billboardKeys in billboards.Keys){
        //     billboards[billboardKeys].SetActive(state);
        // }
        foreach(IPR ipr in lowerIprs) {
            ipr.toggle(state);
            ipr.setIprIsActive(state);
        }
    }

    private void toggleIpr(int aState) {
        activeIPR = aState == 1;
        
        if(CameraScript.CameraMovement.upperViewActive){
            toggleUpperIpr(aState);
        }
        else {
            toggleUpperIpr(0);
        }
        if(CameraScript.CameraMovement.lowerViewActive){
            toggleLowerIpr(aState);
        }
        else {
            toggleLowerIpr(0);
        }
    }
}

}

