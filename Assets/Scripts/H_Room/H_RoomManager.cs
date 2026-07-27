using Spine.Unity;
using UnityEngine;

public class H_RoomManager : MonoBehaviour
{
    [SerializeField] private SkeletonAnimation[] girlsSpineAnimations;
    [SerializeField] private GameObject[] obj_girls_h;

    private readonly float[] animationSpeeds = { 1f, 2f, 3f };
    private int currentGirlIndex;
    private int[] nextAnimationIndices;
    private int currentSpeedIndex;
    private void Start()
    {
        SceneLoader.HideLoadingScreen();
    }
    private void Awake()
    {
        int girlCount = Mathf.Min(girlsSpineAnimations.Length, obj_girls_h.Length);

        if (girlCount <= 0)
        {
            return;
        }

        nextAnimationIndices = new int[girlCount];
        currentGirlIndex = 0;
        currentSpeedIndex = 0;
        SetActiveGirl(currentGirlIndex);
        ApplySpeedToCurrentGirl();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SwitchToNextGirl();
        }

        if (Input.GetMouseButtonDown(0))
        {
            PlayNextAnimationForCurrentGirl();
        }

        if (Input.GetMouseButtonDown(1))
        {
            CycleSpeedForCurrentGirl();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            EventCenter.Dispatch(StateEvent.EVENT_ENTER_MENU);
        }
    }

    private void SwitchToNextGirl()
    {
        int girlCount = Mathf.Min(girlsSpineAnimations.Length, obj_girls_h.Length);
        if (girlCount <= 0)
        {
            return;
        }

        currentGirlIndex = (currentGirlIndex + 1) % girlCount;
        currentSpeedIndex = 0;
        SetActiveGirl(currentGirlIndex);
        ApplySpeedToCurrentGirl();
    }

    private void SetActiveGirl(int activeIndex)
    {
        for (int i = 0; i < obj_girls_h.Length; i++)
        {
            obj_girls_h[i].SetActive(i == activeIndex);
        }
    }

    private void PlayNextAnimationForCurrentGirl()
    {
        int girlCount = Mathf.Min(girlsSpineAnimations.Length, obj_girls_h.Length);
        if (girlCount <= 0)
        {
            return;
        }

        SkeletonAnimation skeletonAnimation = girlsSpineAnimations[currentGirlIndex];
        if (skeletonAnimation == null)
        {
            return;
        }

        var skeletonData = skeletonAnimation.Skeleton?.Data;
        if (skeletonData == null || skeletonData.Animations == null || skeletonData.Animations.Count == 0)
        {
            return;
        }

        int animationCount = skeletonData.Animations.Count;
        int animationIndex = nextAnimationIndices[currentGirlIndex] % animationCount;
        string animationName = skeletonData.Animations.Items[animationIndex].Name;

        skeletonAnimation.timeScale = animationSpeeds[currentSpeedIndex];
        skeletonAnimation.AnimationState.SetAnimation(0, animationName, true);
        nextAnimationIndices[currentGirlIndex] = (animationIndex + 1) % animationCount;
    }

    private void CycleSpeedForCurrentGirl()
    {
        if (animationSpeeds.Length == 0)
        {
            return;
        }

        currentSpeedIndex = (currentSpeedIndex + 1) % animationSpeeds.Length;
        ApplySpeedToCurrentGirl();
    }

    private void ApplySpeedToCurrentGirl()
    {
        int girlCount = Mathf.Min(girlsSpineAnimations.Length, obj_girls_h.Length);
        if (girlCount <= 0)
        {
            return;
        }

        SkeletonAnimation skeletonAnimation = girlsSpineAnimations[currentGirlIndex];
        if (skeletonAnimation == null)
        {
            return;
        }

        skeletonAnimation.timeScale = animationSpeeds[currentSpeedIndex];
    }
}
