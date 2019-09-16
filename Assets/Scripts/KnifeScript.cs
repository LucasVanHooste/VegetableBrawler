using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;

public class KnifeScript : MonoBehaviour
{
    [SerializeField] private int _damage=0;
    [SerializeField] private float _timeBeforeRise=0;
    [SerializeField] private Vector2 _slashTimeRange = Vector2.zero;
    [SerializeField] private int _amountOfSlashes=0;

    [HideInInspector]public bool IsSlashing = false;
    [HideInInspector] public bool DoesDamage = false;
    [HideInInspector] public bool Shake = false;

    private Animator _animator;

    private int _rise = Animator.StringToHash("Rise");
    private int _descend = Animator.StringToHash("Descend");
    private int _slash = Animator.StringToHash("Slash");
    private int _hoverSpeed = Animator.StringToHash("HoverSpeed");

    private float _originalHoverSpeed;
    private Coroutine _slashRoutine;

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        _slashRoutine = StartCoroutine(SlashCoroutine());
    }
    

    // Update is called once per frame
    void Update()
    {
        if (IsSlashing)
        {
            _animator.SetFloat(_hoverSpeed, Mathf.Lerp(_animator.GetFloat(_hoverSpeed), 0, .1f));
        }
        else
        {
            _animator.SetFloat(_hoverSpeed, Mathf.Lerp(_animator.GetFloat(_hoverSpeed), 1, .04f));
        }

        if (Shake)
        {
            CameraScript.Shake(.2f);
            StartCoroutine(Rumble());
        }
    }

    private IEnumerator Rumble()
    {
        GamePad.SetVibration(PlayerIndex.One, .8f, .8f);
        GamePad.SetVibration(PlayerIndex.Two, .8f, .8f);
        yield return new WaitForSeconds(.5f);
        GamePad.SetVibration(PlayerIndex.One, 0, 0);
        GamePad.SetVibration(PlayerIndex.Two, 0, 0);
    }

    private IEnumerator SlashCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(_timeBeforeRise);
            Rise();

            float _slashTime = Random.Range(_slashTimeRange.x, _slashTimeRange.y);
            int counter = 0;

            while (counter < _amountOfSlashes)
            {
                yield return new WaitForSeconds(_slashTime);
                Slash();
                _slashTime = Random.Range(_slashTimeRange.x, _slashTimeRange.y);
                counter++;
            }


            yield return new WaitForSeconds(2f);
            Descend();
        }
    }

    private void Rise()
    {
        _animator.SetTrigger(_rise);
    }

    private void Descend()
    {
        _animator.SetTrigger(_descend);
    }

    private void Slash()
    {
       _animator.SetTrigger(_slash);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!DoesDamage) return; 

        PlayerScript player = collision.collider.GetComponent<PlayerScript>();
        if (player)
        {
            player.TakeDamage(_damage, collision.GetContact(0).point);
        }
    }
}
