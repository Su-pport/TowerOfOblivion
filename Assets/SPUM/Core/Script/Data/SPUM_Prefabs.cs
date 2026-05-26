using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

// 플레이어 상태를 정의하는 enum
public enum PlayerState
{
    IDLE,      // 대기 상태
    MOVE,      // 이동 상태
    ATTACK,    // 공격 상태
    DAMAGED,   // 피격 상태
    DEBUFF,    // 디버프 상태
    DEATH,     // 죽음 상태
    OTHER,     // 기타 상태
}

public class SPUM_Prefabs : MonoBehaviour
{
    public float _version; // 프리팹 버전 관리용
    public bool EditChk;   // 에디터에서 수정 여부 체크
    public string _code;   // 프리팹 코드(식별자)

    public Animator _anim; // 연결된 Animator
    public AnimatorOverrideController OverrideController; // 애니메이션 교체용 컨트롤러

    public string UnitType; // 유닛 타입(예: Player, Enemy 등)
    public List<SpumPackage> spumPackages = new List<SpumPackage>(); // 애니메이션 패키지 데이터
    public List<PreviewMatchingElement> ImageElement = new();        // 프리뷰 이미지 요소
    public List<SPUM_AnimationData> SpumAnimationData = new();       // 애니메이션 데이터 리스트

    // 상태별 애니메이션 리스트를 담는 딕셔너리
    public Dictionary<string, List<AnimationClip>> StateAnimationPairs = new();

    // 상태별 애니메이션 클립 리스트
    public List<AnimationClip> IDLE_List = new();
    public List<AnimationClip> MOVE_List = new();
    public List<AnimationClip> ATTACK_List = new();
    public List<AnimationClip> DAMAGED_List = new();
    public List<AnimationClip> DEBUFF_List = new();
    public List<AnimationClip> DEATH_List = new();
    public List<AnimationClip> OTHER_List = new();

    public SPUM_Prefabs spumPrefabs; // 자기 자신 참조(혹은 다른 프리팹 참조)

    // AnimatorOverrideController 초기화
    public void OverrideControllerInit()
    {
        Animator animator = _anim; // 현재 Animator 가져오기
        OverrideController = new AnimatorOverrideController(); // 새 오버라이드 컨트롤러 생성
        OverrideController.runtimeAnimatorController = animator.runtimeAnimatorController; // 기존 컨트롤러 복제

        // Animator에 등록된 모든 애니메이션 클립 가져오기
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;

        foreach (AnimationClip clip in clips)
        {
            // 오버라이드 컨트롤러에 클립 등록
            OverrideController[clip.name] = clip;
        }

        // Animator가 오버라이드 컨트롤러를 사용하도록 설정
        animator.runtimeAnimatorController = OverrideController;

        // enum의 모든 상태를 돌면서 상태별 리스트 연결
        foreach (PlayerState state in Enum.GetValues(typeof(PlayerState)))
        {
            var stateText = state.ToString(); // enum을 문자열로 변환
            StateAnimationPairs[stateText] = new List<AnimationClip>(); // 딕셔너리에 빈 리스트 생성

            // 상태 이름에 따라 해당 리스트 연결
            switch (stateText)
            {
                case "IDLE": StateAnimationPairs[stateText] = IDLE_List; break;
                case "MOVE": StateAnimationPairs[stateText] = MOVE_List; break;
                case "ATTACK": StateAnimationPairs[stateText] = ATTACK_List; break;
                case "DAMAGED": StateAnimationPairs[stateText] = DAMAGED_List; break;
                case "DEBUFF": StateAnimationPairs[stateText] = DEBUFF_List; break;
                case "DEATH": StateAnimationPairs[stateText] = DEATH_List; break;
                case "OTHER": StateAnimationPairs[stateText] = OTHER_List; break;
            }
        }
    }

    // 모든 상태 리스트에 애니메이션이 최소 1개 이상 있는지 확인
    public bool allListsHaveItemsExist()
    {
        List<List<AnimationClip>> allLists = new List<List<AnimationClip>>()
        {
            IDLE_List, MOVE_List, ATTACK_List, DAMAGED_List, DEBUFF_List, DEATH_List, OTHER_List
        };

        return allLists.All(list => list.Count > 0);
    }

