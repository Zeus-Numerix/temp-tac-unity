using System.Collections;
using System.Linq;
using System.Collections.Generic;
using IPRObjectNameSpace;
using Ipr;
using UnityEngine;

public class IprHoverHandler : MonoBehaviour
{
    private List<IPR> iprs;
    // private List<IPR> lowerIprs;

    // Start is called before the first frame update
    void Start()
    {
        iprs = IPRObject.upperIprs;

        iprs.AddRange(IPRObject.lowerIprs);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseEnter()
    {
        // Debug.Log("Mouse entered " + gameObject.name);

        IPR ipr = iprs.FirstOrDefault(ipr => gameObject.name == ipr.name);

        ipr.toggleIprInfo(true);
    }

    private void OnMouseExit()
    {
        // Debug.Log("Mouse exited " + gameObject.name);
        IPR ipr = iprs.FirstOrDefault(ipr => gameObject.name == ipr.name);
        ipr.toggleIprInfo(false);
    }
}
