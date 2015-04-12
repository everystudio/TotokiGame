using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteAnimation : SpriteAnimationBase {

	// Use this for initialization
	void Start () {
		m_SpriteRenderer = GetComponent<SpriteRenderer> ();

		Init (m_SpriteRenderer);
	
	}
}
