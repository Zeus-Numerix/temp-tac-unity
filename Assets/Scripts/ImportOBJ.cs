using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;
using System.IO;
using System.Linq;
using System;
using UnityEngine.Networking;
using UnityEngine;
using CameraScript;
// using Dummiesman;

namespace ImportOBJNameSpace {

    public class ImportOBJ : MonoBehaviour
    {
        // JavaScript Callbacks Declaration
        #if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void loadSuccess(string aSubStep);
        [DllImport("__Internal")]
        private static extern void loadFailed(string aSubStep);
        [DllImport("__Internal")]
        private static extern void showSuccess(string aSubStep);
        [DllImport("__Internal")]
        private static extern void showFailed(string aSubStep);
        [DllImport("__Internal")]
        private static extern void hideSuccess(string aSubStep);
        [DllImport("__Internal")]
        private static extern void hideFailed(string aSubStep);
        [DllImport("__Internal")]
        private static extern void resetSuccess();
        [DllImport("__Internal")]
        private static extern void resetFailed(string aSubStep);
        #endif

        // Materials declaratioon
        [SerializeField] private Material mat_gum;
        [SerializeField] private Material mat_tooth;
        [SerializeField] private Material mat_attachment;

        [SerializeField] private Material mat_gum_1;
        [SerializeField] private Material mat_gum_2;
        [SerializeField] private Material mat_gum_3;

        private Material temp_mat_gum;

        public static Bounds bounds;

        public static Bounds teethBounds;

        public static Vector3 boundCenter = Vector3.zero;

        [SerializeField] 
        private Material SubAttachMat;

        [System.Serializable]
        public class Jaw{
            public string type;
            public string url;
        }

        [System.Serializable]
        public class SubStep {
            public int num;
            public string case_name;
            public string mesh_type;
            public Jaw upper;
            public Jaw lower;
            public bool loaded = false;
        }

        public class LoadedSubStep: IDisposable {
            // Class for loaded substep in scene
            public int num;
            public GameObject upper;
            public GameObject lower;

            // ~LoadedSubStep(){
            // }

            public void Dispose(){
                Destroy(this.upper);
                Destroy(this.lower);
                GC.Collect();
            }

            public void show() {
                this.upper.SetActive(true);
                this.lower.SetActive(true);
            }

            public void hide() {
                this.upper.SetActive(false);
                this.lower.SetActive(false);
            }
        }

        [System.Serializable]
        public class GumColor {
            public int r;
            public int g;
            public int b;
            public int a;
        }

        public GameObject upper_obj;
        public GameObject lower_obj;

        public static GameObject meshRoot;

        public static List<LoadedSubStep> LoadedSubStepList = new List<LoadedSubStep>();


        async void Start()
        {
            #if !UNITY_EDITOR && UNITY_WEBGL
                WebGLInput.captureAllKeyboardInput = false;
            #endif
            meshRoot = GameObject.Find("MeshRoot");
            meshRoot.transform.position = Vector3.zero;
            // meshRoot.transform.localRotation = Quaternion.Euler(9f, 0, 0);
            #if UNITY_EDITOR
            string s = File.ReadAllText("Assets/Scripts/temp.json");
            string superS = File.ReadAllText("Assets/Scripts/superImposeTemp.json");

            await loadSubStep(s);
            await loadSubStep(superS);

            SubStep ss = JsonUtility.FromJson<SubStep>(s);
            SubStep superSs = JsonUtility.FromJson<SubStep>(superS);
            
            ss.loaded = true;
            superSs.loaded = true;

            s = JsonUtility.ToJson(ss);
            superS = JsonUtility.ToJson(superSs);
            showSubStep(s);
            // showSubStep(superS);
            #endif
            // mat_gum = mat_gum_1;
            temp_mat_gum = new Material(mat_gum);


            // meshRoot = GameObject.Find("MeshRoot");
        }

