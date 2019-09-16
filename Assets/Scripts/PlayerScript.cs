using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;

[RequireComponent(typeof(PhysicsController))]
public class PlayerScript : MonoBehaviour
{
    [Header("General")]
    [SerializeField] [Range(1, 2)] private int _playerNumber=1;
    [SerializeField] private ParticleSystem _starParticles;
    [SerializeField] private int _maxHealth = 0;
    [SerializeField] private float _flinchTime=0;
    [SerializeField] private float _knockbackForce = 10;
    [SerializeField] private bool _canControlDuringAttack = false;

    [Header("Attack fields")]
    [SerializeField] private int _attackDamage=0;
    [SerializeField] private AttackCollider[] _attackColliders;
    public float AttackDuration = 0;
    [SerializeField] private Vector2 _attackDamageTimeRange=Vector2.zero;
    [SerializeField] private bool _useAttackMotion=false;

    [Header("Special attack fields")]
    [SerializeField] private int _specialAttackDamage=0;
    [SerializeField] private AttackCollider[] _specialAttackColliders;
    public float SpecialAttackDuration = 0;
    [SerializeField] private Vector2 _specialAttackDamageTimeRange=Vector2.zero;
    [SerializeField] private bool _useSpecialAttackMotion=false;

    public int MaxHealth { get => _maxHealth; }
    public int Health { get; private set; }
    public int PlayerNumber { get => _playerNumber; set => _playerNumber=value; }

    private Transform _transform;
    private PhysicsController _physicsController;
    private Animator _animator;
    private AnimationsController _animationsController;
    private ParticleSystem _particleSystem;

    public float AttackCooldownTimer { get; set; }
    public float SpecialAttackCooldownTimer { get; set; }
    private Coroutine _generalAttackCoroutine;

    private bool _isFlinched=false;
    private bool _isDead;

    private int _mapLayer;
    private bool _wasGrounded;
    private float _timer;
    void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Player");
        _transform = transform;
        _physicsController = GetComponent<PhysicsController>();
        _animator = GetComponent<Animator>();
        _animationsController = new AnimationsController(_animator, _physicsController);
        _particleSystem = GetComponent<ParticleSystem>();

        Health = _maxHealth;
        AttackCooldownTimer = AttackDuration;
        SpecialAttackCooldownTimer = SpecialAttackDuration;
        
