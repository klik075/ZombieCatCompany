# ZombieCatCompany - Manager Systems Documentation

## ?? 개요
이 문서는 ZombieCatCompany 프로젝트의 모든 Manager 클래스들의 기능과 역할을 정리한 것입니다.

---

## ?? **GameManager**
**역할**: 게임의 핵심 데이터와 상태를 관리하는 중앙 관리자

### 주요 기능
- **게임 데이터 관리**: Year(연차), Gold(골드), Food(식량), GameMode, CompanyName 등
- **게임 상태 제어**: GameState (Night/Dev/Morning/Popup/Ending)
- **이벤트 시스템**: 데이터 변경 시 자동으로 EventManager를 통해 이벤트 발생
- **게임 일시정지/재개**: 팝업 열릴 때 자동 일시정지

### 핵심 프로퍼티
```csharp
public int Year { get; set; }           // 연차 (최소 1)
public int Gold { get; set; }           // 골드 (최소 0)
public int Food { get; set; }           // 식량 (최소 0)
public EGameMode GameMode { get; set; } // 게임 모드 (Purchase/Extortion)
public EGameState GameState { get; set; } // 게임 상태
```

---

## ?? **MemberManager**
**역할**: 팀 멤버(Player 오브젝트)들의 생성, 관리, 저장/로드를 담당

### 주요 기능
- **멤버 생성/관리**: 사장과 직원들의 Player 오브젝트 생성 및 관리
- **데이터 저장/로드**: MemberData와 Player 오브젝트 간의 데이터 연동
- **멤버 검색**: 인덱스, ID를 통한 멤버 검색 기능
- **초기화**: 사장 캐릭터 자동 생성

### 핵심 메서드
```csharp
public Player GetMember(int index)                    // 인덱스로 멤버 검색
public int GetIndex(Player player)                    // 플레이어의 인덱스 반환
public void InitBoss()                               // 사장 초기화
public List<PlayerSaveData> GetSaveData()           // 저장용 데이터 생성
public void LoadFromSaveData(List<PlayerSaveData>)  // 저장된 데이터 로드
```

---

## ?? **DataManager**
**역할**: 게임의 모든 정적 데이터(JSON, ScriptableObject)를 로드하고 관리

### 주요 기능
- **ScriptableObject 로드**: GameConfig, LocalizationConfig, AdsConfig, IAPConfig
- **JSON 데이터 로드**: TextData, ItemData, MemberData, EducationData
- **데이터 검증**: 로드된 데이터의 유효성 검사
- **중앙 집중식 데이터 접근**: 모든 매니저가 참조하는 데이터 허브

### 관리하는 데이터
```csharp
public Dictionary<string, TextData> TextDict         // 텍스트 데이터
public Dictionary<int, ItemData> ItemDict            // 아이템 데이터
public Dictionary<int, MemberData> MemberDict        // 멤버 데이터
public Dictionary<int, EducationData> EducationDict  // 교육 데이터
```

---

## ?? **EducationManager**
**역할**: 멤버 교육 시스템을 관리하는 전문 매니저

### 주요 기능
- **교육 데이터 관리**: DataManager의 EducationDict를 캐싱하여 효율적 접근
- **페이지네이션 지원**: UI용 페이지별 교육 데이터 제공 (5개씩)
- **교육 가능 여부 체크**: 골드 부족, 멤버 상태(파견 중 등) 검사
- **교육 실행**: 능력치 적용 및 골드 차감 처리
- **UI 지원**: 능력치 증가 텍스트, 비용 포맷팅 등

### 핵심 API
```csharp
public List<EducationData> GetEducationsForPage(int pageIndex, int itemsPerPage = 5)
public bool CanExecuteEducation(Player member, EducationData educationData)
public bool CompleteEducation(Player member, EducationData educationData)
public string GetAbilityIncreaseText(EducationData educationData)
```

---

## ?? **UIManager**
**역할**: UI 시스템의 중앙 관리자 (Scene UI + Popup UI)

### 주요 기능
- **Scene UI 관리**: 각 씬의 메인 UI 관리 (한 번에 하나만)
- **Popup UI 스택**: 팝업들의 스택 구조 관리, 순서대로 열고 닫기
- **자동 정렬**: Canvas sortingOrder 자동 관리
- **UI Toolkit 지원**: UGUI와 UI Toolkit 동시 지원

### 핵심 메서드
```csharp
public T ShowSceneUI<T>()                    // Scene UI 표시
public T ShowPopupUI<T>()                    // Popup UI 표시 (스택에 추가)
public void ClosePopupUI()                   // 최상위 팝업 닫기
public void CloseAllPopupUI()                // 모든 팝업 닫기
```