        // Update is called once per frame
        async void Update()
        {
            #if UNITY_EDITOR
            // if(Input.GetKeyDown(KeyCode.Space)) {
            //     resetScene();
            // }
            if(Input.GetKeyDown(KeyCode.Tab)) {
                string s = File.ReadAllText("Assets/Scripts/temp.json");
                await loadSubStep(s);
                SubStep ss = JsonUtility.FromJson<SubStep>(s);
                ss.loaded = true;
                s = JsonUtility.ToJson(ss);
                showSubStep(s);
            }
            if(Input.GetKeyDown(KeyCode.Keypad1)){
                changeGumMat("gum_1");
            }
            if(Input.GetKeyDown(KeyCode.Keypad2)){
                changeGumMat("gum_2");
            }
            if(Input.GetKeyDown(KeyCode.Keypad3)){
                changeGumBaseColor("#ff0000");
            }
            if(Input.GetKeyDown(KeyCode.Keypad4)){
                changeGumSmoothness(0.9f);
            }
            if(Input.GetKeyDown(KeyCode.Keypad5)){
                resetGumMaterial();
            }
            #endif
        }


        public static GameObject getUpperFirstStage() {
        /**
         * getter function to get Upper Jaw mesh  used for SuperImposition
         * return: GemeObject 
         */

        if(LoadedSubStepList.Count == 0) return null;

        LoadedSubStep substep = LoadedSubStepList.FirstOrDefault(substep =>  1 == substep.num);

        return substep.upper;

        }

        public static GameObject getLowerFirstStage() {
            /**
         * getter function to get Lower Jaw mesh  used for SuperImposition
         * return: GemeObject 
         */

        if(LoadedSubStepList.Count == 0) return null;

        LoadedSubStep substep = LoadedSubStepList.FirstOrDefault(substep =>  1 == substep.num);

        return substep.lower;
        }

        public void loadMaterials(GameObject obj)
        {
            foreach (var mesh in obj.GetComponentsInChildren<Transform>())
            {
                if (mesh.name.StartsWith("Maxillary") || mesh.name.StartsWith("Mandibular"))
                {
                    mesh.GetComponent<Renderer>().material = mat_gum;
                }
                else if (mesh.name.StartsWith("Tooth"))
                {
                    mesh.GetComponent<Renderer>().material = mat_tooth;
                }
                else if (mesh.name.StartsWith("Attachment"))
                {
                    mesh.GetComponent<Renderer>().material = mat_attachment;
                }
            }
        }

        public LoadedSubStep getSubStep(int aNum) {
            return LoadedSubStepList.FirstOrDefault(substep => aNum == substep.num);
        }

        public void showSubStep(string aSubStep){
            SubStep ss = JsonUtility.FromJson<SubStep>(aSubStep);
            if(ss.loaded) {
                if(getSubStep(ss.num) != null){
                    LoadedSubStep substep = getSubStep(ss.num);
                    substep.show();
                    #if UNITY_WEBGL && !UNITY_EDITOR
                    showSuccess(JsonUtility.ToJson(ss));
                    #endif
                }
                    
            }
            else{
                #if UNITY_WEBGL && !UNITY_EDITOR
                showFailed(JsonUtility.ToJson(ss));
                #endif
            }
        }

        public void hideSubStep(string aSubStep) {
            SubStep ss = JsonUtility.FromJson<SubStep>(aSubStep);
            if(ss.loaded) {
                LoadedSubStep substep = getSubStep(ss.num);
                substep.hide();
                #if UNITY_WEBGL && !UNITY_EDITOR
                hideSuccess(JsonUtility.ToJson(ss));
                #endif
            }
            else {
                #if UNITY_WEBGL && !UNITY_EDITOR
                hideFailed(JsonUtility.ToJson(ss));
                #endif
            }
        }