        _mapLayer = LayerMask.NameToLayer("Map");

    }

    private void Update()
    {
        if (_isDead || _physicsController==null)
        {
            return;
        }


        if (!_isFlinched)
        {
            if(_generalAttackCoroutine == null || _canControlDuringAttack)
            {
                _physicsController.InputMovement = new Vector3(InputController.GetHorizontalMovement(_playerNumber), 0, 0);

                if (InputController.IsJumpButtonPressed(_playerNumber) && _physicsController.IsGrounded())
                {
                    _physicsController.Jump = true;
                }
                _timer = 0;
                TryAttack();
                TrySpecialAttack();
            }
            else
            {
                _timer += Time.deltaTime;
                Debug.Log(_timer);
                _physicsController.InputMovement = Vector3.zero;
            }

        }
        else
        {
            Debug.Log("FLINCHED");
            _physicsController.InputMovement = Vector3.zero;
        }
        //_flinchTimer += Time.deltaTime;

        if (CheckIfPlayerLands())
        {
            _particleSystem.Play(false);
        }
        _wasGrounded = _physicsController.IsGrounded();
        _animationsController.Update();
    }

    private bool CheckIfPlayerLands()
    {
        return _physicsController.IsGrounded() && !_wasGrounded;
    }

    private void OnTriggerStay(Collider col)
    {
        if(col.gameObject.layer== _mapLayer)
        UseAnimationMotion(false);
    }

    private void TryAttack()
    {
        if (_generalAttackCoroutine==null && InputController.IsAttackButtonPressed(_playerNumber))
        {
            Attack();
        }
        AttackCooldownTimer += Time.deltaTime;
    }

    private void TrySpecialAttack()
    {
        if (_generalAttackCoroutine == null && InputController.IsSpecialAttackButtonPressed(_playerNumber))
        {
            SpecialAttack();
        }
        SpecialAttackCooldownTimer += Time.deltaTime;
    }

    private void Attack()
    {
        StartAttackCoroutine(TryAttackDamageOpponent());

        UseAnimationMotion(_useAttackMotion);
        _animationsController.Attack(true);
        AttackCooldownTimer = 0;
    }

    private void SpecialAttack()
    {
        StartAttackCoroutine(TrySpecialAttackDamageOpponent());

        UseAnimationMotion(_useSpecialAttackMotion);
        _animationsController.SpecialAttack(true);
        SpecialAttackCooldownTimer = 0;
    }

    private void UseAnimationMotion(bool useMotion)
    {
        if (!useMotion && _physicsController.IsKinematic)
            _physicsController.Velocity = Vector3.zero;

        _physicsController.IsKinematic = useMotion;
        _animationsController.ApplyRootMotion(useMotion);

    }

    private void StartAttackCoroutine(IEnumerator attack)
    {
        if (_generalAttackCoroutine != null)
            StopCoroutine(_generalAttackCoroutine);

        _generalAttackCoroutine = StartCoroutine(attack);
    }

    private IEnumerator TryAttackDamageOpponent()
    {
        Debug.Log("ATTACK");
        float timer = 0;
        while (timer < _attackDamageTimeRange.x)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        bool hasLandedHit = false;

        while (timer < _attackDamageTimeRange.y)
        {
            if (!hasLandedHit)
            {
                foreach (AttackCollider attackCollider in _attackColliders)
                {
                    PlayerScript opponent = attackCollider.Opponent;
                    if (opponent != null)
                    {
                        StartCoroutine(RumbleAttack());
                        opponent.TakeDamage(_attackDamage, attackCollider.HitOrigin);
                        hasLandedHit = true;
                        FixedTime.FreezeTime(.15f);
                        CameraScript.Shake(.1f);
                        break;
                    }
                }
            }

            timer += Time.deltaTime;
            yield return null;
        }

        while (timer < AttackDuration)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        _generalAttackCoroutine = null;
        Debug.Log("finished");
        _animationsController.Attack(false);
        UseAnimationMotion(false);
    }

    private IEnumerator TrySpecialAttackDamageOpponent()
    {
        Debug.Log("SPECIAL ATTACK");
        float timer = 0;
        while (timer < _specialAttackDamageTimeRange.x)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        bool hasLandedHit = false;

        while (timer < _specialAttackDamageTimeRange.y)
        {
            if (!hasLandedHit)
            {
                foreach (AttackCollider specialAttackCollider in _specialAttackColliders)
                {
                    PlayerScript opponent = specialAttackCollider.Opponent;
                    if (opponent != null)
                    {
                        StartCoroutine(RumbleAttack());
                        opponent.TakeDamage(_specialAttackDamage, specialAttackCollider.HitOrigin);
                        hasLandedHit = true;
                        FixedTime.FreezeTime(.2f);
                        CameraScript.Shake(.15f);
                        break;
                    }
                }
            }

            timer += Time.deltaTime;
            yield return null;
        }

        while (timer < SpecialAttackDuration)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        _generalAttackCoroutine = null;
        _animationsController.SpecialAttack(false);
        UseAnimationMotion(false);
    }

    public void TakeDamage(int damage, Vector3 origin)
    {
        if (_generalAttackCoroutine != null)
        {
            UseAnimationMotion(false);
            StopCoroutine(_generalAttackCoroutine);
            _generalAttackCoroutine = null;
        }

        if (_starParticles)
            _starParticles.Play();

        //_flinchTimer = 0;
        StartCoroutine(RumbleDamage());
        StartCoroutine(Flinch());
        _physicsController.TakeKnockBack(_knockbackForce, origin);
        _animationsController.TakeDamage();
        Health -= damage;
        Die();
        Debug.Log("DAMAGE");
    }

    private IEnumerator Flinch()
    {
        _isFlinched = true;
        yield return new WaitForSeconds(_flinchTime);

        _isFlinched = false;
        _animationsController.Recover();
    }

    private IEnumerator RumbleAttack()
    {
        GamePad.SetVibration(IntToPlayerIndex(_playerNumber), .1f, .1f);
        yield return new WaitForSeconds(_flinchTime);
        GamePad.SetVibration(IntToPlayerIndex(_playerNumber), 0, 0);
    }

    private IEnumerator RumbleDamage()
    {
        GamePad.SetVibration(IntToPlayerIndex(_playerNumber), .5f, .5f);
        yield return new WaitForSeconds(_flinchTime);
        GamePad.SetVibration(IntToPlayerIndex(_playerNumber), 0, 0);
    }

    private void Die()
    {
        if (Health <= 0)
        {
            _isDead = true;
            gameObject.layer = LayerMask.NameToLayer("NoCollisionWithPlayer");
            _animationsController.Die();
            GameControllerScript.Instance.EndGame(_playerNumber);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(transform.position + Vector3.up, transform.forward);
    }

    private PlayerIndex IntToPlayerIndex(int playerNumber)
    {
        switch (playerNumber)
        {
            case 1: return PlayerIndex.One;
            case 2: return PlayerIndex.Two;
            case 3: return PlayerIndex.Three;
            case 4: return PlayerIndex.Four;
            default: return PlayerIndex.One;
        }            

    }

    public void PlayStartAnimation()
    {
        if (UnityEngine.Random.Range(0, 2) > 0)
        {
            _animationsController.Attack(true);
            StartCoroutine(StopAttack());
        }
        else
        {
            _animationsController.SpecialAttack(true);
            StartCoroutine(StopSpeciaAttack());
        }
    }

    private IEnumerator StopAttack()
    {
        yield return new WaitForSeconds(AttackDuration);
        _animationsController.Attack(false);
    }
    private IEnumerator StopSpeciaAttack()
    {
        yield return new WaitForSeconds(SpecialAttackDuration);
        _animationsController.SpecialAttack(false);
    }
}
