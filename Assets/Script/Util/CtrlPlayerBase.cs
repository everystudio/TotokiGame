﻿using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(CtrlAnimation))]
public class CtrlPlayerBase : MonoBehaviourEx {

	public enum STATE
	{
		IDLE			= 0,
		WALK_SIDE		,
		WALK_FRONT		,
		WALK_BACK		,
		RUN				,
		JUMP_READY		,
		JUMP_UP			,
		JUMP_TOP		,
		JUMP_DOWN		,
		JUMP_RANDING	,
	};
	public STATE m_eState;
	public STATE m_eStatePre;

	public float m_fSpeed  = 15.0f;
	public float m_fJumpSpeed = 25.0f;
	public float GRAVITY = 50.0f;
	private float m_fVelocityY;
	private bool m_bDir;		// 真だと右向き
	public Vector3 m_v3MoveDirection = Vector3.zero;

	public bool m_bIsGround;

	public CtrlAnimation m_CtrlAnimation;
	public CtrlAnimation ctrlAnimation{
		get{
			if (m_CtrlAnimation == null) {
				m_CtrlAnimation = GetComponent<CtrlAnimation> ();
			}
			return m_CtrlAnimation;
		}
	}

	public CharacterController m_CharacterController;
	public CharacterController Chara{
		get{ 
			if (m_CharacterController == null) {
				m_CharacterController = GetComponent<CharacterController> ();
			}
			return m_CharacterController;
		}
	}

	// Update is called once per frame
	void Update () {
		move ();
		stateCheck ();
		return;
	}

	public virtual void move(){
		m_bIsGround = Chara.isGrounded;
		Vector3 tempMove = new Vector3 (Input.GetAxis ("Horizontal"), 0, Input.GetAxis ("Vertical"));
		tempMove = tempMove.normalized;
		tempMove *= m_fSpeed;

		if (0.0f < tempMove.x) {
			if (m_bDir == false) {
				myTransform.eulerAngles = new Vector3 (0.0f, 180.0f, 0.0f);
			}
			m_bDir = true;
		} else if (tempMove.x < 0.0f) {
			if (m_bDir == true) {
				myTransform.eulerAngles = new Vector3 (0.0f, 0.0f, 0.0f);
			}
			m_bDir = false;
		} else {
		}

		//地面についているかどうか
		if( Chara.isGrounded ){
			//移動方向を取得
			//ジャンプ
			if (Input.GetButton ("Jump")) {
				m_fVelocityY = m_fJumpSpeed;
			}
		} else {
			// 重力を計算
			m_fVelocityY -= GRAVITY * Time.deltaTime;
		}
		m_v3MoveDirection.x = tempMove.x;
		m_v3MoveDirection.z = tempMove.z;
		m_v3MoveDirection.y = m_fVelocityY;

		// 移動
		Chara.Move(m_v3MoveDirection * Time.deltaTime);
		m_fVelocityY = m_v3MoveDirection.y;
	}

	public virtual void stateCheck(){

		bool bInit = false;
		if (m_eStatePre != m_eState) {
			m_eStatePre  = m_eState;
			ctrlAnimation.SetMotion (m_eState);
			bInit = true;
		}

		switch (m_eState) {
		case STATE.IDLE:
			if (Chara.velocity.x < -0.01f) {
				m_eState = STATE.WALK_SIDE;
			} else if (0.01f < Chara.velocity.x) {
				m_eState = STATE.WALK_SIDE;
			} else if ( m_bIsGround == false && 0.01f < Chara.velocity.y) {
				m_eState = STATE.JUMP_READY;
			} else if (m_bIsGround == false && Chara.velocity.y < -0.01f) {
				m_eState = STATE.JUMP_DOWN;
				Debug.Log (Chara.velocity.y);
			} else if (Chara.velocity.y < 0.0f) {
				Debug.Log ( "zero : "+ Chara.velocity.y);
			} else {
			}
			break;
		case STATE.WALK_SIDE:
			if (Chara.velocity.x == 0.0f) {
				m_eState = STATE.IDLE;
			}
			if (m_bIsGround == false &&0.01f < Chara.velocity.y) {
				m_eState = STATE.JUMP_READY;
			} else if (m_bIsGround == false &&Chara.velocity.y < -0.01f) {
				m_eState = STATE.JUMP_DOWN;
				Debug.Log (Chara.velocity.y);
			} else if (Chara.velocity.y < 0.0f) {
				Debug.Log ( "zero : "+ Chara.velocity.y);
			} else {
			}

			break;
		case STATE.WALK_FRONT:
			break;
		case STATE.WALK_BACK:
			break;
		case STATE.JUMP_READY:
			m_eState = STATE.JUMP_UP;
			break;
		case STATE.JUMP_UP:
			if (Chara.velocity.y <= 0.0f) {
				m_eState = STATE.JUMP_TOP;
			}
			break;
		case STATE.JUMP_TOP:
			if (ctrlAnimation.IsEnd ()) {
				m_eState = STATE.JUMP_DOWN;
			}
			break;
		case STATE.JUMP_DOWN:
			if (Chara.isGrounded) {
				m_eState = STATE.JUMP_RANDING;
				//m_eState = STATE.IDLE;
			}
			break;
		case STATE.JUMP_RANDING:
			if (ctrlAnimation.IsEnd ()) {
				m_eState = STATE.IDLE;
			}
			break;
		default:
			break;
		}



	}


}




















