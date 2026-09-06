# 좀비냥 컴퍼니
<img width="500" height="500" alt="Image" src="https://github.com/user-attachments/assets/e6ae64c8-bc48-44c7-838d-15ca7be08207" />

## ✏️ 한 줄 소개
게임 회사 운영, 디펜스를 합친 생존 게임

## 📄 개요
- 프로젝트 기간 : 2026.01 ~ 2026.05
- 개발 인원 : 1명 (아트 외주 제외)
- 장르 : 3D, 캐주얼, 성장
- 다운로드 : x
- 시연 영상 : https://www.youtube.com/watch?v=pnnl-7sGSjw

## 🖥️ 기술 스택
- Language : C#
- Engine : Unity 6000.3.10.f1
- Tools : Visual Studio 2026, GitHub DeskTop, Notion, Excel

## 🛠️ 주요 기능
- Isometric 맵
- 캐릭터 이동
- 직원 관리
- 게임 개발
- 상인
- 이벤트 시스템
- 디펜스
- 엔딩

## 🔧 구현 내용
### 1. 직원 이동

폴더 위치 : Assets/@Scripts/Controllers/Cat/Movement
- CatMover : 직원 캐릭터의 이동 및 이동 상태 관리
- IMovementStrategy : 직원 이동 방식에 대한 공통 인터페이스
- PathMovement : 지정된 경로를 따라 이동하는 처리
- SeatMovement : 직원의 좌석 이동 처리
- WaitMovement : 대기 상태에서의 이동 처리
- FenceTargetMovementStrategy : 방어 시 울타리 위치를 목표로 이동하는 처리

### 2. 이벤트 시스템

폴더 위치 :Assets/@Scripts/YearEvents
- YearEventManager : 연차 이벤트의 선택과 실행을 관리
- YearEventData : 이벤트가 가지는 정적 데이터를 정의
- YearEventConditionData : 이벤트가 발생할 수 있는 조건을 데이터로 정의
- YearEventActionData : 이벤트가 발생했을 때 수행할 행동을 정의

## 📌 참조
사용 에셋 : Rookiss에 캐릭터 spine 외주 에셋, 제미나이 생성한 이미지 
