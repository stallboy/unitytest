using UnityEngine;

public class DollyZoom : MonoBehaviour
{
    public Transform target;
    public Camera myCamera;
    
    private float initViewPointY;
    public bool dzEnabled;
    
    void StartDZ()
    {
        initViewPointY = myCamera.WorldToViewportPoint(target.position).y;
        dzEnabled = true;
    }

    void Start()
    {
        StartDZ();
    }

    void Update()
    {
        if (dzEnabled)
        {
            var plane = new Plane(myCamera.transform.up, myCamera.transform.position);
            float hh = plane.GetDistanceToPoint(target.position) / initViewPointY;
            var clipplane = new Plane(-myCamera.transform.forward, target.position);
            float dist = clipplane.GetDistanceToPoint(myCamera.transform.position);
            myCamera.fieldOfView = Mathf.Abs( 2.0f * Mathf.Atan(hh / dist) * Mathf.Rad2Deg );
        }
        transform.Translate(Input.GetAxis("Vertical") * Vector3.forward * Time.deltaTime * 5f);
    }

}