// This script handles the change of projection relative to the viewer perspective.
// It should be a component of the main camera.

using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour 
{
	
	public KinectWrapper.NuiSkeletonPositionIndex TrackedJoint = KinectWrapper.NuiSkeletonPositionIndex.HandRight;
	public float zFactor = 10.0f;
	public float xFactor = -10.0f;
	public float distanceFromKinectInMeters = 2.0f;
	public float cameraHeight = 7.0f;
	public float cameraHeightFactor = 0.8f;
	public GameObject projectionScreen;
	public bool estimateViewFrustum = true;
	public Transform[] Corners;
	public Transform lookTarget;

	void Start()
	{
	}

	private ArrayList lastHeadPositions = new ArrayList();

	private Vector3 getPositionsMean(ArrayList positions, int N) {
		Vector3 sum = new Vector3(0.0f, 0.0f, 0.0f);
		int toSumQuantity = Mathf.Min(N, positions.Count);
		if (toSumQuantity == 0)
			return sum;
		
		for(int i=0; i<toSumQuantity; i++) {
			sum = sum + (Vector3)positions[i];
		}
		
		return sum / (float)toSumQuantity;
	}

	void Update() 
	{
		if (Input.GetKeyDown (KeyCode.F))
			Screen.fullScreen = !Screen.fullScreen;

		KinectManager manager = KinectManager.Instance;
		
		if(manager && manager.IsInitialized())
		{

			int iJointIndex = (int)TrackedJoint;
			
			if(manager.IsUserDetected())
			{
				uint userId = manager.GetPlayer1ID();
				
				if(manager.IsJointTracked(userId, iJointIndex))
				{
					Vector3 posJoint = manager.GetRawSkeletonJointPos(userId, iJointIndex);

					// Ignore positions too close to the Kinect
					if(posJoint.z < 0.5f) return;

					// Smooth signal by using averaging
					Vector3 meanLastPosition = getPositionsMean(lastHeadPositions, 4);

					if(lastHeadPositions.Count > 8 && Vector3.Distance(meanLastPosition, posJoint) > 0.8f) {
						// Handle outlier
					}

					// Save the most recent head coordinates
					lastHeadPositions.Add (posJoint);
					if (lastHeadPositions.Count >= 10) {
						lastHeadPositions.RemoveAt(0);
					}

					Vector3 camPos = posJoint;

					camPos.z = (camPos.z*10.0f - (17.5f * (2.0f/distanceFromKinectInMeters))) * zFactor;

					Camera.main.transform.position = new Vector3 (camPos.x * xFactor * (-1.0f) * cameraHeightFactor, cameraHeight, camPos.z * (-1.0f) * cameraHeightFactor);

					changePerspective ();
				}
				
			}
			
		}
	}

	// Code for the perspective projection transformation was extracted from
	// http://en.wikibooks.org/wiki/Cg_Programming/Unity/Projection_for_Virtual_Reality
	// and altered to fit this environment
	void  changePerspective (){
		if (null != projectionScreen)
		{

			Vector3 pa = Corners[0].position; //Bottom-Left
			Vector3 pb = Corners[1].position; //Bottom-Right
			Vector3 pc = Corners[2].position; //Top-Left

			Vector3 pe = Camera.main.transform.position;

			// eye position
			float n = GetComponent<Camera>().nearClipPlane;
			// distance of near clipping plane
			float f = GetComponent<Camera>().farClipPlane;
			// distance of far clipping plane
			
			Vector3 va; // from pe to pa
			Vector3 vb; // from pe to pb
			Vector3 vc; // from pe to pc
			Vector3 vr; // right axis of screen
			Vector3 vu; // up axis of screen
			Vector3 vn; // normal vector of screen
			
			float l; // distance to left screen edge
			float r; // distance to right screen edge
			float b; // distance to bottom screen edge
			float t; // distance to top screen edge
			float d; // distance from eye to screen 
			
			vr = pb - pa;
			vu = pc - pa;
			vr.Normalize();
			vu.Normalize();
			vn = -Vector3.Cross(vr, vu); 
			// we need the minus sign because Unity 
			// uses a left-handed coordinate system
			vn.Normalize();
			
			va = pa - pe;
			vb = pb - pe;
			vc = pc - pe;
			
			d = -Vector3.Dot(va, vn);
			l = Vector3.Dot(vr, va) * n / d;
			r = Vector3.Dot(vr, vb) * n / d;
			b = Vector3.Dot(vu, va) * n / d;
			t = Vector3.Dot(vu, vc) * n / d;
			
			Matrix4x4 p = new Matrix4x4(); // projection matrix 
			p[0,0] = 2.0f*n/(r-l); 
			p[0,1] = 0.0f; 
			p[0,2] = (r+l)/(r-l); 
			p[0,3] = 0.0f;
			
			p[1,0] = 0.0f; 
			p[1,1] = 2.0f*n/(t-b); 
			p[1,2] = (t+b)/(t-b); 
			p[1,3] = 0.0f;
			
			p[2,0] = 0.0f;         
			p[2,1] = 0.0f; 
			p[2,2] = (f+n)/(n-f); 
			p[2,3] = 2.0f*f*n/(n-f);
			
			p[3,0] = 0.0f;         
			p[3,1] = 0.0f; 
			p[3,2] = -1.0f;        
			p[3,3] = 0.0f;		
			
			Matrix4x4 rm = new Matrix4x4(); // rotation matrix;
			rm[0,0] = vr.x; 
			rm[0,1] = vr.y; 
			rm[0,2] = vr.z; 
			rm[0,3] = 0.0f;	
			
			rm[1,0] = vu.x; 
			rm[1,1] = vu.y; 
			rm[1,2] = vu.z; 
			rm[1,3] = 0.0f;	
			
			rm[2,0] = vn.x; 
			rm[2,1] = vn.y; 
			rm[2,2] = vn.z; 
			rm[2,3] = 0.0f;	
			
			rm[3,0] = 0.0f;  
			rm[3,1] = 0.0f;  
			rm[3,2] = 0.0f;  
			rm[3,3] = 1.0f;		
			
			Matrix4x4 tm = new Matrix4x4(); // translation matrix;
			tm[0,0] = 1.0f; 
			tm[0,1] = 0.0f; 
			tm[0,2] = 0.0f; 
			tm[0,3] = -pe.x;	
			
			tm[1,0] = 0.0f; 
			tm[1,1] = 1.0f; 
			tm[1,2] = 0.0f; 
			tm[1,3] = -pe.y;	
			
			tm[2,0] = 0.0f; 
			tm[2,1] = 0.0f; 
			tm[2,2] = 1.0f; 
			tm[2,3] = -pe.z;	
			
			tm[3,0] = 0.0f; 
			tm[3,1] = 0.0f; 
			tm[3,2] = 0.0f; 
			tm[3,3] = 1.0f;		
			
			// set matrices
			GetComponent<Camera>().projectionMatrix = p;
			GetComponent<Camera>().worldToCameraMatrix = rm * tm; 
			// The original paper puts everything into the projection 
			// matrix (i.e. sets it to p * rm * tm and the other 
			// matrix to the identity), but this doesn't appear to 
			// work with Unity's shadow maps.
			
			if (estimateViewFrustum)
			{
				// rotate camera to screen for culling to work
				Quaternion q = new Quaternion();
				q.SetLookRotation((0.5f * (pb + pc) - pe), vu); 
				// look at center of screen
				GetComponent<Camera>().transform.rotation = q;
				
				// set fieldOfView to a conservative estimate 
				// to make frustum tall enough
				if (GetComponent<Camera>().aspect >= 1.0f)
				{ 
					GetComponent<Camera>().fieldOfView = Mathf.Rad2Deg * 
						Mathf.Atan(((pb-pa).magnitude + (pc-pa).magnitude) 
						           / va.magnitude);
				}
				else 
				{
					// take the camera aspect into account to 
					// make the frustum wide enough 
					GetComponent<Camera>().fieldOfView = 
						Mathf.Rad2Deg / GetComponent<Camera>().aspect *
							Mathf.Atan(((pb-pa).magnitude + (pc-pa).magnitude) 
							           / va.magnitude);
				}	
			}
		}
	}

}