---

## ?? **EventManager**
**역할**: 게임 전반의 이벤트 시스템을 관리하는 옵저버 패턴 구현체

### 주요 기능
- **이벤트 등록/해제**: EEventType 기반의 이벤트 시스템
- **느슨한 결합**: 컴포넌트 간 직접 참조 없이 통신 가능
- **UI 업데이트**: 데이터 변경 시 자동으로 UI 갱신 트리거

### 주요 이벤트 타입
```csharp
YearChanged, GoldChanged, FoodChanged        // 게임 데이터 변경
UI_PopupOpened, UI_PopupClosed              // UI 상태 변경
LanguageChanged                              // 언어 변경
EducationCompleted                           // 교육 완료
```

---

## ??? **ResourceManager**
**역할**: 게임 리소스(Prefab, Texture, Audio 등)의 로드와 관리

### 주요 기능
- **리소스 로드**: Resources 폴더의 모든 에셋 로드 및 캐싱
- **오브젝트 생성**: Prefab 인스턴스 생성
- **메모리 관리**: 사용하지 않는 리소스 해제
- **확장 가능**: IResourceLoader 인터페이스로 다른 로드 방식 지원 가능

### 핵심 메서드
```csharp
public T Get<T>(string key)                          // 리소스 가져오기
public GameObject Instantiate(string key)           // 오브젝트 생성
public void Destroy(GameObject go)                  // 오브젝트 제거
```

---

## ?? **SaveManager**
**역할**: 게임 데이터의 저장과 로드를 담당

### 주요 기능
- **자동 저장**: 10초 간격 자동 저장 (코루틴 사용)
- **JSON 직렬화**: Newtonsoft.Json을 사용한 데이터 저장
- **데이터 초기화**: 기본값으로 게임 리셋
- **저장 파일 관리**: persistentDataPath에 GameData.json으로 저장

### 핵심 메서드
```csharp
public void Save()                    // 즉시 저장
public void Load()                    // 저장된 데이터 로드
public void Reset()                   // 기본값으로 초기화
public void StartAutoSave()           // 자동 저장 시작
```

---

## ?? **LocalizationManager**
**역할**: 다국어 지원 시스템 관리

### 주요 기능
- **언어 전환**: 런타임에 언어 변경 가능 (KOR/ENG)
- **텍스트 관리**: TemplateID 기반 텍스트 시스템
- **폰트 관리**: 언어별 폰트 에셋 자동 적용
- **이벤트 발생**: 언어 변경 시 LanguageChanged 이벤트 발생

### 핵심 API
```csharp
public ELanguage CurrentLanguage { get; set; }       // 현재 언어
public TMP_FontAsset CurrentFontAsset { get; }       // 현재 폰트
public string GetLocalizedText(string templateID)    // 번역된 텍스트 가져오기
```

---

## ?? **SoundManager**
**역할**: 게임의 모든 오디오 시스템 관리

### 주요 기능
- **BGM/Effect 분리**: BGM과 효과음 별도 관리
- **2D/3D 사운드**: 평면 사운드와 공간 사운드 지원
- **볼륨 제어**: 사운드 타입별 볼륨 조절
- **리소스 캐싱**: AudioClip 자동 캐싱으로 성능 최적화

### 핵심 메서드
```csharp
public void Play2D(ESound type, string key, float pitch = 1.0f)
public void Play3D(string key, GameObject soundObject)
public void SetVolume(ESound type, float volume)
```

---

## ?? **ObjectManager**
**역할**: 게임 오브젝트(Player, Monster, NPC)의 스폰과 관리

### 주요 기능
- **카테고리별 관리**: Player, Monster, NPC를 별도 Root로 관리
- **오브젝트 풀링**: 성능 최적화를 위한 풀링 지원
- **생명주기 관리**: 스폰부터 디스폰까지 전체 관리
- **HashSet 추적**: 활성 오브젝트들을 HashSet으로 빠른 관리

### 핵심 메서드
```csharp
public Player SpawnPlayer(string prefab = "Player", bool pooling = false)
public void Despawn(ObjectBase obj)
```

---

## ?? **PoolManager**
**역할**: 오브젝트 풀링 시스템으로 성능 최적화

### 주요 기능
- **오브젝트 재사용**: 생성/파괴 비용 절약
- **풀별 관리**: Prefab별로 독립적인 풀 관리
- **자동 확장**: 필요 시 풀 크기 자동 확장
- **계층 정리**: 사용하지 않는 오브젝트를 별도 부모 하위로 정리

