using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Animator))]
[RequireComponent (typeof(CapsuleCollider))]
[RequireComponent (typeof(Rigidbody))]
public class PlayerController : MonoBehaviour 
{
	public enum CharacterMovemnts {
		AllDirections,
		Sideways
	}
	public CharacterMovemnts movementType = CharacterMovemnts.AllDirections;

	Rigidbody _rb;
	CapsuleCollider _capCol;
	Animator _anim;
	[SerializeField] PhysicMaterial _zFriction;
	[SerializeField] PhysicMaterial _mFriction;
	Transform _cam;

	[SerializeField] float _speed = .8f;
	[SerializeField] float _turnSpeed = 5f;
	[SerializeField] float _jumpPower = 5f;

	Vector3 _directionPos;
	Vector3 _storeDir;

	float _horizontal;
	float _vertical;
	bool _jumpInput;
	bool _onGround = true;
	

	void Awake ()
	{
		_rb = GetComponent<Rigidbody>();
		_capCol = GetComponent<CapsuleCollider>();
		_cam = Camera.main.transform;

		SetupAnimator();
	}

	void Update ()
	{
		HandleFriction();
	}

	void FixedUpdate ()
	{
		_horizontal = Input.GetAxis("Horizontal");
		_vertical = Input.GetAxis("Vertical");
		_jumpInput = Input.GetButtonDown("Jump");

//		if (_horizontal == 0f)
//		{
			_storeDir = _cam.right;
//		}

		if (_onGround)
		{
			if (movementType == CharacterMovemnts.AllDirections) {
				_rb.AddForce(((_storeDir * _horizontal) + (_cam.forward * _vertical)) * _speed / Time.deltaTime);
			}
			else if (movementType == CharacterMovemnts.Sideways) {
				_rb.AddForce((_storeDir * _horizontal) * _speed / Time.deltaTime);
			}

			if (_jumpInput)
			{
				_anim.SetTrigger("Jump");
				_rb.AddForce(Vector3.up * _jumpPower, ForceMode.Impulse);
			}
		}

		//Find the position in front of where the camera is looking
		if (movementType == CharacterMovemnts.AllDirections) {
			_directionPos = transform.position + (_storeDir * _horizontal) + (_cam.forward * _vertical);
		}
		else if (movementType == CharacterMovemnts.Sideways) {
			_directionPos = transform.position + (_storeDir * _horizontal);
		}


		//Find the direction from that position
		Vector3 dir = _directionPos - transform.position;
		dir.y = 0f;

		//Turn input into anim values.
		float animValue = 0f;

		if (movementType == CharacterMovemnts.AllDirections) {
			animValue = Mathf.Abs(_horizontal) + Mathf.Abs(_vertical);
		}
		else if (movementType == CharacterMovemnts.Sideways) {
			animValue = Mathf.Abs(_horizontal);
		}

		_anim.SetFloat("Forward", animValue, .1f, Time.deltaTime);


		if (_horizontal != 0f || _vertical != 0f)
		{
			if (movementType == CharacterMovemnts.AllDirections) {
				
			}
			else if (movementType == CharacterMovemnts.Sideways) {
				if (_vertical != 0f)
					return;
			}

			float angle = Quaternion.Angle(transform.rotation, Quaternion.LookRotation(dir));
			
			if (angle != 0f)
			{
				_rb.rotation = Quaternion.Slerp(_rb.rotation, Quaternion.LookRotation(dir), _turnSpeed * Time.deltaTime);
			}
		}
	}

	void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.CompareTag("Floor"))
		{
			_onGround = true;
			_anim.SetBool("onAir", false);
			_rb.drag = 5f;
		}
	}

	void OnCollisionExit (Collision collision)
	{
		if (collision.gameObject.CompareTag("Floor"))
		{
			_onGround = false;
			_anim.SetBool("onAir", true);
			_rb.drag = 0f;
		}
	}

	void SetupAnimator ()
	{
		_anim = GetComponent<Animator> ();

		foreach (var childAnim in GetComponentsInChildren<Animator>()) {

			if (childAnim != _anim)
			{
				_anim.avatar = childAnim.avatar;
				Destroy(childAnim);
				break;
			}
		}
	}

	void HandleFriction ()
	{
		if (_horizontal == 0f && _vertical == 0f)
		{
			_capCol.material = _mFriction;
		}
		else
		{
			_capCol.material = _zFriction;
		}
	}
}