        public void getBounds(GameObject upper, GameObject lower) {
            bounds = new Bounds(Vector3.zero, Vector3.zero);
            teethBounds = new Bounds(Vector3.zero, Vector3.zero);
            foreach (Renderer r in lower.GetComponentsInChildren<Renderer>()) {
                bounds.Encapsulate(r.bounds);
                if(r.name.StartsWith("Tooth")){
                    teethBounds.Encapsulate(r.bounds);
                }
            }
            foreach (Renderer r in upper.GetComponentsInChildren<Renderer>()){
                bounds.Encapsulate(r.bounds);
                if(r.name.StartsWith("Tooth")){
                    teethBounds.Encapsulate(r.bounds);
                }
            }
            // Gizmos.DrawWireCube( bounds.center, bounds.size );
        }

        public async Task loadSubStep(string aSubStep) {
            SubStep ss = JsonUtility.FromJson<SubStep>(aSubStep);

            // Step 1: Dowload Files
            string upper_url = ss.upper.url;
            string lower_url = ss.lower.url;

            using UnityWebRequest upper_www = UnityWebRequest.Get(upper_url);
            using UnityWebRequest lower_www = UnityWebRequest.Get(lower_url);

            var upper_xhr = upper_www.SendWebRequest();
            var lower_xhr = lower_www.SendWebRequest();

            while(!upper_xhr.isDone || !lower_xhr.isDone){
                await Task.Yield(); // wait till both requests get response
            }

            if (upper_www.result == UnityWebRequest.Result.Success && lower_www.result == UnityWebRequest.Result.Success){
                // Step 2: Loading OBJ to Unity Scene
                var upper_gltf = new GLTFast.GltfImport();
                var lower_gltf = new GLTFast.GltfImport();

                bool upper_success = await upper_gltf.LoadGltfBinary(
                    upper_www.downloadHandler.data,
                    new Uri(upper_url)
                );
                bool lower_success = await lower_gltf.LoadGltfBinary(
                    lower_www.downloadHandler.data,
                    new Uri(lower_url)
                );

                if(upper_success && lower_success){
                    upper_obj = new GameObject();
                    lower_obj = new GameObject();

                    await upper_gltf.InstantiateMainSceneAsync(upper_obj.transform);
                    await lower_gltf.InstantiateMainSceneAsync(lower_obj.transform);
                }

                upper_obj.name = "upper" + ss.num.ToString().PadLeft(2, '0');
                lower_obj.name = "lower" + ss.num.ToString().PadLeft(2, '0');

                upper_obj.tag = "Jaw";
                lower_obj.tag = "Jaw";

                // upper_obj.transform.localScale = ss.mesh_type == "obj" ? new Vector3(1,1,1) : new Vector3(1,1,1);
                // lower_obj.transform.localScale = ss.mesh_type == "obj" ? new Vector3(1,1,1) : new Vector3(1,1,1);


                // Loading Materials
                loadMaterials(upper_obj);
                loadMaterials(lower_obj);




                if(bounds.size == Vector3.zero) {
                    getBounds(upper_obj, lower_obj);
                    boundCenter = bounds.center;
                }
                // upper_obj.transform.position -= boundCenter;
                // lower_obj.transform.position -= boundCenter;
                

                // upper_obj.transform.Rotate(9, 0, 0, Space.World);
                // lower_obj.transform.Rotate(9, 0, 0, Space.World);
                // meshRoot.transform.Rotate(9, 0, 0, Space.World);
                
                meshRoot.transform.localPosition = Vector3.zero - boundCenter;

                

                upper_obj.transform.SetParent(meshRoot.transform, false);
                lower_obj.transform.SetParent(meshRoot.transform, false);

                upper_obj.transform.localPosition = Vector3.zero;
                lower_obj.transform.localPosition = Vector3.zero;

                //setting Camera clipimPanes
                CameraMovement.setCameraClipingPlane();

                // Initail Hide objects
                upper_obj.SetActive(false);
                lower_obj.SetActive(false);

                // Storing GameObject to Unity 
                LoadedSubStep substep = new LoadedSubStep();
                substep.num = ss.num;
                substep.upper = upper_obj;
                substep.lower = lower_obj;

                LoadedSubStepList.Add(substep);

                ss.loaded = true;
                #if UNITY_WEBGL && !UNITY_EDITOR
                loadSuccess(JsonUtility.ToJson(ss));
                #endif
            }
            else {
                #if UNITY_WEBGL && !UNITY_EDITOR
                loadFailed(JsonUtility.ToJson(ss));
                #endif
            }

        }