### 핵심 메서드
```csharp
public void Reserve(string prefabName, int count)     // 미리 생성
public GameObject Pop(string prefabName)             // 풀에서 가져오기
public bool Push(GameObject go)                      // 풀에 반환
```

---

## ??? **MapManager**
**역할**: 타일맵 기반 맵 시스템과 길찾기 관리

### 주요 기능
- **타일맵 분석**: Tilemap에서 이동 가능한 영역 분석
- **A* 길찾기**: 효율적인 경로 탐색 알고리즘
- **점유 시스템**: Cat들의 위치 충돌 방지
- **좌표 변환**: 셀 좌표와 월드 좌표 간 변환

### 핵심 메서드
```csharp
public bool CanMove(Vector2Int position)              // 이동 가능 여부
public List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal) // 경로 탐색
public bool MoveTo(Cat cat, Vector2Int newPosition)   // Cat 이동
```

---

## ?? **SceneManager**
**역할**: 씬 전환과 현재 씬 정보 관리

### 주요 기능
- **씬 전환**: EScene enum을 통한 타입 안전 씬 전환
- **현재 씬 추적**: 현재 활성 씬과 타입 정보 제공
- **BaseScene 연동**: 씬별 BaseScene 컴포넌트 자동 탐지

### 핵심 API
```csharp
public BaseScene CurrentScene { get; }               // 현재 씬 오브젝트
public EScene CurrentSceneType { get; }              // 현재 씬 타입
public void LoadScene(EScene sceneType)              // 씬 전환
```

---

## ? **CoroutineManager**
**역할**: 코루틴의 중앙 집중식 관리

### 주요 기능
- **코루틴 실행**: 다른 매니저들의 코루틴 실행 대행
- **생명주기 관리**: 코루틴 시작/정지 관리
- **안전성**: null 코루틴 체크 및 예외 처리
- **일괄 정리**: 필요시 모든 코루틴 일괄 정지

### 핵심 메서드
```csharp
public Coroutine Run(IEnumerator coroutine)          // 코루틴 실행
public void Stop(Coroutine coroutine)                // 특정 코루틴 정지
public void StopAll()                                // 모든 코루틴 정지
```

---

## ?? **AdsManager**
**역할**: Unity LevelPlay를 통한 광고 시스템 관리

### 주요 기능
- **전면 광고**: Interstitial 광고 표시
- **보상 광고**: Rewarded 광고와 보상 콜백 처리
- **SDK 초기화**: LevelPlay SDK 자동 초기화
- **이벤트 처리**: 광고 로드/표시/실패 등 모든 이벤트 핸들링

### 핵심 메서드
```csharp
public void ShowInterstitialAds()                    // 전면 광고
public void ShowRewardedAds(Action rewardedCallback) // 보상 광고
```

---

## ?? **IAPManager**
**역할**: Unity IAP를 통한 인앱결제 시스템 관리

### 주요 기능
- **결제 처리**: 상품 구매 및 결제 검증
- **복구**: 이전 구매 내역 복구
- **권한 확인**: 구매한 상품의 권한 상태 체크
- **이벤트 처리**: 결제 성공/실패/지연 등 모든 상황 핸들링

### 핵심 메서드
```csharp
public void Purchase(string productId, Action onPurchaseCallback)
public void RestorePurchases(Action<bool, string> onRestoreCallback)
```

---

## ??? **아키텍처 특징**

### **Singleton 패턴**
- 모든 Manager는 Singleton<T>을 상속하여 전역 접근 보장
- Instance를 통한 편리한 접근 (예: `GameManager.Instance.Gold`)

### **이벤트 기반 통신**
- EventManager를 통한 느슨한 결합
- 데이터 변경 시 자동 UI 업데이트

### **계층적 책임 분리**
- 각 Manager는 명확한 단일 책임
- 상호 의존성 최소화

### **확장성**
- 인터페이스 기반 설계 (IResourceLoader, IDataLoader 등)
- 새로운 기능 추가 시 기존 코드 최소 변경

---

## ?? **사용 가이드라인**

1. **새로운 Manager 추가 시**
   - Singleton<T> 상속
   - 명확한 단일 책임 정의
   - 필요시 IValidate 인터페이스 구현

2. **데이터 접근 시**
   - 항상 해당 Manager를 통해 접근
   - 직접적인 데이터 조작 지양

3. **이벤트 사용 시**
   - EventManager를 통한 통신 권장
   - 직접 참조보다는 이벤트 기반 통신

4. **성능 고려사항**
   - 풀링이 필요한 오브젝트는 PoolManager 사용
   - 리소스 로드는 ResourceManager를 통해 캐싱 활용

---

*마지막 업데이트: 2024년*
*작성자: GitHub Copilot*