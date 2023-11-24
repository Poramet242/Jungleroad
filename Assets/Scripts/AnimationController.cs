using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    #region Singleton
    public static AnimationController Instance { get { return _instance; } }
    private static AnimationController _instance;
    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
        {
            Destroy(this.gameObject);
            return;
        }
    }
    #endregion
    // Get value from server and Play animation
    [SerializeField] Animator animator;
    public string currentState;
    public bool isForward;
    public class AnimationListState
    {
        public const string IDLE = "Idle";
        public const string JUMP_UP = "Jump_forward";
        public const string JUMP_DOWN = "Jump_back";
        public const string DIE_HIT = "Die_hit";
        public const string DIE_WATER = "Die_water";
        public const string DIE_TIME = "Die_time";
        public const string DIE_CROCODILE = "Die_crocodile";
    }

    public void ChangeAnimationState(string newState)
    {
        if(newState == "Jump_forward" || newState == "Jump_back")
        {
            animator.SetTrigger(newState);
            if (newState == "Jump_forward")
                isForward = true;
            else
                isForward = false;
            return;
        }
        if (currentState == newState)
            return;
        animator.Play(newState);
        currentState = newState;
    }

    private void Start()
    {
        currentState = AnimationListState.IDLE;
    }

}
