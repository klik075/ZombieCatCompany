using System;
using UnityEngine;

public class Projectile : ObjectBase
{
    [SerializeField] private float _speed = 10f;
    [SerializeField] private float _arcHeight = 2f; // 포물선 높이
    [SerializeField] private bool _useRotation = true; // 회전 효과 사용 여부
    [SerializeField] private float _maxLifetime = 5f; // 최대 생존 시간 (초)
    
    private NormalCat _target;
    private int _damage;
    private Action _onLaunchComplete;
    
    private Vector3 _startPosition;
    private Vector3 _targetPosition;
    private float _journeyLength;
    private float _journeyTime;
    private float _lifeTime; // 생성 후 경과 시간
    private bool _isActive;

    public void Initialize(NormalCat target, int damage, Action onLaunchComplete = null)
    {
        _target = target;
        _damage = damage;
        _onLaunchComplete = onLaunchComplete;
        
        _startPosition = transform.position;
        _targetPosition = target.transform.position;
        
        // 2D 거리 계산 (z축 무시)
        Vector2 start2D = new Vector2(_startPosition.x, _startPosition.y);
        Vector2 target2D = new Vector2(_targetPosition.x, _targetPosition.y);
        _journeyLength = Vector2.Distance(start2D, target2D);
        
        _journeyTime = 0f;
        _lifeTime = 0f; // 생존 시간 초기화
        _isActive = true;

        // 발사 방향에 따라 초기 회전 설정 (선택사항)
        if (_useRotation)
        {
            Vector2 direction = target2D - start2D;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void Update()
    {
        if (!_isActive)
            return;

        // 생존 시간 체크
        _lifeTime += Time.deltaTime;
        if (_lifeTime >= _maxLifetime)
        {
            Debug.LogWarning($"[Projectile] Lifetime exceeded {_maxLifetime}s, force despawn");
            Despawn();
            return;
        }

        // 타겟이 사라지거나 죽으면 마지막 위치로 계속 이동
        Vector3 currentTargetPos = _target != null && _target.IsAlive 
            ? _target.transform.position 
            : _targetPosition;

        // 포물선 이동
        _journeyTime += Time.deltaTime * _speed / _journeyLength;

        if (_journeyTime >= 1f)
        {
            OnHit();
            return;
        }

        // 2D 선형 보간 (z축 유지)
        Vector2 start2D = new Vector2(_startPosition.x, _startPosition.y);
        Vector2 target2D = new Vector2(currentTargetPos.x, currentTargetPos.y);
        Vector2 current2D = Vector2.Lerp(start2D, target2D, _journeyTime);
        
        Vector3 currentPos = new Vector3(current2D.x, current2D.y, transform.position.z);
        
        // 포물선 효과 (sin 곡선) - y축으로 올라감
        float arc = Mathf.Sin(_journeyTime * Mathf.PI) * _arcHeight;
        currentPos.y += arc;

        transform.position = currentPos;
        
        // 회전 효과 (Z축 회전만)
        if (_useRotation)
        {
            transform.Rotate(0, 0, 360f * Time.deltaTime * 2f);
        }
    }

    private void OnHit()
    {
        if (_target != null && _target.IsAlive)
        {
            _target.TakeDamage(_damage);
            Debug.Log($"[Projectile] Hit {_target.name} for {_damage} damage");
        }

        _onLaunchComplete?.Invoke();
        Despawn();
    }

    private void Despawn()
    {
        _isActive = false;
        _target = null;
        _onLaunchComplete = null;
        
        ObjectManager.Instance.Despawn(this);
    }

    /// <summary>
    /// 풀로 반환될 때 초기화
    /// </summary>
    public override void Init()
    {
        base.Init();
        _isActive = false;
        _target = null;
        _onLaunchComplete = null;
        _journeyTime = 0f;
        _lifeTime = 0f; // 생존 시간 초기화
        transform.rotation = Quaternion.identity;
    }
}
