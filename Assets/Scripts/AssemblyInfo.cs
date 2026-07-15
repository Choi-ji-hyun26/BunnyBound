using System.Runtime.CompilerServices;

// Edit Mode 테스트 어셈블리에 internal 멤버 접근을 허용
// (테스트 전용 메서드를 public으로 노출해 공개 API 표면을 흐리지 않기 위함)
[assembly: InternalsVisibleTo("BunnyBound.EditMode.Tests")]
