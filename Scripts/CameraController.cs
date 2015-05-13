using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour 
{
	
	public KinectWrapper.NuiSkeletonPositionIndex TrackedJoint = KinectWrapper.NuiSkeletonPositionIndex.HandRight;
	public float zFactor = 10.0f;
	public float xFactor = -10.0f;
	public float distanceFromKinectInMeters = 2.0f;
	public bool flipScreen = false;
	public float cameraHeight = 7.0f;





	
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

					Debug.Log(posJoint);
					if(posJoint.z < 0.5f) return;

					Vector3 meanLastPosition = getPositionsMean(lastHeadPositions, 4);

					if(lastHeadPositions.Count > 8 && Vector3.Distance(meanLastPosition, posJoint) > 0.8f) {
						// outlier
						//return;
					}

					lastHeadPositions.Add (posJoint);
					if (lastHeadPositions.Count >= 10) {
						lastHeadPositions.RemoveAt(0);
					}

					//Vector3 camPos = getPositionsMean(lastHeadPositions, 3);
					Vector3 camPos = posJoint;

					//Debug.Log(camPos.z * 10.0f);

					camPos.z = (camPos.z*10.0f - (17.5f * (2.0f/distanceFromKinectInMeters))) * zFactor;

					//posJoint.Normalize();

					//posJoint.x *= xFactor;
					//posJoint.z *= zFactor;
					//camPos.x *= -1.0f;
					//posJoint.z -= 0.9f;

					float cameraHeightFactor = 0.8f;
					//cameraHeightFactor = 1.0f;

					//Camera.main.transform.position = new Vector3 (camPos.x * xFactor * (-1.0f) * cameraHeightFactor, cameraHeight, camPos.z);

					if(flipScreen) {
						Camera.main.transform.position = new Vector3 (camPos.z * (-1.0f) * cameraHeightFactor, cameraHeight, camPos.x * xFactor * cameraHeightFactor);
					} else {
						Camera.main.transform.position = new Vector3 (camPos.x * xFactor * (-1.0f) * cameraHeightFactor, cameraHeight, camPos.z * (-1.0f) * cameraHeightFactor);
					}

					doThis ();
/*
					GameObject[] allObjects = GameObject.FindGameObjectsWithTag("Untagged") ;
					foreach(GameObject go in allObjects){
							float distance = Vector3.Distance (Camera.main.transform.position, go.transform.position);
							go.transform.localScale *= Camera.main.transform.position.y/distance;
					}*/
				}
				
			}
			
		}
	}




	public GameObject projectionScreen;
	public bool  estimateViewFrustum = true;

	










	void  doThis (){
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








	
	
	public Transform[] Corners;
	public Transform lookTarget;
	/*
	public bool drawNearCone, drawFrustum;
	public float near = 10.0f;
	
	//Camera theCam;
	/*
	void Start () {
		theCam = camera;Camera cam = Camera.main;
	}
	void Update() {
		//Camera.main.transform.position = new Vector3 (Camera.main.transform.position.x, 6.0f, 0.0f);
		UpdateMainCamera (Camera.main.transform.position);
	}
	
	void UpdateMainCamera (Vector3 pe) {
		Vector3 pa, pb, pc, pd;
		pa = Corners[0].position; //Bottom-Left
		pb = Corners[1].position; //Bottom-Right
		pc = Corners[2].position; //Top-Left
		pd = Corners[3].position; //Top-Right

		//Debug.Log (pe);
		
		Vector3 vr = ( pb - pa ).normalized; // right axis of screen
		Vector3 vu = ( pc - pa ).normalized; // up axis of screen
		Vector3 vn = Vector3.Cross( vr, vu ).normalized; // normal vector of screen
		
		Vector3 va = pa - pe; // from pe to pa
		Vector3 vb = pb - pe; // from pe to pb
		Vector3 vc = pc - pe; // from pe to pc
		Vector3 vd = pd - pe; // from pe to pd
		
		//float n = -lookTarget.InverseTransformPoint( Camera.main.transform.position ).z; // distance to the near clip plane (screen)
		float n = near;
		float f = Camera.main.farClipPlane; // distance of far clipping plane
		float d = Vector3.Dot( va, vn ); // distance from eye to screen
		float l = Vector3.Dot( vr, va ) * n / d; // distance to left screen edge from the 'center'
		float r = Vector3.Dot( vr, vb ) * n / d; // distance to right screen edge from 'center'
		float b = Vector3.Dot( vu, va ) * n / d; // distance to bottom screen edge from 'center'
		float t = Vector3.Dot( vu, vc ) * n / d; // distance to top screen edge from 'center'
		
		Matrix4x4 p = new Matrix4x4(); // Projection matrix
		p[0, 0] = 2.0f * n / ( r - l );
		p[0, 2] = ( r + l ) / ( r - l );
		p[1, 1] = 2.0f * n / ( t - b );
		p[1, 2] = ( t + b ) / ( t - b );
		p[2, 2] = ( f + n ) / ( n - f );
		p[2, 3] = (2.0f * f * n / ( n - f )) * 0.5f;
		p[3, 2] = -1.0f;
		
		Camera.main.projectionMatrix = p; // Assign matrix to camera
		
		if ( drawNearCone ) { //Draw lines from the camera to the corners f the screen
			Debug.DrawRay( pe, va, Color.blue );
			Debug.DrawRay( pe, vb, Color.blue );
			Debug.DrawRay( pe, vc, Color.blue );
			Debug.DrawRay( pe, vd, Color.blue );
		}
		
		if ( drawFrustum ) DrawFrustum( Camera.main ); //Draw actual camera frustum
		
	}
	
	Vector3 ThreePlaneIntersection ( Plane p1, Plane p2, Plane p3 ) { //get the intersection point of 3 planes
		return ( ( -p1.distance * Vector3.Cross( p2.normal, p3.normal ) ) +
		        ( -p2.distance * Vector3.Cross( p3.normal, p1.normal ) ) +
		        ( -p3.distance * Vector3.Cross( p1.normal, p2.normal ) ) ) /
			( Vector3.Dot( p1.normal, Vector3.Cross( p2.normal, p3.normal ) ) );
	}
	
	void DrawFrustum ( Camera cam ) {
		Vector3[] nearCorners = new Vector3[4]; //Approx'd nearplane corners
		Vector3[] farCorners = new Vector3[4]; //Approx'd farplane corners
		Plane[] camPlanes = GeometryUtility.CalculateFrustumPlanes( cam ); //get planes from matrix
		Plane temp = camPlanes[1]; camPlanes[1] = camPlanes[2]; camPlanes[2] = temp; //swap [1] and [2] so the order is better for the loop
		
		for ( int i = 0; i < 4; i++ ) {
			nearCorners[i] = ThreePlaneIntersection( camPlanes[4], camPlanes[i], camPlanes[( i + 1 ) % 4] ); //near corners on the created projection matrix
			farCorners[i] = ThreePlaneIntersection( camPlanes[5], camPlanes[i], camPlanes[( i + 1 ) % 4] ); //far corners on the created projection matrix
		}
		
		for ( int i = 0; i < 4; i++ ) {
			Debug.DrawLine( nearCorners[i], nearCorners[( i + 1 ) % 4], Color.red, Time.deltaTime, false ); //near corners on the created projection matrix
			Debug.DrawLine( farCorners[i], farCorners[( i + 1 ) % 4], Color.red, Time.deltaTime, false ); //far corners on the created projection matrix
			Debug.DrawLine( nearCorners[i], farCorners[i], Color.red, Time.deltaTime, false ); //sides of the created projection matrix
		}
	}

*/






}
