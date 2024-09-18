using UnityEngine;
using System.Collections;

public class QT_SimpleRotate : MonoBehaviour {

    public bool XAxis, YAxis, ZAxis;
    
    public float Speed = 25f;
    private Vector3 rotationAxis = new Vector3(0, 0, 0);
    

	void Start () {

        if (XAxis)
            rotationAxis += new Vector3(1, 0, 0);
        if (YAxis)
            rotationAxis += new Vector3(0, 1, 0);
        if (ZAxis)
            rotationAxis += new Vector3(0, 0, 1);
	}

	void FixedUpdate()
	{

		if(rotationAxis!=Vector3.zero)    
			this.transform.Rotate((rotationAxis * Time.deltaTime)*Speed);
	}
	
    
}