        private void updateAttachmentMat(string aAttachmentName) {

            GameObject[] objects = GameObject.FindObjectsOfType<GameObject>();
            System.Collections.IEnumerator objectsEnumerator = objects.GetEnumerator();
            try{
                while(objectsEnumerator.MoveNext()) {
                    GameObject obj = (GameObject)objectsEnumerator.Current;

                    if(obj.name.StartsWith(aAttachmentName)) {
                        Renderer mesh = obj.transform.GetComponent<Renderer>();
                        mesh.material = SubAttachMat;
                        break;
                    }
                }
            }
            finally {
                IDisposable disposable = objectsEnumerator as System.IDisposable;
                if (disposable != null) disposable.Dispose();
            }
            // GameObject attachmentObj = GameObject.Find(aAttachmentName);
            
        }

        public void changeGumMat(string mat) {
            switch (mat)
            {
                case "gum_1":
                    mat_gum = mat_gum_1;
                    break;
                case "gum_2":
                    mat_gum = mat_gum_2;
                    break;
                case "gum_3":
                    mat_gum = mat_gum_3;
                    break;
                default:
                    break;
            }
            foreach (var substep in LoadedSubStepList){
                loadMaterials(substep.upper);
                loadMaterials(substep.lower);
            }
        }

        public void changeGumBaseColor(string aColor){
            // GumColor gum_color = JsonUtility.FromJson<GumColor>(aColor);
            // Color color = new Color(gum_color.r, gum_color.g, gum_color.b, gum_color.a);
            Color color;
            mat_gum = temp_mat_gum;
            if(ColorUtility.TryParseHtmlString(aColor, out color)){
                mat_gum.color = color;
                foreach (var substep in LoadedSubStepList){
                    loadMaterials(substep.upper);
                    loadMaterials(substep.lower);
                }
            } 
        }

        public void changeGumSmoothness(float smoothness){
            mat_gum = temp_mat_gum;
            mat_gum.SetFloat("_Smoothness", smoothness);
            foreach (var substep in LoadedSubStepList){
                loadMaterials(substep.upper);
                loadMaterials(substep.lower);
            }
        }

        public void resetGumMaterial() {
            mat_gum = mat_gum_2;
            temp_mat_gum = new Material(mat_gum);
            foreach (var substep in LoadedSubStepList){
                loadMaterials(substep.upper);
                loadMaterials(substep.lower);
            }
        }

        public void resetScene() {
            try{
                foreach (var substep in LoadedSubStepList)
                {
                    // Destroy(substep.upper);
                    // Destroy(substep.lower);
                    substep.Dispose();
                }
                // for(int i = 0; i< LoadedSubStepList.Count; i++){
                //     // Dispose(LoadedSubStepList[i]);
                // }
                LoadedSubStepList.Clear();
                // LoadedSubStepList = null;
                // LoadedSubStepList = new List<LoadedSubStep>();
                LoadedSubStepList.Clear();
                bounds.size = Vector3.zero;
                bounds.center = Vector3.zero;
                GC.Collect();
                #if UNITY_WEBGL && !UNITY_EDITOR
                resetSuccess();
                #endif
            }
            catch (Exception e){
                #if UNITY_WEBGL && !UNITY_EDITOR
                resetFailed(e.Message);
                #endif
            }
        }

    }
}
