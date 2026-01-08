using UnityEngine;
using Spine.Unity;

[RequireComponent(typeof(SkeletonAnimation))]
public class Cat : ObjectBase
{
    public enum EAnimation
    {
        b_wait,
        b_walk,
        f_wait,
        f_walk
    }

    public enum ECatState
    {
        Idle,
        Move,
        Work
    }

    private SkeletonAnimation _skeletonAnimation;
    private ECatState _state;
    private bool _isFacingForward = true; // true for front, false for back
    private bool _isFlipped = false;

    public Vector2Int CellPosition { get; set; }

    public ECatState State
    {
        get { return _state; }
        set { _state = value; UpdateAnimation(); }
    }

    public bool IsFacingForward
    {
        get { return _isFacingForward; }
        set { _isFacingForward = value; UpdateAnimation(); }
    }

    public bool IsFlipped
    {
        get { return _isFlipped; }
        set { _isFlipped = value; _skeletonAnimation.skeleton.ScaleX = _isFlipped ? -1 : 1; }
    }

    void Start()
    {
        _skeletonAnimation = GetComponent<SkeletonAnimation>();
            
		State = ECatState.Idle; // Initialize to Idle
    }

    // Update is called once per frame
    public virtual void Update()
    {
		// Isometric sorting for spine renderer
		if (_skeletonAnimation != null)
		{
			Renderer renderer = _skeletonAnimation.GetComponent<Renderer>();
			if (renderer != null)
			{
				renderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100f);
			}
		}
	}

    public void PlayAnimation(EAnimation animation)
    {
        if (_skeletonAnimation != null)
        {
            _skeletonAnimation.AnimationState.SetAnimation(0, animation.ToString(), true);
        }
    }

    private void UpdateAnimation()
    {
        EAnimation animation;
        string prefix = _isFacingForward ? "f_" : "b_" ;

        switch (_state)
        {
            case ECatState.Idle:
                animation = _isFacingForward ? EAnimation.f_wait : EAnimation.b_wait;
                break;
            case ECatState.Move:
                animation = _isFacingForward ? EAnimation.f_walk : EAnimation.b_walk;
                break;
            case ECatState.Work:
                // Assuming Work uses wait animation, adjust if needed
                animation = _isFacingForward ? EAnimation.f_wait : EAnimation.b_wait;
                break;
            default:
                animation = EAnimation.f_wait;
                break;
        }

        PlayAnimation(animation);
    }
}
