using UnityEngine;
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
	protected float m_fVelocityY;
	private bool m_bDir;		// 真だと右向き
	public Vector3 m_v3MoveDirection = Vector3.zero;
	public Vector3 m_v3CharaDirection = Vector3.zero;
	[SerializeField]
	private ColliderTriggerListener m_FrontTrigger;

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

	void Start(){
		start ();
	}

	public virtual void start(){
		return;
	}



	// Update is called once per frame
	void Update () {
		exe_set ();
		return;
	}

	private void exe_set(){
		move ();
		stateCheck ();
		exe ();
	}



	public virtual void move(){
		m_bIsGround = Chara.isGrounded;
		Vector3 tempMove = new Vector3 (Input.GetAxis ("Horizontal"), 0, Input.GetAxis ("Vertical"));

		EasyTouchManager.Instance.GetJoystick ("Chara", ref tempMove);

		tempMove.z = tempMove.y;
		tempMove.y = 0.0f;

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
			if (EasyTouchManager.Instance.GetButton("B-Button") == ETCAxis.AxisState.Down) {
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

		// 会話とか
		if (EasyTouchManager.Instance.GetButton("A-Button") == ETCAxis.AxisState.Down) {
			RaycastHit hitinfo;
			Vector3 fwd = transform.TransformDirection(Vector3.forward);

			fwd = m_v3CharaDirection;
			Debug.Log (m_v3CharaDirection);

			int layerMask = 1 << 10;
			if (Physics.Raycast (transform.position, fwd, out hitinfo,10.0f, layerMask )) {
//				print ("There is something in front of the object!");

				Debug.Log (hitinfo.transform.gameObject);
			}
		}




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
				m_v3CharaDirection = new Vector3 (-1.0f, 0.0f, 0.0f);

			} else if (0.01f < Chara.velocity.x) {
				m_eState = STATE.WALK_SIDE;
				m_v3CharaDirection = new Vector3 ( 1.0f, 0.0f, 0.0f);
			} else if (m_bIsGround == false && 0.01f < Chara.velocity.y) {
				m_eState = STATE.JUMP_READY;
			} else if (m_bIsGround == false && Chara.velocity.y < -0.01f) {
				m_eState = STATE.JUMP_DOWN;
				//Debug.Log (Chara.velocity.y);
			} else if (Chara.velocity.z < -0.01f) {
				m_eState = STATE.WALK_FRONT;
			} else if (0.01f < Chara.velocity.z) {
				m_eState = STATE.WALK_BACK;
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
				//Debug.Log (Chara.velocity.y);
			} else if (Chara.velocity.y < 0.0f) {
				//Debug.Log ( "zero : "+ Chara.velocity.y);
			} else {
			}
			break;
		case STATE.WALK_FRONT:
		case STATE.WALK_BACK:
			if (Chara.velocity.z == 0.0f) {
				m_eState = STATE.IDLE;
			}
			if (m_bIsGround == false &&0.01f < Chara.velocity.y) {
				m_eState = STATE.JUMP_READY;
			} else if (m_bIsGround == false &&Chara.velocity.y < -0.01f) {
				m_eState = STATE.JUMP_DOWN;
				Debug.Log (Chara.velocity.y);
			} else if (Chara.velocity.y < 0.0f) {
				//Debug.Log ( "zero : "+ Chara.velocity.y);
			} else {
			}
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

	public virtual void exe(){
		return;
	}


}




















