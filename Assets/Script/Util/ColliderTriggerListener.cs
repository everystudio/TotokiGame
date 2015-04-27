using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ColliderTriggerListener : MonoBehaviour {

	List<Collider> m_ColliderList = new List<Collider>();
	void OnTriggerEnter(Collider _collider ){
		//m_ColliderList.Add (_collider);

		Debug.Log ("OnTriggerEnter:" + _collider.gameObject);
		return;
	}

	void OnTriggerStay( Collider _collider ){
		Debug.Log ("OnTriggerStay:"+_collider.gameObject);
	}

	void OnCollisionEnter(Collision collision) {
		Debug.Log ("OnCollisionEnter:" + collision.gameObject);
	}

}



