using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace Ipr {
    public class IPR: IDisposable {
        private GameObject billboard;
        private GameObject line;
        private GameObject iprInfoPopupObj;

        private GameObject billboardTextObj;
        private GameObject billboardTextObj2;
        public string name;

        private string value;

        private string maxValue;

        private string maxStage;

        private Vector3 startPosition;

        private Vector3 endPosition;

        private Vector3 initialEndPos;

        private Vector3 direction;

        private Transform parentNode;

        private Material mat;

        private GameObject meshRoot;

        private bool isActive;

        private string jawType;
        private float defaultLineLength;

        // Constructor: call one creating object of IPR calss
        public IPR(string aName, string aValue, string aMaxValue, string aMaxStage, Vector3 aStartPos, Vector3 aEndPos, Vector3 aDirection, string aJawType, Transform aParent) {
            name = aName;
            value = aValue;
            maxValue = aMaxValue;
            maxStage = aMaxStage;
            startPosition = aStartPos;
            endPosition = aEndPos;
            initialEndPos = aEndPos;
            direction = aDirection;
            jawType = aJawType;
            parentNode = aParent;
            billboard = new GameObject(name);
            line = new GameObject("Line");
            meshRoot = GameObject.Find("MeshRoot");
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            defaultLineLength = Vector3.Distance(startPosition, endPosition);
            createBillboard();
        }

        public void setBillbordImage(Sprite aImage, float aWidth, float aHeight, Color aColor){
            Image image = billboard.GetComponentsInChildren<Image>()[0];
            TextMeshPro textComp = billboardTextObj.GetComponent<TextMeshPro>();
            
            image.rectTransform.sizeDelta = new Vector2(aWidth, aHeight);
            image.color = aColor;
            image.sprite = aImage;

            textComp.rectTransform.sizeDelta = new Vector2(aWidth - 1.5f, aHeight - 1f);
            textComp.alignment = TextAlignmentOptions.Center;

            string hexColor = "#" + ColorUtility.ToHtmlStringRGB(aColor);

            Color textColor = hexColor.Equals("#ffffff", StringComparison.OrdinalIgnoreCase) ? Color.black : Color.white;
            textComp.color = textColor;

            if("0" != this.maxStage) {
                BoxCollider collider = billboard.GetComponent<BoxCollider>();
                collider.size = new Vector3(aWidth, aHeight, 0);
            }
        }

        public void updatLine(Color aColor, float aWidth){
            LineRenderer lr = line.GetComponent<LineRenderer>();
            
            mat.color = aColor;

            lr.material = mat;
            lr.startWidth = aWidth;
        }

        

        public void updateIprPos(Vector3 aStartPos, Vector3 aEndPos, Vector3 aDirecction){
            startPosition = aStartPos;
            endPosition = aEndPos;
            // direction = aDirecction;

            LineRenderer lr = line.GetComponent<LineRenderer>();
            RectTransform canvasRectTranform = billboard.GetComponent<RectTransform>();
            Vector3[] pointPositions = {startPosition, aEndPos};
            lr.SetPositions(pointPositions);
            canvasRectTranform.anchoredPosition3D = aEndPos;
            direction = aDirecction;
        }

        public void updateValue(string aValue) {
            this.value = aValue;
            if(this.value == "0") {
                this.billboard.SetActive(false);
                this.line.SetActive(false);
                return;
            }
            if("0" != this.maxStage) {
                return;
            }
            TextMeshPro textComp = billboard.GetComponentsInChildren<TextMeshPro>()[0];

            string maxStageValue = maxStage.Length <= 1 ? " " + maxStage : maxStage;
            textComp.text = value + " \\ " + "<b>" + maxStageValue + "</b>";
        }

        public void lookAt(Vector3 aLookAtPoint, Vector3 upVector){
            // ipr lookat at aLookAtPoint
            RectTransform canvasRectTranform = billboard.GetComponent<RectTransform>();
            canvasRectTranform.transform.LookAt(aLookAtPoint, upVector);
            // canvasRectTranform.transform.up = upVector;
        }
        
        // Method to crate billboad gameobject
        private void createBillboard() {
            //declaring Gameobjects
            // billboard = new GameObject(name);
            billboard.transform.parent = meshRoot.transform; 
            // billboard.transform.localPosition = endPosition;
            billboard.transform.localRotation = Quaternion.Euler(0, 0, 0);

            GameObject billboardImageObj = new GameObject("billboard image");
            billboardImageObj.transform.parent = billboard.transform;
            billboardImageObj.transform.localRotation = Quaternion.Euler(0, 0, 0);
            // billboardImageObj.transform.localPosition = endPosition;

            billboardTextObj = new GameObject("billboard text");
            billboardTextObj.transform.parent = billboard.transform;
            billboardTextObj.transform.localRotation = Quaternion.Euler(0, 180, 0);

            // billboardTextObj2 = new GameObject("billboard text 2");
            // billboardTextObj2.transform.parent = billboard.transform;
            // billboardTextObj2.transform.localRotation = Quaternion.Euler(0, 180, 0);

            // adding components to gameobject
            billboard.AddComponent<Canvas>();
            billboardImageObj.AddComponent<Image>();
            billboardTextObj.AddComponent<TextMeshPro>();
            // billboardTextObj2.AddComponent<TextMeshPro>();

            if("0" != maxStage) {
                BoxCollider collider = billboard.AddComponent<BoxCollider>();
                collider.size = new Vector3(5, 3, 0);
                billboard.AddComponent<IprHoverHandler>();
            }
            
            // canvas coonfiguration
            Canvas canvas = billboard.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;

            // Canvas Rect config
            RectTransform canvasRectTranform = billboard.GetComponent<RectTransform>();
            canvasRectTranform.anchoredPosition3D = endPosition;
            canvasRectTranform.SetParent(billboard.transform);
            canvasRectTranform.ForceUpdateRectTransforms();
            canvasRectTranform.sizeDelta = new Vector2(0, 0);

            // Image Config
            Image image = billboardImageObj.GetComponent<Image>();
            RectTransform imageRectTransform = billboardImageObj.GetComponent<RectTransform>();
            // imageRectTransform.anchoredPosition3D = Vector3.zero;


            // Text Config
            TextMeshPro textComp = billboardTextObj.GetComponent<TextMeshPro>();
            string maxStageValue = maxStage.Length <= 1 ? " " + maxStage : maxStage;
            textComp.text = value + " \\ " + "<b>" + maxStageValue + "</b>";
            textComp.enableAutoSizing = true;
            textComp.rectTransform.anchoredPosition3D = new Vector3(0,0,1);
            textComp.fontSizeMin = 1;
            textComp.fontSizeMax = 72;

            billboard.SetActive(false);

            DrawLine(startPosition, endPosition);

            createIprInfoPopup();
        }

        /// <summary>
        /// return quadrnt fo a tooth
        /// </summary>
        /// <param name="aToothNum"></param>
        /// <returns> int quadrant </returns>
        private int getQuadrant(int aToothNum) {
            if(8 >= aToothNum) {
                return 1;
            }
            if(16 >= aToothNum) {
                return 2;
            }
            if (24 >= aToothNum) {
                return 3;
            }

            return 4;
        }

        /// <summary>
        /// converts the tooth number from Universal numbering system to FDI
        /// numbering system
        /// </summary>
        /// <param name="aTooth"></param>
        /// <returns></returns>
        private string convertToFdi(string aTooth) {
            int toothNum = int.Parse(aTooth);

            int quadrant = getQuadrant(toothNum);

            int[] fdiArray = {1,2,3,4,5,6,7,8};
            if(1 == quadrant || 3 == quadrant) {
                Array.Reverse( fdiArray);
            }
            int startNum = 0;
            switch (quadrant)
            {
                case 1:
                    startNum = 1;
                    break;
                case 2:
                    startNum = 9;
                    break;
                case 3:
                    startNum = 17;
                    break;
                case 4:
                    startNum = 25;
                    break;
                
                default:
                    break;
            }

            int fdiNum = fdiArray[toothNum - startNum];

            return quadrant.ToString() + fdiNum.ToString();

        }

        private void createIprInfoPopup() {

            if("0" == maxStage) return;
            // Creating ipr popup
            iprInfoPopupObj = new GameObject("IPR Info Popup");
            GameObject iprInfoPopupTextObj = new GameObject("IPR Info Popup text");
            GameObject iprInfoPopupPointerObj = new GameObject("IPR Info Popup pointer");
            iprInfoPopupObj.transform.SetParent(billboard.transform, false);

            // Creatiing canvas
            Canvas canvas = iprInfoPopupObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;

            // popup text config
            iprInfoPopupTextObj.transform.SetParent(canvas.transform, false);
            iprInfoPopupTextObj.transform.localRotation = Quaternion.Euler(0, 180, 0);
            TextMeshPro textComp = iprInfoPopupTextObj.AddComponent<TextMeshPro>();
            textComp.enableAutoSizing = true;
            textComp.rectTransform.anchoredPosition3D = new Vector3(0,0,1);
            textComp.fontSizeMin = 1;
            textComp.fontSizeMax = 72;
            textComp.alignment = TextAlignmentOptions.Center;

            string toothNum1 = name.Replace("ipr-", "").Split("-")[0].Split("_")[1];
            string toothNum2 = name.Replace("ipr-", "").Split("-")[1].Split("_")[1];

            // string teeth = tooth1.Split("_")[1] + " & " + tooth2.Split("_")[1];
            string teeth = convertToFdi(toothNum1) + " & " + convertToFdi(toothNum2);
    
            textComp.text = $"IPR {maxValue} mm before aligner {maxStage} between teeth {teeth}";

            // popup image config
            Image image = iprInfoPopupObj.AddComponent<Image>();
            image.color = new Color(0.1698113F, 0.1626023F, 0.1626023F);

            // popup pointer
            iprInfoPopupPointerObj.transform.SetParent(canvas.transform, false);
            Image pointerImage = iprInfoPopupPointerObj.AddComponent<Image>();
            pointerImage.color = new Color(0.1698113F, 0.1626023F, 0.1626023F);
            iprInfoPopupPointerObj.transform.localRotation = Quaternion.Euler(0, 0, 45f);
            pointerImage.rectTransform.anchoredPosition3D = new Vector3(0,-3.5f,0);
            pointerImage.rectTransform.sizeDelta = new Vector2(2.5f, 2.5f);

            // popup rectTransform config
            RectTransform canvasRectTranform = iprInfoPopupObj.GetComponent<RectTransform>();
            canvasRectTranform.anchoredPosition3D = new Vector3(0, 7, 0);
            canvasRectTranform.sizeDelta = new Vector2(25f, 8f);

            iprInfoPopupObj.SetActive(false);

        }

        public void toggleIprInfo(bool aSate) {
            iprInfoPopupObj.SetActive(aSate);
        }

        private void DrawLine(Vector3 start, Vector3 end){
            line.AddComponent<LineRenderer>();
            MeshCollider coll = line.AddComponent<MeshCollider>();

            // Collider coll = line.GetComponent<Collider>();

            line.transform.SetParent(meshRoot.transform, false);
            line.transform.localPosition = Vector3.zero;

            LineRenderer lr = line.GetComponent<LineRenderer>();

            Mesh mesh = new Mesh();
            lr.BakeMesh(mesh, true);
            coll.sharedMesh = mesh;

            lr.startWidth = 0.2f;
            lr.useWorldSpace = false;
            Vector3[] pointPositions = {start, end};
            lr.SetPositions(pointPositions);
            line.SetActive(false);
        }
        public void toggle(bool aState){
            if(this.value == "0") {
                billboard.SetActive(false);
                line.SetActive(false);
                // isActive = false;
                return;
            }
            billboard.SetActive(aState);
            line.SetActive(aState);
            // isActive = aState;
        }

        public void setIprIsActive(bool aState){
            this.isActive = aState;
        }


        public void Dispose(){
            UnityEngine.Object.Destroy(billboard);
            UnityEngine.Object.Destroy(line);
        }

        // getter function 

        public GameObject getLine(){
            return this.line;
        }
        public bool iprIsActive(){
            return isActive;
        }

        public Vector3 getStartPos() {
            return startPosition;
        }
        public Vector3 getEndPos() {
            return endPosition;
        }

        /// returns billboard position in world space
        public Vector3 getBillboardPos() {
            return billboard.transform.position;
        }

        public Vector3 getDirection() {
            return direction;
        }

        public string getJawType() {
            return jawType;
        }

        public float getDefaultLineLength() {
            return defaultLineLength;
        }

        public Vector3 getInitialEndPos() {
            return initialEndPos;
        }

    }
}

