using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ImportOBJNameSpace;
using CameraScript;

namespace SuperImposeNameSpace {
    public class SuperImpose : MonoBehaviour
    {

        private static GameObject upper;
        private static GameObject lower;

        [SerializeField]
        public Material superImposMateral;

        public static bool isActive = false;


        // Start is called before the first frame update
        void Start()
        {
            #if !UNITY_EDITOR && UNITY_WEBGL
                WebGLInput.captureAllKeyboardInput = false;
            #endif
        }

        // Update is called once per frame
        void Update()
        {
            #if UNITY_EDITOR
            if(Input.GetKeyDown(KeyCode.S)){
                if(Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift)){
                    toggle(0);
                    return;
                }
                toggle(1);
            }
            #endif
        }

        bool meshLoaded() {
            // returns true if upper and lower obj are set
            return upper != null && lower != null;
        }

        void setMesh() {
            /** 
            * get mesh from Import OBJ class
            */


            upper = GameObject.Instantiate(ImportOBJ.getUpperFirstStage());
            lower = GameObject.Instantiate(ImportOBJ.getLowerFirstStage());

            upper.transform.SetParent(ImportOBJ.meshRoot.transform, false);
            lower.transform.SetParent(ImportOBJ.meshRoot.transform, false);

            

            upper.name = "SuperImposeUpper";
            lower.name = "SuperImposeLower";

            setMaterial();

        }

        void setMaterial() {
            if(!meshLoaded()) return;
            foreach (var mesh in upper.GetComponentsInChildren<Transform>()) {
                if(mesh.childCount > 0) continue;
                mesh.GetComponent<Renderer>().material = superImposMateral;
            }
            foreach (var mesh in lower.GetComponentsInChildren<Transform>()) {
                if(mesh.childCount > 0) continue;
                mesh.GetComponent<Renderer>().material = superImposMateral;
            }
        }

        public static void toggleUpperSuperImposition(bool aState) {
            upper.SetActive(aState);
            foreach(Transform t in upper.GetComponentsInChildren<Transform>()) {
                if(t.name.StartsWith("Maxillary")) {
                    t.gameObject.SetActive(false);    
                    break;
                }
            }

        }
        public static void toggleLowerSuperImposition(bool aState) {
            lower.SetActive(aState);
            foreach(Transform t in lower.GetComponentsInChildren<Transform>()) {
                if(t.name.StartsWith("Mandibular")) {
                    t.gameObject.SetActive(false);
                    break;
                }
            }
        }

        void toggle(int aState){
            /** 
            * toggle superimposition 

            */

            bool state = aState == 1;
            if(!meshLoaded()) {
                setMesh();
            }

            if (CameraMovement.upperViewActive){
                toggleUpperSuperImposition(state);
            }
            if(CameraMovement.lowerViewActive) {
                toggleLowerSuperImposition(state);
            }
            isActive = state;
        }

        void adjustSuperImposition(int aState) {
            /**
             * adjust superoimpose mesh to avoid z fighting
             */

            bool state = aState == 1;

            if(state) {
                upper.transform.localPosition = new Vector3(0,0,0.01f);
                lower.transform.localPosition = new Vector3(0,0,0.01f);

                upper.transform.localScale = new Vector3(1.01f, 1.01f, 1.01f);
                lower.transform.localScale = new Vector3(1.01f, 1.01f, 1.01f);
            }
            else {
                upper.transform.localPosition = new Vector3(0,0,0f);
                lower.transform.localPosition = new Vector3(0,0,0f);

                upper.transform.localScale = new Vector3(1f, 1f, 1f);
                lower.transform.localScale = new Vector3(1f, 1f, 1f);
            }
        }
    }
    
}