    // Inspector 메뉴에서 직접 실행할 수 있는 함수
    [ContextMenu("PopulateAnimationLists")]
    public void PopulateAnimationLists()
    {
        // 상태별 리스트 초기화
        IDLE_List = new();
        MOVE_List = new();
        ATTACK_List = new();
        DAMAGED_List = new();
        DEBUFF_List = new();
        DEATH_List = new();
        OTHER_List = new();

        // spumPackages에서 애니메이션 데이터 추출 후 상태별 그룹화
        var groupedClips = spumPackages
        .SelectMany(package => package.SpumAnimationData)
        .Where(spumClip => spumClip.HasData &&
                        spumClip.UnitType.Equals(UnitType) &&
                        spumClip.index > -1)
        .GroupBy(spumClip => spumClip.StateType)
        .ToDictionary(
            group => group.Key,
            group => group.OrderBy(clip => clip.index).ToList()
        );

        // 상태별로 애니메이션 클립 로드해서 리스트에 추가
        foreach (var kvp in groupedClips)
        {
            var stateType = kvp.Key;
            var orderedClips = kvp.Value;
            switch (stateType)
            {
                case "IDLE": IDLE_List.AddRange(orderedClips.Select(clip => LoadAnimationClip(clip.ClipPath))); break;
                case "MOVE": MOVE_List.AddRange(orderedClips.Select(clip => LoadAnimationClip(clip.ClipPath))); break;
                case "ATTACK": ATTACK_List.AddRange(orderedClips.Select(clip => LoadAnimationClip(clip.ClipPath))); break;
                case "DAMAGED": DAMAGED_List.AddRange(orderedClips.Select(clip => LoadAnimationClip(clip.ClipPath))); break;
                case "DEBUFF": DEBUFF_List.AddRange(orderedClips.Select(clip => LoadAnimationClip(clip.ClipPath))); break;
                case "DEATH": DEATH_List.AddRange(orderedClips.Select(clip => LoadAnimationClip(clip.ClipPath))); break;
                case "OTHER": OTHER_List.AddRange(orderedClips.Select(clip => LoadAnimationClip(clip.ClipPath))); break;
            }
        }
    }

    // 특정 상태 애니메이션 실행
    public void PlayAnimation(PlayerState PlayState, int index)
    {
        Animator animator = _anim; // Animator 가져오기
        var animations = StateAnimationPairs[PlayState.ToString()]; // 해당 상태의 애니메이션 리스트 가져오기
        OverrideController[PlayState.ToString()] = animations[index]; // 오버라이드 컨트롤러에 애니메이션 교체

        var StateStr = PlayState.ToString(); // 상태 문자열로 변환

        // 상태에 따라 Animator Bool 파라미터 설정
        bool isMove = StateStr.Contains("MOVE");
        bool isDebuff = StateStr.Contains("DEBUFF");
        bool isDeath = StateStr.Contains("DEATH");
        animator.SetBool("1_Move", isMove);
        animator.SetBool("5_Debuff", isDebuff);
        animator.SetBool("isDeath", isDeath);

        // Move, Debuff가 아닐 때 Trigger 파라미터 확인
        if (!isMove && !isDebuff)
        {
            AnimatorControllerParameter[] parameters = animator.parameters;
            foreach (AnimatorControllerParameter parameter in parameters)
            {
                if (parameter.type == AnimatorControllerParameterType.Trigger)
                {
                    bool isTrigger = parameter.name.ToUpper().Contains(StateStr.ToUpper());
                    if (isTrigger)
                    {
                        Debug.Log($"Parameter: {parameter.name}, Type: {parameter.type}");
                        animator.SetTrigger(parameter.name); // 해당 Trigger 실행
                    }
                }
            }
        }
    }

    // Resources 폴더에서 애니메이션 클립 로드
    AnimationClip LoadAnimationClip(string clipPath)
    {
        AnimationClip clip = Resources.Load<AnimationClip>(clipPath.Replace(".anim", ""));

        if (clip == null)
        {
            Debug.LogWarning($"Failed to load animation clip '{clipPath}'.");
        }

        return clip;
    }
}
