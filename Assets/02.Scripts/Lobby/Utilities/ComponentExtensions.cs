using UnityEngine;
/*
 * 확장함수 정의 방법
 * static 클래스 내부에 첫번째 파라미터에 this 키워드가 붙은 static 함수를 정의
 */
namespace HideAndSkull.Lobby.Utilities
{
    public static class ComponentExtensions
    {
        public static Transform FindChildReculsively(this Component component, string childName)
        {
            foreach (Transform child in component.transform)
            {
                if (child.name.Equals(childName))
                {
                    return child;
                }
                else
                {
                    Transform grandChild = FindChildReculsively(child, childName);

                    if (grandChild)
                        return grandChild;
                }
            }

            return null;
        }
    }
}